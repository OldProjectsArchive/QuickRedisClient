using QuickRedisClient.Internal;
using QuickRedisClient.Thread.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace QuickRedisClient.Thread {
	/// <summary>
	/// Redis client based on thread without pipeline support
	/// TODO: reduce connections that become idle
	/// TODO: support retry command
	/// </summary>
	internal class ThreadRedisClient : IRedisClient {
		private RedisClientConfiguration _configuration;
		private HashSet<ThreadRedisClientConnection> _connections;
		private ThreadRedisClientConnection _freeConnections;
		private object _connectionsLock;
		private int _minConnections;
		private int _maxConnections;
		private int _lastConnectionId;
		private int _lastUsedConnectionIndex;
		private List<EndPoint> _remoteEPs;
		private int _lastUsedRemoteEPIndex;

		public ThreadRedisClient(RedisClientConfiguration configuration) {
			_configuration = configuration;
			_connections = new HashSet<ThreadRedisClientConnection>();
			_connectionsLock = new object();
			_minConnections = Math.Max(configuration.MinConnection, 1);
			_maxConnections = Math.Max(configuration.MaxConnection, configuration.MinConnection);
			_lastConnectionId = 0;
			_lastUsedConnectionIndex = 0;
			_remoteEPs = configuration.ServerAddresses
				.Select(a => ObjectConverter.StringToEndPoint(a)).ToList();
			_lastUsedRemoteEPIndex = 0;
			SpawnMinConnections();
		}

		public void Dispose() {
			lock (_connectionsLock) {
				foreach (var connection in _connections) {
					connection.Dispose();
				}
				_connections.Clear();
				_freeConnections = null;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void PushFreeConnection_NonThreadSafe(ThreadRedisClientConnection connection) {
			connection._nextFree = _freeConnections;
			_freeConnections = connection;
			if (connection._nextFree == null) {
				// notify other thread there new free connection
				Monitor.Pulse(_connectionsLock);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ThreadRedisClientConnection PopFreeConnection_NonThreadSafe() {
			var connection = _freeConnections;
			_freeConnections = connection?._nextFree;
			return connection;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ThreadRedisClientConnection SpawnOneConnection_NonThreadSafe(bool pushFree) {
			if (_connections.Count >= _maxConnections) {
				return null;
			}
			var connectionId = Interlocked.Increment(ref _lastConnectionId);
			var remoteEPIndex = Interlocked.Increment(
				ref _lastUsedRemoteEPIndex) % _remoteEPs.Count;
			var connection = new ThreadRedisClientConnection(
				this, connectionId, _remoteEPs[remoteEPIndex], _configuration);
			_connections.Add(connection);
			if (pushFree) {
				PushFreeConnection_NonThreadSafe(connection);
			}
			return connection;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SpawnMinConnections() {
			lock (_connectionsLock) {
				for (int from = _connections.Count, to = _minConnections; from < to; ++from) {
					SpawnOneConnection_NonThreadSafe(pushFree: true);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void RemoveOneConnection_NonThreadSafe(ThreadRedisClientConnection connection) {
			_connections.Remove(connection);
			if (connection == _freeConnections) {
				_freeConnections = connection._nextFree;
			} else {
				var free = _freeConnections;
				while (free != null) {
					if (free._nextFree == connection) {
						free._nextFree = connection._nextFree;
						break;
					}
					free = free._nextFree;
				}
			}
			DebugLogger.Log("conection {0} removed", connection.ConnectionId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ThreadRedisClientConnection AcquireConnection() {
			lock (_connectionsLock) {
				while (true) {
					// grab free connection
					var connection = PopFreeConnection_NonThreadSafe();
					if (connection != null) {
						return connection;
					}
					// no free connection, try create one
					connection = SpawnOneConnection_NonThreadSafe(pushFree: false);
					if (connection != null) {
						return connection;
					}
					// failed, just wait for new free connection
					Monitor.Wait(_connectionsLock);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void ReleaseConnection(ThreadRedisClientConnection connection) {
			lock (_connectionsLock) {
				// add connection to free list
				PushFreeConnection_NonThreadSafe(connection);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ReleaseErrorConnection(
			ThreadRedisClientConnection connection, Exception ex) {
			lock (_connectionsLock) {
				DebugLogger.Log("redis connection {0} error: {1}",
					connection.ConnectionId, ex.ToString());
				RemoveOneConnection_NonThreadSafe(connection);
			}
		}

		public void Set(RedisObject key, RedisObject value) {
			var keyBytes = (byte[])key;
			var valueBytes = (byte[])value;
			var connection = AcquireConnection();
			try {
				connection.EnsureConnected();
				ThreadSetMessage.Send(connection._client,
					connection._sendbuf, keyBytes, valueBytes);
				ThreadGetMessage.Recv(connection._client,
					connection._recvbuf, ref connection._recvBufStart, ref connection._recvBufEnd);
				ReleaseConnection(connection);
			} catch (Exception ex) {
				ReleaseErrorConnection(connection, ex);
				throw;
			}
		}

		public Task SetAsync(RedisObject key, RedisObject value) {
			return Task.Factory.StartNew(() => Set(key, value));
		}

		public RedisObject Get(RedisObject key) {
			var keyBytes = (byte[])key;
			var connection = AcquireConnection();
			try {
				connection.EnsureConnected();
				ThreadGetMessage.Send(connection._client,
					connection._sendbuf, keyBytes);
				var result = ThreadGetMessage.Recv(connection._client,
					connection._recvbuf, ref connection._recvBufStart, ref connection._recvBufEnd);
				ReleaseConnection(connection);
				return result;
			} catch (Exception ex) {
				ReleaseErrorConnection(connection, ex);
				throw;
			}
		}

		public Task<RedisObject> GetAsync(RedisObject key) {
			return Task.Factory.StartNew(() => Get(key));
		}

		public bool Del(RedisObject key) {
			var keyBytes = (byte[])key;
			var connection = AcquireConnection();
			try {
				connection.EnsureConnected();
				ThreadDelMessage.Send(connection._client,
					connection._sendbuf, keyBytes);
				var result = ThreadDelMessage.Recv(connection._client,
					connection._recvbuf, ref connection._recvBufStart, ref connection._recvBufEnd);
				ReleaseConnection(connection);
				return result;
			} catch (Exception ex) {
				ReleaseErrorConnection(connection, ex);
				throw;
			}
		}

		public Task<bool> DelAsync(RedisObject key) {
			return Task.Factory.StartNew(() => Del(key));
		}

		public long DelMany(IList<RedisObject> keys) {
			if (keys.Count == 0) {
				// avoid "ERR wrong number of arguments for 'del' command"
				return 0;
			}
			var connection = AcquireConnection();
			try {
				connection.EnsureConnected();
				ThreadDelManyMessage.Send(connection._client,
					connection._sendbuf, keys);
				var result = ThreadDelManyMessage.Recv(connection._client,
					connection._recvbuf, ref connection._recvBufStart, ref connection._recvBufEnd);
				ReleaseConnection(connection);
				return result;
			} catch (Exception ex) {
				ReleaseErrorConnection(connection, ex);
				throw;
			}
		}

		public Task<long> DelManyAsync(IList<RedisObject> keys) {
			return Task.Factory.StartNew(() => DelMany(keys));
		}
	}
}
