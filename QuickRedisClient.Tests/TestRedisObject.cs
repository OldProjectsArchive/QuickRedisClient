using Xunit;

namespace QuickRedisClient.Tests {
	public class TestRedisObject {
		[Fact]
		public void Equals() {
			RedisObject a = 1;
			RedisObject b = "aaa";
			RedisObject c = 1;
			RedisObject d = "aaa";
			RedisObject e = 1.0M;
			Assert.Equal(a, c);
			Assert.NotEqual(a, b);
			Assert.NotEqual(b, c);
			Assert.Equal(b, d);
			Assert.Equal(a, e);
		}

		[Fact]
		public void FromToString() {
			RedisObject value = "abc";
			Assert.Equal("abc", (string)value);
			var bytes = (byte[])value;
			Assert.Equal(3, bytes.Length);
			Assert.Equal((byte)'a', bytes[0]);
			Assert.Equal((byte)'b', bytes[1]);
			Assert.Equal((byte)'c', bytes[2]);
		}

		[Fact]
		public void FromToLongInt() {
			RedisObject value = 123;
			Assert.Equal(123, (long)value);
			Assert.Equal(123, (int)value);
			value = "-321";
			Assert.Equal(-321, (long)value);
			Assert.Equal(-321, (int)value);
		}

		[Fact]
		public void FromToDoubleFloat() {
			RedisObject value = 123.1;
			Assert.Equal((123.1).ToString(), ((double)value).ToString());
			Assert.Equal((123.1).ToString(), ((float)value).ToString());
			value = "-321.3";
			Assert.Equal((-321.3).ToString(), ((double)value).ToString());
			Assert.Equal((-321.3).ToString(), ((float)value).ToString());
		}

		[Fact]
		public void FromToDecimal() {
			RedisObject value = 123.1M;
			Assert.Equal(123.1M, (decimal)value);
			value = "-321.3";
			Assert.Equal(-321.3M, (decimal)value);
		}

		[Fact]
		public void FromToBytes() {
			RedisObject value = new byte[] { (byte)'a', (byte)'b', (byte)'c' };
			var bytes = (byte[])value;
			Assert.Equal(3, bytes.Length);
			Assert.Equal((byte)'a', bytes[0]);
			Assert.Equal((byte)'b', bytes[1]);
			Assert.Equal((byte)'c', bytes[2]);
			Assert.Equal("abc", (string)value);
		}
	}
}
