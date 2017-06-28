using QuickRedisClient.Internal;
using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace QuickRedisClient.Thread {
	/// <summary>
	/// Single redis connection
	/// </summary>
	internal class ThreadRedisClientConnection : IDisposable {
		private static Exception _errorConnectionClosedUnexceptedly =
			new RedisClientException("Redis client error: connection closed unexceptedly");
		public int ConnectionId => _connectionId;
		private ThreadRedisClient _redisClient;
		private int _connectionId;
		private RedisClientConfiguration _configuration;
		internal byte[] _sendbuf;
		internal byte[] _recvbuf;
		internal int _recvBufStart;
		internal int _recvBufEnd;
		internal Socket _client;
		internal ThreadRedisClientConnection _next;

		public ThreadRedisClientConnection(
			ThreadRedisClient redisClient, int connectionId,
			RedisClientConfiguration configuration) {
			_redisClient = redisClient;
			_connectionId = connectionId;
			_configuration = configuration;
			_sendbuf = new byte[configuration.SendBufferCapacity];
			_recvbuf = new byte[configuration.RecvBufferCapacity];
			_recvBufStart = 0;
			_recvBufEnd = 0;
			_client = null;
			_next = null;
			DebugLogger.Log("redis connection {0} started", _connectionId);
		}

		public void Dispose() {
			_client?.Dispose();
			_recvBufStart = 0;
			_recvBufEnd = 0;
			_client = null;
			_next = null;
		}

		[MethodImpl(InlineOptimization.InlineOption)]
		internal void EnsureConnected() {
			if (_client == null) {
				var remoteEP = _redisClient.NextEndPoint();
				var client = new Socket(remoteEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				if (_configuration.UseTcpNoDelay) {
					client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
				}
				client.Connect(remoteEP);
				_client = client;
				DebugLogger.Log("redis connection {0} connected", _connectionId);
			}
		}
	}
}
