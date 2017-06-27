using System;
using System.Threading.Tasks;

namespace QuickRedisClient.Thread {
	/// <summary>
	/// Redis client based on thread without pipeline support
	/// </summary>
	internal class ThreadRedisClient : IRedisClient {
		public void Set(byte[] key, byte[] value) {
			throw new NotImplementedException();
		}

		public Task SetAsync(byte[] key, byte[] value) {
			throw new NotImplementedException();
		}

		public byte[] Get(byte[] key) {
			throw new NotImplementedException();
		}

		public Task<byte[]> GetAsync(byte[] key) {
			throw new NotImplementedException();
		}

		public void Del(byte[] key) {
			throw new NotImplementedException();
		}

		public Task DelAsync(byte[] key) {
			throw new NotImplementedException();
		}

		public void Del(byte[][] keys) {
			throw new NotImplementedException();
		}

		public Task DelAsync(byte[][] keys) {
			throw new NotImplementedException();
		}

		public void Dispose() {
			throw new NotImplementedException();
		}
	}
}
