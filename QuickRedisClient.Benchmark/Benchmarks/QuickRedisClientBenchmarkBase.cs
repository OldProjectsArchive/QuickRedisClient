using JustBenchmark;
using System.Threading.Tasks;

namespace QuickRedisClient.Benchmark.Benchmarks {
	internal abstract class QuickRedisClientBenchmarkBase {
		protected RedisObject _key = "quickRedisClientKey";
		protected RedisObject _value = "quickRedisClientValue";
		protected IRedisClient _client;
		private volatile byte[] _resultBytes;

		[ParallelBenchmark(
			BenchmarkConstants.StringSetIteration,
			BenchmarkConstants.StringSetParallelDegree)]
		[Benchmark(BenchmarkConstants.StringSetIteration)]
		public void Set() {
			_client.Set(_key, _value);
		}

		[ParallelTaskBenchmark(
			BenchmarkConstants.StringSetIteration,
			BenchmarkConstants.StringSetParallelDegree)]
		[TaskBenchmark(BenchmarkConstants.StringSetIteration)]
		public Task SetAsync() {
			return _client.SetAsync(_key, _value);
		}

		[ParallelBenchmark(
			BenchmarkConstants.StringGetIteration,
			BenchmarkConstants.StringGetParallelDegree)]
		[Benchmark(BenchmarkConstants.StringGetIteration)]
		public void Get() {
			_resultBytes = (byte[])_client.Get(_key);
		}

		[ParallelTaskBenchmark(
			BenchmarkConstants.StringGetIteration,
			BenchmarkConstants.StringGetParallelDegree)]
		[TaskBenchmark(BenchmarkConstants.StringGetIteration)]
		public Task GetAsync() {
			return _client.GetAsync(_key);
		}
	}
}
