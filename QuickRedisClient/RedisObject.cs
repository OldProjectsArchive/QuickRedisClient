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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private RedisObject(byte[] bytes, long integer) {
			_bytes = bytes;
			_integer = integer;
		}

		/// <summary>
		/// Convert string to redis object
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator RedisObject(string str) {
			return new RedisObject(ObjectCache.UTF8Encoding.GetBytes(str), 0);
		}

		/// <summary>
		/// Convert byte[] to redis object
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator RedisObject(byte[] bytes) {
			return new RedisObject(bytes, 0);
		}

		/// <summary>
		/// Convert long to redis object
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator RedisObject(long integer) {
			return new RedisObject(IsInteger, 0);
		}

		/// <summary>
		/// Convert double to redis object
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator RedisObject(double value) {
			if (value % 1 == 0 && value >= long.MinValue && value <= long.MaxValue) {
				return (long)value;
			}
			return value.ToString();
		}

		/// <summary>
		/// Determine this redis object has value or not
		/// </summary>
		public bool HasValue => _bytes == null;

		/// <summary>
		/// Convert redis object to long
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator long(RedisObject obj) {
			if (obj._bytes == IsInteger) {
				return obj._integer;
			} else if (obj._bytes == null) {
				return 0;
			}
			return ObjectConverter.StringToInteger(obj._bytes, 0, obj._bytes.Length);
		}

		/// <summary>
		/// Convert redis object to int
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator int(RedisObject obj) {
			var value = (long)obj;
			return checked((int)value);
		}

		/// <summary>
		/// Convert redis object to double
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator float(RedisObject obj) {
			var value = (double)obj;
			return checked((float)value);
		}

		/// <summary>
		/// Convert redis object to string
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator byte[] (RedisObject obj) {
			if (obj._bytes == IsInteger) {
				return ObjectConverter.IntegerToString(obj._integer);
			}
			return obj._bytes;
		}

		/// <summary>
		/// Determine if two redis object are equivalent
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(RedisObject lhs, RedisObject rhs) {
			return lhs._bytes == rhs._bytes && lhs._integer == rhs._integer;
		}

		/// <summary>
		/// Determine if two redis object are not equivalent
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(RedisObject lhs, RedisObject rhs) {
			return lhs._bytes != rhs._bytes || lhs._integer != rhs._integer;
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
	}
}
