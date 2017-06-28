using QuickRedisClient.Internal;
using System.Runtime.CompilerServices;

namespace QuickRedisClient {
	/// <summary>
	/// Redis object class
	/// Support convert from and to string, byte[], long
	/// </summary>
	public struct RedisObject {
		private readonly static byte[] IsInteger = new byte[0];
		byte[] _bytes;
		long _integer;

		[MethodImpl(InlineOptimization.InlineOption)]
		private RedisObject(byte[] bytes, long integer) {
			_bytes = bytes;
			_integer = integer;
		}

		/// <summary>
		/// Convert string to redis object
		/// </summary>
		[MethodImpl(InlineOptimization.InlineOption)]
		public static implicit operator RedisObject(string str) {
			return new RedisObject(ObjectCache.UTF8Encoding.GetBytes(str), 0);
		}

		/// <summary>
		/// Convert byte[] to redis object
		/// </summary>
		[MethodImpl(InlineOptimization.InlineOption)]
		public static implicit operator RedisObject(byte[] bytes) {
			return new RedisObject(bytes, 0);
		}

		/// <summary>
		/// Convert long to redis object
		/// </summary>
		[MethodImpl(InlineOptimization.InlineOption)]
		public static implicit operator RedisObject(long integer) {
			return new RedisObject(IsInteger, integer);
		}

		/// <summary>
		/// Convert double to redis object
		/// </summary>
		[MethodImpl(InlineOptimization.InlineOption)]
		public static implicit operator RedisObject(double value) {
			if (value % 1 == 0 && value >= long.MinValue && value <= long.MaxValue) {
				return (long)value;
			}
			return value.ToString();
		}

		/// <summary>
		/// Convert decimal to redis object
		/// </summary>
		[MethodImpl(InlineOptimization.InlineOption)]
		public static implicit operator RedisObject(decimal value) {
			if (value % 1 == 0 && value >= long.MinValue && value <= long.MaxValue) {
				return (long)value;
			}
			return value.ToString();
		}

		/// <summary>
		/// Determine this redis object has value or not
		/// </summary>
		public bool HasValue => _bytes != null;

		/// <summary>
		/// Convert redis object to long
		/// </summary>
		[MethodImpl(InlineOptimization.InlineOption)]
		public static explicit operator long(RedisObject obj) {
			if (obj._bytes == IsInteger) {
				return obj._integer;
			} else if (obj._bytes == null) {
				return 0;
			}
			return ObjectConverter.StringBytesToLong(obj._bytes, 0, obj._bytes.Length);
		}

		/// <summary>
		/// Convert redis object to int
		/// </summary>
		[MethodImpl(InlineOptimization.InlineOption)]
		public static explicit operator int(RedisObject obj) {
			var value = (long)obj;
			return checked((int)value);
		}

		/// <summary>
		/// Convert redis object to double
		/// </summary>
		[MethodImpl(InlineOptimization.InlineOption)]
		public static explicit operator double(RedisObject obj) {
			if (obj._bytes == IsInteger) {
				return obj._integer;
			} else if (obj._bytes == null) {
				return 0;
			}
			return double.Parse((string)obj);
		}

		/// <summary>
		/// Convert redis object to float
		/// </summary>
		[MethodImpl(InlineOptimization.InlineOption)]
		public static explicit operator float(RedisObject obj) {
			var value = (double)obj;
			return checked((float)value);
		}

		/// <summary>
		/// Convert redis object to decimal
		/// </summary>
		[MethodImpl(InlineOptimization.InlineOption)]
		public static explicit operator decimal(RedisObject obj) {
			if (obj._bytes == IsInteger) {
				return obj._integer;
			} else if (obj._bytes == null) {
				return 0;
			}
			return decimal.Parse((string)obj);
		}

		/// <summary>
		/// Convert redis object to string
		/// </summary>
		[MethodImpl(InlineOptimization.InlineOption)]
		public static explicit operator string(RedisObject obj) {
			if (obj._bytes == null) {
				return null;
			} else if (obj._bytes == IsInteger) {
				return obj._integer.ToString();
			}
			return ObjectCache.UTF8Encoding.GetString(obj._bytes);
		}

		/// <summary>
		/// Convert redis object to byte[]
		/// </summary>
		[MethodImpl(InlineOptimization.InlineOption)]
		public static explicit operator byte[] (RedisObject obj) {
			if (obj._bytes == IsInteger) {
				return ObjectConverter.LongToStringBytes(obj._integer);
			}
			return obj._bytes;
		}

		/// <summary>
		/// Determine if two redis object are equivalent
		/// </summary>
		[MethodImpl(InlineOptimization.InlineOption)]
		public static bool operator ==(RedisObject lhs, RedisObject rhs) {
			if ((lhs._bytes == IsInteger) != (rhs._bytes == IsInteger)) {
				return false; // one is integer, other is not
			} else if (lhs._bytes == IsInteger) {
				return lhs._integer == rhs._integer; // both is integer
			} else if ((lhs._bytes == null) != (rhs._bytes == null)) {
				return false; // one is null, other is not
			} else if (lhs._bytes == null) {
				return true; // both is null
			} else if (lhs._bytes.Length != rhs._bytes.Length) {
				return false; // length is different
			}
			for (int from = 0, to = lhs._bytes.Length; from < to; ++from) {
				if (lhs._bytes[from] != rhs._bytes[from]) {
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Determine if two redis object are not equivalent
		/// </summary>
		[MethodImpl(InlineOptimization.InlineOption)]
		public static bool operator !=(RedisObject lhs, RedisObject rhs) {
			return !(lhs == rhs);
		}

		/// <summary>
		/// Get hash code from this object
		/// </summary>
		public override int GetHashCode() {
			if (_bytes == IsInteger) {
				return _integer.GetHashCode();
			} else if (_bytes == null) {
				return -1;
			}
			return _bytes.GetHashCode();
		}

		/// <summary>
		/// Determine if this and other object are equivalent
		/// </summary>
		public override bool Equals(object obj) {
			if (!(obj is RedisObject)) {
				return false;
			}
			return this == (RedisObject)obj;
		}

		/// <summary>
		/// Return the string representation of this object
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			if (_bytes == IsInteger) {
				return _integer.ToString();
			} else if (_bytes == null) {
				return "(nil)";
			}
			return ObjectConverter.BytesToDebugString(_bytes, 0, _bytes.Length);
		}
	}
}
