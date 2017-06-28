using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;

namespace QuickRedisClient.Internal {
	/// <summary>
	/// Fast object converter
	/// </summary>
	internal class ObjectConverter {
		/// <summary>
		/// Convert long to byte[], eg: 123 => '123'
		/// </summary>
		[MethodImpl(InlineOptimization.InlineOption)]
		public static byte[] LongToStringBytes(long value) {
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
		/// Convert byte[] to long, eg: '-123' => -123
		/// </summary>
		[MethodImpl(InlineOptimization.InlineOption)]
		public static long StringBytesToLong(byte[] bytes, int start, int len) {
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

		/// <summary>
		/// Convert string to end point, eg "127.0.0.1:6379" => IPEndPoint
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		[MethodImpl(InlineOptimization.InlineOption)]
		public static EndPoint StringToEndPoint(string address) {
			var index = address.LastIndexOf(':');
			var ip = IPAddress.Parse(address.Substring(0, index));
			var port = ushort.Parse(address.Substring(index + 1));
			return new IPEndPoint(ip, port);
		}

		/// <summary>
		/// Convert bytes to string only for debug
		/// </summary>
		[MethodImpl(InlineOptimization.InlineOption)]
		public static string BytesToDebugString(byte[] bytes, int start, int len) {
			try {
				return JsonConvert.SerializeObject(
					ObjectCache.UTF8Encoding.GetString(bytes, start, len));
			} catch {
				return "[" + string.Join(", ", bytes.Skip(start).Take(len)) + "]";
			}
		}
	}
}
