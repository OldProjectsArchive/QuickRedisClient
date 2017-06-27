using System;
using System.Threading.Tasks;

namespace QuickRedisClient.LibuvPipeline {
	/// <summary>
	/// Redis client based on libuv with pipeline support
	/// </summary>
	internal class LibuvPipelineRedisClient : IRedisClient {
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
