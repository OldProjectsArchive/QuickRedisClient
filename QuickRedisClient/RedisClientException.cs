using System;

namespace QuickRedisClient {
	/// <summary>
	/// Exception thrown by redis client
	/// </summary>
	public class RedisClientException : Exception {
		/// <summary>
		/// Initialize
		/// </summary>
		public RedisClientException(string message) : base(message) {
		}

		/// <summary>
		/// Initialize
		/// </summary>
		public RedisClientException(string message, Exception innerException) :
			base(message, innerException) {
		}
	}
}
