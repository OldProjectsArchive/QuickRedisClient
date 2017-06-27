using System;

namespace QuickRedisClient.ThreadPipeline {
	/// <summary>
	/// Create redis client based on thread with pipeline support
	/// </summary>
	public class ThreadPipelineRedisClientFactory : IRedisClientFactory {
		/// <summary>
		/// Create redis client
		/// </summary>
		public IRedisClient Create() {
			throw new NotImplementedException();
		}
	}
}
