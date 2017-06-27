using System;
using System.Runtime.CompilerServices;

namespace QuickRedisClient.Internal {
	/// <summary>
	/// Fast object converter
	/// </summary>
	internal class ObjectConverter {
		/// <summary>
		/// Convert int to string, eg: 123 => '123'
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte[] IntegerToString(long value) {
			if (value >= 0) {
				if (value < ObjectCache.SmallIntUpper) {
					return ObjectCache.PositiveIntToString[value];
				}
			} else {
				var absValue = -value;
				if (absValue < ObjectCache.SmallIntUpper) {
					return ObjectCache.NegativeIntToString[absValue];
				}
			}
			return ObjectCache.AsciiEncoding.GetBytes(value.ToString());
		}

		/// <summary>
		/// Convert string to int, eg: '-123' => -123
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long StringToInteger(byte[] bytes, int start, int len) {
			if (len == 0) {
				return 0;
			}
			bool negative = false;
			if (bytes[start] == '-') {
				negative = true;
				++start;
				--len;
			}
			long result = 0;
			for (int from = start, to = start + len; from < to; ++from) {
				var value = bytes[from] - '0';
				if (value >= 10) {
					throw new FormatException($"Invalid integer format");
				}
				result = checked(result * 10 + value);
			}
			if (negative) {
				result = -result;
			}
			return result;
		}
	}
}
