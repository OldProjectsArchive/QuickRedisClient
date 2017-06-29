using System.Collections.Generic;

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
		/// Redis server address, format: "ip:port"
		/// </summary>
		public IList<string> ServerAddresses { get; set; }
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
		/// Extra configuration
		/// </summary>
		public IDictionary<string, object> Extra { get; set; }

		/// <summary>
		/// Initialize
		/// </summary>
		public RedisClientConfiguration() : this("127.0.0.1:6379") {
		}

		/// <summary>
		/// Initialize
		/// </summary>
		public RedisClientConfiguration(string serverAddress) {
			MinConnection = 10;
			MaxConnection = 100;
			ServerAddresses = new[] { serverAddress };
			SendBufferCapacity = 1024;
			RecvBufferCapacity = 1024;
			UseTcpNoDelay = false;
			Extra = new Dictionary<string, object>();
		}
	}
}
