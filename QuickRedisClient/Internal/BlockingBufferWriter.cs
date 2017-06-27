using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace QuickRedisClient.Internal {
	/// <summary>
	/// Blocking buffer writer
	/// </summary>
	internal static class BlockingBufferWriter {
		/// <summary>
		/// Writer raw string to buffer, without header
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteRawString(byte[] buf, ref int start, byte[] str) {
			Buffer.BlockCopy(str, 0, buf, start, str.Length);
			start += str.Length;
		}

		/// <summary>
		/// Write bulk string to buffer, with header
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteBulkString(byte[] buf, ref int start, byte[] str) {
			buf[start++] = (byte)'$';
			WriteRawInt(buf, ref start, str.Length);
			buf[start++] = (byte)'\r';
			buf[start++] = (byte)'\n';
			WriteRawString(buf, ref start, str);
			buf[start++] = (byte)'\r';
			buf[start++] = (byte)'\n';
		}

		/// <summary>
		/// Write buffer string header to buffer, without contents
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteBulkStringHeaderOnly(byte[] buf, ref int start, byte[] str) {
			buf[start++] = (byte)'$';
			WriteRawInt(buf, ref start, str.Length);
			buf[start++] = (byte)'\r';
			buf[start++] = (byte)'\n';
		}

		/// <summary>
		/// Write raw int to buffer
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void WriteRawInt(byte[] buf, ref int start, int value) {
			var bytes = ObjectConverter.IntegerToString(value);
			Buffer.BlockCopy(bytes, 0, buf, start, bytes.Length);
			start += bytes.Length;
		}

		/// <summary>
		/// Send all contents in buffer immediately
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FlushSendBuf(Socket client, byte[] buf, ref int len) {
			DebugLogger.LogSendContents(buf, 0, len);
			int sent = 0;
			while (sent < len) {
				var result = client.Send(buf, 0, len - sent, SocketFlags.None);
				if (result <= 0) {
					throw new RedisClientException(
						"Redis client error: connection closed unexpectedly (found by send)");
				}
				sent += result;
			}
			len = 0;
		}

		/// <summary>
		/// Send all contents in buffer immediately
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FlushSendBuf(Socket client, byte[] buf) {
			int len = buf.Length;
			FlushSendBuf(client, buf, ref len);
		}
	}
}
