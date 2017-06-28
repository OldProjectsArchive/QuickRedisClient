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
	/// </summary>
	internal class ThreadRedisClient : IRedisClient {
		private RedisClientConfiguration _configuration;
		private List<ThreadRedisClientConnection> _connections;
		private object _connectionsLock;
		private int _minConnections;
		private int _maxConnections;
		private int _lastConnectionId;
		private int _lastUsedConnectionIndex;
		private List<EndPoint> _remoteEPs;
		private int _lastUsedRemoteEPIndex;

		public ThreadRedisClient(RedisClientConfiguration configuration) {
			_configuration = configuration;
			_connections = new List<ThreadRedisClientConnection>();
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

		private ThreadRedisClientConnection SpawnOneConnection() {
			if (_connections.Count >= _maxConnections) {
				return null;
			}
			var connectionId = Interlocked.Increment(ref _lastConnectionId);
			var remoteEPIndex = Interlocked.Increment(
				ref _lastUsedRemoteEPIndex) % _remoteEPs.Count;
			var connection = new ThreadRedisClientConnection(
				this, connectionId, _remoteEPs[remoteEPIndex], _configuration);
			lock (_connectionsLock) {
				if (_connections.Count >= _maxConnections) {
					connection.Dispose();
					return null;
				}
				_connections.Add(connection);
			}
			return connection;
		}

		private void SpawnMinConnections() {
			for (int from = _connections.Count, to = _minConnections; from < to; ++from) {
				SpawnOneConnection();
			}
		}

		internal void ReportConnectionDisposed(ThreadRedisClientConnection connection) {
			lock (_connectionsLock) {
				if (_connections.Remove(connection)) {
					DebugLogger.Log("conection {0} removed", connection.ConnectionId);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ThreadRedisClientConnection AcquireLockedConnection() {
			ThreadRedisClientConnection connection = null;
			var retryTimes = 0;
			var noMoreRetry = false;
			while (true) {
				lock (_connectionsLock) {
					if (_connections.Count > 0) {
						var index = Interlocked.Increment(
							ref _lastUsedConnectionIndex) % _connections.Count;
						connection = _connections[index];
					}
					noMoreRetry = retryTimes > _connections.Count;
				}
				if (connection.TryAcquireClientLock()) {
					return connection;
				}
				++retryTimes;
				if (noMoreRetry) {
					var newConnection = SpawnOneConnection();
					if (newConnection == null) {
						// already spawn maximum connections
						DebugLogger.Log(
							"force redis client {0} acquire lock", connection.ConnectionId);
						connection.AcquireClientLock();
						return connection;
					} else if (newConnection.TryAcquireClientLock()) {
						return newConnection;
					}
				}
			}
		}

		public void Dispose() {
			lock (_connectionsLock) {
				while (_connections.Count > 0) {
					_connections[_connections.Count - 1].Dispose();
				}
			}
		}

		public void Set(RedisObject key, RedisObject value) {
			var keyBytes = (byte[])key;
			var valueBytes = (byte[])value;
			var connection = AcquireLockedConnection();
			try {
				connection.EnsureConnected();
				ThreadSetMessage.Send(connection._client,
					connection._sendbuf, keyBytes, valueBytes);
				ThreadGetMessage.Recv(connection._client,
					connection._recvbuf, ref connection._recvBufStart, ref connection._recvBufEnd);
			} catch (Exception ex) {
				connection.OnError(ex);
				throw;
			} finally {
				connection.ReleaseClientLock();
			}
		}

		public Task SetAsync(RedisObject key, RedisObject value) {
			return Task.Factory.StartNew(() => Set(key, value));
		}

		public RedisObject Get(RedisObject key) {
			var keyBytes = (byte[])key;
			var connection = AcquireLockedConnection();
			try {
				connection.EnsureConnected();
				ThreadGetMessage.Send(connection._client,
					connection._sendbuf, keyBytes);
				return ThreadGetMessage.Recv(connection._client,
					connection._recvbuf, ref connection._recvBufStart, ref connection._recvBufEnd);
			} catch (Exception ex) {
				connection.OnError(ex);
				throw;
			} finally {
				connection.ReleaseClientLock();
			}
		}

		public Task<RedisObject> GetAsync(RedisObject key) {
			return Task.Factory.StartNew(() => Get(key));
		}

		public bool Del(RedisObject key) {
			var keyBytes = (byte[])key;
			var connection = AcquireLockedConnection();
			try {
				connection.EnsureConnected();
				ThreadDelMessage.Send(connection._client,
					connection._sendbuf, keyBytes);
				return ThreadDelMessage.Recv(connection._client,
					connection._recvbuf, ref connection._recvBufStart, ref connection._recvBufEnd);
			} catch (Exception ex) {
				connection.OnError(ex);
				throw;
			} finally {
				connection.ReleaseClientLock();
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
			var connection = AcquireLockedConnection();
			try {
				connection.EnsureConnected();
				ThreadDelManyMessage.Send(connection._client,
					connection._sendbuf, keys);
				return ThreadDelManyMessage.Recv(connection._client,
					connection._recvbuf, ref connection._recvBufStart, ref connection._recvBufEnd);
			} catch (Exception ex) {
				connection.OnError(ex);
				throw;
			} finally {
				connection.ReleaseClientLock();
			}
		}

		public Task<long> DelManyAsync(IList<RedisObject> keys) {
			return Task.Factory.StartNew(() => DelMany(keys));
		}
	}
}
