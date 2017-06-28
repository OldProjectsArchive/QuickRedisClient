using QuickRedisClient.Thread;
using System;
using System.Threading.Tasks;

namespace QuickRedisClient.Benchmark {
	class Program {
		static void Main(string[] args) {
			var factory = new ThreadRedisClientFactory();
			var client = factory.Create(new RedisClientConfiguration());
			for (var x = 0; x < 100; ++x) {
				client.Set("abc", "xxx");
				Console.WriteLine((string)client.Get("abc"));
			}
			client.DelMany(new RedisObject[] { });
		}
	}
}