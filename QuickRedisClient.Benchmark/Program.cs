using QuickRedisClient.Benchmark.Benchmarks;
using QuickRedisClient.Thread;
using System;
using System.Threading.Tasks;

namespace QuickRedisClient.Benchmark {
	class Program {
		static void Benchmark() {
			var benchmark = new JustBenchmark.JustBenchmark();
			benchmark.Run(new StackExchangeBenchmark());
			benchmark.Run(new QuickThreadRedisClientBenchmark());
			benchmark.Run(new QuickThreadPipelineRedisClientBenchmark());
			benchmark.Run(new QuickLibuvPipelineRedisClientBenchmark());
		}

		static void Example() {
			var factory = new ThreadRedisClientFactory();
			using (var client = factory.Create(new RedisClientConfiguration())) {
				Parallel.For(0, 100, x => {
					var key = $"testSetKey_{x}";
					var value = $"testSetValue_{x}";
					client.Set(key, value);
					if (client.Get(key) != value) {
						Console.WriteLine($"wrong value: {key}, {value}");
					}
				});
			}
			Console.WriteLine("success");
		}

		static void Main(string[] args) {
			// Example();
			Benchmark();
			Console.ReadLine();
		}
	}
}