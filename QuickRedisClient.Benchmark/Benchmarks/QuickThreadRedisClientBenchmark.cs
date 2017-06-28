using QuickRedisClient.Thread;

namespace QuickRedisClient.Benchmark.Benchmarks {
	internal class QuickThreadRedisClientBenchmark : QuickRedisClientBenchmarkBase {
		public QuickThreadRedisClientBenchmark() {
			var factory = new ThreadRedisClientFactory();
			_client = factory.Create(new RedisClientConfiguration());
		}
	}
}
