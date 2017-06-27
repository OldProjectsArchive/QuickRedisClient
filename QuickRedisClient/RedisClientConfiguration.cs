namespace QuickRedisClient {
	/// <summary>
	/// Redis client configuration
	/// Some implementation may not support all options
	/// </summary>
	public class RedisClientConfiguration {
		/// <summary>
		/// Min connections
		/// </summary>
		public int MinConnection { get; set; }
		/// <summary>
		/// Max connections
		/// </summary>
		public int MaxConnection { get; set; }
		/// <summary>
		/// Redis server ip address
		/// </summary>
		public string IpAddress { get; set; }
		/// <summary>
		/// Redis server port
		/// </summary>
		public int Port { get; set; }
		/// <summary>
		/// Send buffer capacity
		/// </summary>
		public int SendBufferCapacity { get; set; }
		/// <summary>
		/// Receive buffer capacity
		/// </summary>
		public int RecvBufferCapacity { get; set; }
		/// <summary>
		/// Use tcp nodelay option
		/// </summary>
		public bool UseTcpNoDelay { get; set; }

		/// <summary>
		/// Initialize
		/// </summary>
		public RedisClientConfiguration() : this("127.0.0.1", 6379) {
		}

		/// <summary>
		/// Initialize
		/// </summary>
		public RedisClientConfiguration(string ipAddress, int port) {
			MinConnection = 10;
			MaxConnection = 30;
			IpAddress = ipAddress;
			Port = port;
			SendBufferCapacity = 1024;
			RecvBufferCapacity = 1024;
			UseTcpNoDelay = false;
		}
	}
}
