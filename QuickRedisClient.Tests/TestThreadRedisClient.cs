using QuickRedisClient.Thread;

namespace QuickRedisClient.Tests {
	public class TestThreadRedisClient : TestRedisClientBase {
		public override IRedisClient Client => _client;
		private IRedisClientFactory _factory;
		private IRedisClient _client;

		public TestThreadRedisClient() {
			_factory = new ThreadRedisClientFactory();
			_client = _factory.Create(new RedisClientConfiguration());
		}
	}
}
