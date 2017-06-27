using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickRedisClient.Thread {
	/// <summary>
	/// Redis client based on thread without pipeline support
	/// </summary>
	internal class ThreadRedisClient : IRedisClient {
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
