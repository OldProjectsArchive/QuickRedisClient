using System.Linq;
using System.Text;

namespace QuickRedisClient.Internal {
	/// <summary>
	/// Object cache of common objects
	/// </summary>
	internal static class CommonObjectCache {
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
		/// Small int cache
		/// </summary>
		public readonly static byte[][] SmallIntCache = Enumerable.Range(0, 1000)
			.Select(x => AsciiEncoding.GetBytes(x.ToString())).ToArray();
	}
}
