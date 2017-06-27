using System.Linq;
using System.Text;

namespace QuickRedisClient.Internal {
	/// <summary>
	/// Object cache of common objects
	/// </summary>
	internal static class ObjectCache {
		/// <summary>
		/// Ascii encoding
		/// </summary>
		public readonly static Encoding AsciiEncoding = Encoding.ASCII;
		/// <summary>
		/// Utf8 encoding
		/// </summary>
		public readonly static Encoding UTF8Encoding = Encoding.UTF8;
		/// <summary>
		/// Byte array '\r\n'
		/// </summary>
		public readonly static byte[] CRLF = AsciiEncoding.GetBytes("\r\n");
		/// <summary>
		/// Byte array 'OK'
		/// </summary>
		public readonly static byte[] OK = AsciiEncoding.GetBytes("OK");
		/// <summary>
		/// Byte array ''
		/// </summary>
		public readonly static byte[] Empty = new byte[0];
		/// <summary>
		/// Cache small int from 0 to 65535 (include 65535)
		/// </summary>
		public const int SmallIntUpper = 0x10000;
		/// <summary>
		/// Positive small int => string cache
		/// </summary>
		public readonly static byte[][] PositiveIntToString = Enumerable
			.Range(0, SmallIntUpper)
			.Select(x => AsciiEncoding.GetBytes(x.ToString())).ToArray();
		/// <summary>
		/// Negative small int => string cache
		/// </summary>
		public readonly static byte[][] NegativeIntToString = Enumerable
			.Range(0, SmallIntUpper)
			.Select(x => AsciiEncoding.GetBytes((-x).ToString())).ToArray();
	}
}
