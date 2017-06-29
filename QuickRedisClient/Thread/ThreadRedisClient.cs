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
	/// TODO: support retry command
	/// </summary>
	internal class ThreadRedisClient : IRedisClient {
		private const string RemoveIdleConnectionsIntervalKey = "RemoveIdleConnectionsInterval";
		private const int RemoveIdleConnectionsIntervalDefault = 5000;
		private RedisClientConfiguration _configuration;
		private ThreadRedisClientConnection _connections;
		private int _connectionsFreeCount;
		private int _connectionsMinFreeCount;
		private int _connectionsTotalCount;
		private int _minConnections;
		private int _maxConnections;
		private int _lastConnectionId;
		private List<EndPoint> _remoteEPs;
		private int _lastUsedRemoteEPIndex;
		private int _removeIdleConnectionsInterval;
		private Timer _removeIdleConnectionsTimer;

		public ThreadRedisClient(RedisClientConfiguration configuration) {
			_configuration = configuration;
			_connections = null;
			_connectionsFreeCount = 0;
			_connectionsMinFreeCount = 0;
			_connectionsTotalCount = 0;
			_minConnections = Math.Max(configuration.MinConnection, 1);
			_maxConnections = Math.Max(configuration.MaxConnection, configuration.MinConnection);
			_lastConnectionId = 0;
			_remoteEPs = configuration.ServerAddresses
				.Select(a => ObjectConverter.StringToEndPoint(a)).ToList();
			_lastUsedRemoteEPIndex = 0;
			if (configuration.Extra.ContainsKey(RemoveIdleConnectionsIntervalKey)) {
				_removeIdleConnectionsInterval = int.Parse(
					configuration.Extra[RemoveIdleConnectionsIntervalKey].ToString());
			} else {
				_removeIdleConnectionsInterval = RemoveIdleConnectionsIntervalDefault;
			}
			_removeIdleConnectionsTimer = new Timer(
				RemoveIdleConnections_Timer, null, 0, _removeIdleConnectionsInterval);
			SpawnMinConnections_LockFree();
		}

		public void Dispose() {
			_removeIdleConnectionsTimer.Dispose();
			while (true) {
				var connection = RemoveOneConnection_LockFree();
				if (connection == null) {
					break;
				}
			}
			_connectionsFreeCount = 0;
			_connectionsTotalCount = 0;
		}

		private void RemoveIdleConnections_Timer(object state) {
			// find out how many connections idle in this period
			var idleConnections = _connectionsMinFreeCount - _minConnections;
			if (idleConnections > 0) {
				DebugLogger.Log("remove {0} idle connections", idleConnections);
				for (var x = 0; x < idleConnections; ++x) {
					RemoveOneConnection_LockFree();
				}
			} else {
				DebugLogger.Log("no idle connections found");
			}
			// reset min free count
			_connectionsMinFreeCount = _connectionsTotalCount;
		}

		[MethodImpl(InlineOptimization.InlineOption)]
		internal EndPoint NextEndPoint() {
			var remoteEPIndex = Interlocked.Increment(ref _lastUsedRemoteEPIndex);
			remoteEPIndex = (int)((remoteEPIndex & uint.MaxValue) % _remoteEPs.Count);
			return _remoteEPs[remoteEPIndex];
		}

		[MethodImpl(InlineOptimization.InlineOption)]
		private void PushFreeConnection_LockFree(ThreadRedisClientConnection connection) {
			do {
				var head = _connections;
				connection._next = head;
				if (Interlocked.CompareExchange(ref _connections, connection, head) == head) {
					var count = Interlocked.Increment(ref _connectionsFreeCount);
					DebugLogger.Log("push free connections: {0}", count);
					return;
				}
			} while (true);
		}

		[MethodImpl(InlineOptimization.InlineOption)]
		private ThreadRedisClientConnection PopFreeConnection_LockFree() {
			do {
				var head = _connections;
				var next = head?._next;
				if (Interlocked.CompareExchange(ref _connections, next, head) == head) {
					if (head != null) {
						// head._next will update at next push
						var count = Interlocked.Decrement(ref _connectionsFreeCount);
						_connectionsMinFreeCount = Math.Min(_connectionsMinFreeCount, count);
						DebugLogger.Log("pop free connections: {0}", count);
					}
					return head;
				}
			} while (true);
		}

		[MethodImpl(InlineOptimization.InlineOption)]
		private ThreadRedisClientConnection SpawnOneConnection_LockFree(bool pushFree) {
			if (_connectionsTotalCount >= _maxConnections) {
				return null;
			}
			var connectionId = Interlocked.Increment(ref _lastConnectionId);
			var connection = new ThreadRedisClientConnection(this, connectionId, _configuration);
			var count = Interlocked.Increment(ref _connectionsTotalCount);
			if (count > _maxConnections) {
				connection?.Dispose();
				Interlocked.Decrement(ref _connectionsTotalCount);
				return null;
			}
			if (pushFree) {
				PushFreeConnection_LockFree(connection);
			}
			return connection;
		}

		[MethodImpl(InlineOptimization.InlineOption)]
		private void SpawnMinConnections_LockFree() {
			for (int from = _connectionsTotalCount, to = _minConnections; from < to; ++from) {
				SpawnOneConnection_LockFree(pushFree: true);
			}
		}

		[MethodImpl(InlineOptimization.InlineOption)]
		private ThreadRedisClientConnection RemoveOneConnection_LockFree() {
			var connection = PopFreeConnection_LockFree();
			if (connection != null) {
				connection.Dispose();
				Interlocked.Decrement(ref _connectionsTotalCount);
			}
			return connection;
		}

		[MethodImpl(InlineOptimization.InlineOption)]
		internal ThreadRedisClientConnection AcquireConnection() {
			while (true) {
				// grab free connection
				var connection = PopFreeConnection_LockFree();
				if (connection != null) {
					return connection;
				}
				// no free connection, try create one
				connection = SpawnOneConnection_LockFree(pushFree: false);
				if (connection != null) {
					return connection;
				}
				// failed, wait a moment
				// i'm not using Monitor here because it's very slow, even slow than sleep
				System.Threading.Thread.Sleep(1);
			}
		}

		[MethodImpl(InlineOptimization.InlineOption)]
		internal void ReleaseConnection(ThreadRedisClientConnection connection) {
			PushFreeConnection_LockFree(connection);
		}

		[MethodImpl(InlineOptimization.InlineOption)]
		private void ReleaseErrorConnection(
			ThreadRedisClientConnection connection, Exception ex) {
			DebugLogger.Log("redis connection {0} error: {1}",
				connection.ConnectionId, ex.ToString());
			connection.Dispose();
			PushFreeConnection_LockFree(connection);
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
