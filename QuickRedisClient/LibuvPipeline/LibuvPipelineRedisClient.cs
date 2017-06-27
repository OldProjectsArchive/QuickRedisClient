using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickRedisClient.LibuvPipeline {
	/// <summary>
	/// Redis client based on libuv with pipeline support
	/// </summary>
	internal class LibuvPipelineRedisClient : IRedisClient {
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

		public long DelMany(IEnumerable<RedisObject> key) {
			throw new NotImplementedException();
		}

		public Task<long> DelManyAsync(IEnumerable<RedisObject> key) {
			throw new NotImplementedException();
		}

		public void Dispose() {
			throw new NotImplementedException();
		}
	}
}
