using JustBenchmark;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace QuickRedisClient.Benchmark.Benchmarks {
	class StackExchangeBenchmark {
		private RedisKey _key = "stackExchangeKey";
		private RedisValue _value = "stackExchangeValue";
		private ConnectionMultiplexer _connectionMultiplexer;
		private IDatabase _database;
		private volatile byte[] _resultBytes;
		private volatile bool _resultBool;

		public StackExchangeBenchmark() {
			_connectionMultiplexer = ConnectionMultiplexer.Connect(
				ConfigurationOptions.Parse("127.0.0.1:6379"));
			_database = _connectionMultiplexer.GetDatabase();
		}

		[ParallelBenchmark(
			BenchmarkConstants.StringSetIteration,
			BenchmarkConstants.StringSetParallelDegree)]
		[Benchmark(BenchmarkConstants.StringSetIteration)]
		public void StringSet() {
			_resultBool = _database.StringSet(_key, _value);
		}

		[ParallelTaskBenchmark(
			BenchmarkConstants.StringSetIteration,
			BenchmarkConstants.StringSetParallelDegree)]
		[TaskBenchmark(BenchmarkConstants.StringSetIteration)]
		public Task StringSetAsync() {
			return _database.StringSetAsync(_key, _value);
		}

		[ParallelBenchmark(
			BenchmarkConstants.StringGetIteration,
			BenchmarkConstants.StringGetParallelDegree)]
		[Benchmark(BenchmarkConstants.StringGetIteration)]
		public void StringGet() {
			_resultBytes = _database.StringGet(_key);
		}

		[ParallelTaskBenchmark(
			BenchmarkConstants.StringGetIteration,
			BenchmarkConstants.StringGetParallelDegree)]
		[TaskBenchmark(BenchmarkConstants.StringGetIteration)]
		public Task StringGetAsync() {
			return _database.StringGetAsync(_key);
		}
	}
}
