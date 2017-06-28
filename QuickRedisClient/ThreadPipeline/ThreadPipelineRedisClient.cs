using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickRedisClient.ThreadPipeline {
	/// <summary>
	/// Redis client based on thread with pipeline support
	/// </summary>
	internal class ThreadPipelineRedisClient {
		public void Set(RedisObject key, RedisObject value) {
			throw new NotImplementedException();
		}

		public Task SetAsync(RedisObject key, RedisObject value) {
			throw new NotImplementedException();
		}

		public RedisObject Get(RedisObject key) {
			throw new NotImplementedException();
		}

		public Task<RedisObject> GetAsync(RedisObject key) {
			throw new NotImplementedException();
		}

		public bool Del(RedisObject key) {
			throw new NotImplementedException();
		}

		public Task<bool> DelAsync(RedisObject key) {
			throw new NotImplementedException();
		}

		public long DelMany(IList<RedisObject> keys) {
			throw new NotImplementedException();
		}

		public Task<long> DelManyAsync(IList<RedisObject> keys) {
			throw new NotImplementedException();
		}

		public void Dispose() {
			throw new NotImplementedException();
		}
	}
}
