using QuickRedisClient.Internal;
using System;
using System.Threading.Tasks;
using Xunit;

namespace QuickRedisClient.Tests {
	public abstract class TestRedisClientBase {
		public const int LoopCount = 100;
		public abstract IRedisClient Client { get; }
		private Random _random = new Random();

		public TestRedisClientBase() {
			DebugLogger.EnableDebugLog = false;
		}

		[Fact]
		public void Set() {
			Parallel.For(0, 100, x => {
				var key = $"testSetKey_{x}";
				var value = $"testSetValue_{x}";
				Client.Set(key, value);
				Assert.Equal(value, Client.Get(key));
			});
		}

		[Fact]
		public async Task SetAsync() {
			for (var x = 0; x < LoopCount; ++x) {
				var key = $"testSetAsyncKey_{x}";
				var value = $"testSetAsyncValue_{x}";
				await Client.SetAsync(key, value);
				Assert.Equal(value, await Client.GetAsync(key));
			}
		}

		[Fact]
		public void Get() {
			Parallel.For(0, 100, x => {
				var key = $"testGetKey_{x}";
				var value = $"testGetValue_{x}";
				Client.Set(key, value);
				Assert.Equal(value, Client.Get(key));
			});
			var notExistKey = $"testKeyNotExist";
			Client.Del(notExistKey);
			var notExistResult = Client.Get(notExistKey);
			Assert.True(!notExistResult.HasValue);
		}

		[Fact]
		public async Task GetAsync() {
			for (var x = 0; x < LoopCount; ++x) {
				var key = $"testSetAsyncKey_{x}";
				var value = $"testSetAsyncValue_{x}";
				await Client.SetAsync(key, value);
				Assert.Equal(value, await Client.GetAsync(key));
			}
			var notExistKey = $"testKeyNotExist";
			await Client.DelAsync(notExistKey);
			var notExistResult = await Client.GetAsync(notExistKey);
			Assert.True(!notExistResult.HasValue);
		}

		[Fact]
		public void Del() {
			Parallel.For(0, 100, x => {
				var key = $"testGetKey_{x}";
				var value = $"testGetValue_{x}";
				Client.Set(key, value);
				Assert.Equal(value, Client.Get(key));
				Assert.True(Client.Del(key));
				Assert.True(!Client.Get(key).HasValue);
			});
			var notExistKey = $"testKeyNotExist";
			Client.Del(notExistKey);
			Assert.True(!Client.Del(notExistKey));
		}

		[Fact]
		public async Task DelAsync() {
			for (var x = 0; x < LoopCount; ++x) {
				var key = $"testGetKey_{x}";
				var value = $"testGetValue_{x}";
				await Client.SetAsync(key, value);
				Assert.Equal(value, await Client.GetAsync(key));
				Assert.True(await Client.DelAsync(key));
				Assert.True(!(await Client.GetAsync(key)).HasValue);
			}
			var notExistKey = $"testKeyNotExist";
			await Client.DelAsync(notExistKey);
			Assert.True(!(await Client.DelAsync(notExistKey)));
		}

		[Fact]
		public void DelMany() {
			Parallel.For(0, 100, x => {
				var key_a = $"testGetKey_{x}_a";
				var key_b = $"testGetKey_{x}_b";
				var key_c = $"testGetKey_{x}_c";
				var value = $"testGetValue_{x}";
				Client.Set(key_a, value);
				Client.Set(key_b, value);
				Client.Set(key_c, value);
				Assert.Equal(value, Client.Get(key_a));
				Assert.Equal(3, Client.DelMany(new RedisObject[] { key_a, key_b, key_c }));
				Assert.True(!Client.Get(key_a).HasValue);
				Assert.True(!Client.Get(key_b).HasValue);
				Assert.True(!Client.Get(key_c).HasValue);
			});
			var emptyKeys = new RedisObject[] { };
			Assert.Equal(0, Client.DelMany(emptyKeys));
		}

		[Fact]
		public void DelManyAsync() {

		}
	}
}
