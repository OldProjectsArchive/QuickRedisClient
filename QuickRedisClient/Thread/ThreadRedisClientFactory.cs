using System;

namespace QuickRedisClient.Thread {
	/// <summary>
	/// Create redis client based on thread without pipeline support
	/// </summary>
	public class ThreadRedisClientFactory : IRedisClientFactory {
		/// <summary>
		/// Create redis client
		/// </summary>
		public IRedisClient Create(RedisClientConfiguration configuration) {
			return new ThreadRedisClient(configuration);
		}
	}
}
