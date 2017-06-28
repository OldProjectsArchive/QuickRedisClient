using QuickRedisClient.Internal;
using System;
using System.Net;
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
		private EndPoint _remoteEP;
		private RedisClientConfiguration _configuration;
		internal byte[] _sendbuf;
		internal byte[] _recvbuf;
		internal int _recvBufStart;
		internal int _recvBufEnd;
		internal Socket _client;
		internal ThreadRedisClientConnection _nextFree;

		public ThreadRedisClientConnection(
			ThreadRedisClient redisClient, int connectionId, EndPoint remoteEP,
			RedisClientConfiguration configuration) {
			_redisClient = redisClient;
			_connectionId = connectionId;
			_remoteEP = remoteEP;
			_configuration = configuration;
			_sendbuf = new byte[configuration.SendBufferCapacity];
			_recvbuf = new byte[configuration.RecvBufferCapacity];
			_recvBufStart = 0;
			_recvBufEnd = 0;
			_client = null;
			_nextFree = null;
			DebugLogger.Log("redis connection {0} started", _connectionId);
		}

		public void Dispose() {
			_client?.Dispose();
			_client = null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void EnsureConnected() {
			if (_client == null) {
				var client = new Socket(_remoteEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				if (_configuration.UseTcpNoDelay) {
					client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
				}
				client.Connect(_remoteEP);
				_client = client;
				DebugLogger.Log("redis connection {0} connected", _connectionId);
			}
		}
	}
}
