using System;

namespace QuickRedisClient.LibuvPipeline {
	/// <summary>
	/// Create redis client based on libuv with pipeline support
	/// </summary>
	public class LibuvPipelineRedisClientFactory : IRedisClientFactory {
		/// <summary>
		/// Create redis client
		/// </summary>
		public IRedisClient Create(RedisClientConfiguration configuration) {
			throw new NotImplementedException();
		}
	}
}
