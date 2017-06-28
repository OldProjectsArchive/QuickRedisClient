using QuickRedisClient.Internal;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

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
		private object _clientLock;

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
			_clientLock = new object();
			DebugLogger.Log("redis connection {0} started", _connectionId);
		}

		public void Dispose() {
			_redisClient.ReportConnectionDisposed(this);
			_client?.Dispose();
			_client = null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void AcquireClientLock() {
			Monitor.Enter(_clientLock);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool TryAcquireClientLock() {
			return Monitor.TryEnter(_clientLock);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void ReleaseClientLock() {
			Monitor.Exit(_clientLock);
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void OnError(Exception ex) {
			DebugLogger.Log("redis connection {0} error: {1}", _connectionId, ex.ToString());
			Dispose();
		}
	}
}
