using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace QuickRedisClient.Internal {
	/// <summary>
	/// Blocking buffer reader
	/// </summary>
	internal static class BlockingBufferReader {
		/// <summary>
		/// For security check
		/// </summary>
		public const int MaxAllowedBulkStringLength = 1024 * 1024 * 32;
		/// <summary>
		/// For security check
		/// </summary>
		public const int MaxAllowedArraySize = 65535;

		/// <summary>
		/// Receive contents to buffer only if there not exist contents
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FillRecvBuf(Socket client, byte[] buf, ref int start, ref int end) {
			if (start == end) {
				start = end = 0;
				int result = client.Receive(buf, end, buf.Length - end, SocketFlags.None);
				if (result <= 0) {
					throw new RedisClientException("Redis client error: connection closed unexpectedly (found by recv)");
				}
				DebugLogger.LogRecvContents(buf, end, result);
				end += result;
			}
		}

		/// <summary>
		/// Find first index of '\r\n' in buffer, return -1 if not found
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int IndexOfCRLF(byte[] buf, int start, int end) {
			for (int from = start, to = end - 1; from < to; ++from) {
				if (buf[from] == '\r' && buf[from + 1] == '\n') {
					return from;
				}
			}
			return -1;
		}

		/// <summary>
		/// Read contents from buffer until '\r\n' occurs
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BufferSpan ReadUntilCRLF(Socket client, byte[] buf, ref int start, ref int end) {
			// fast path
			var index = IndexOfCRLF(buf, start, end);
			if (index >= 0) {
				var span = new BufferSpan(buf, true, start, index - start);
				start = index + 2;
				return span;
			}
			// oops, recvbuf is not enough to hold the string
			byte[] strBuf = null;
			var strBufActualLen = 0;
			var copyUntil = end;
			var isLastCopy = false;
			do {
				// append buf to str buf
				var copyLen = copyUntil - start;
				var oldBufLen = strBuf == null ? 0 : strBuf.Length;
				var newBufLen = strBufActualLen + copyLen;
				if (newBufLen > oldBufLen) {
					// expand str buf
					var allocateSize = isLastCopy ? newBufLen : Math.Max(oldBufLen * 2, newBufLen);
					var newStrBuf = new byte[allocateSize];
					if (strBuf != null) {
						Buffer.BlockCopy(strBuf, 0, newStrBuf, 0, strBufActualLen);
					}
					strBuf = newStrBuf;
				}
				Buffer.BlockCopy(buf, start, strBuf, strBufActualLen, copyLen);
				strBufActualLen = newBufLen;
				if (isLastCopy) {
					break;
				}
				// receive more
				start = end = 0;
				FillRecvBuf(client, buf, ref start, ref end);
				// check if we have \r in strBuf and \n in buf
				if (strBuf[strBufActualLen - 1] == '\r' && buf[start] == '\n') {
					strBufActualLen -= 1;
					start += 1;
					break;
				}
				// find \r\n in buf
				index = IndexOfCRLF(buf, start, end);
				if (index == 0) {
					start = index + 2;
					break;
				} else if (index > 0) {
					start = index + 2;
					copyUntil = index;
					isLastCopy = true;
				} else {
					copyUntil = end;
				}
			} while (true);
			return new BufferSpan(strBuf, false, 0, strBufActualLen);
		}

		/// <summary>
		/// Read fixed length contens from buffer
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static BufferSpan ReadUntilLength(Socket client, byte[] buf, ref int start, ref int end, int length) {
			// fast path
			var lengthIncludeCRLF = length + 2;
			if (end - start > lengthIncludeCRLF) {
				var span = new BufferSpan(buf, true, start, length);
				start += lengthIncludeCRLF;
				return span;
			}
			// hmm, recvbuf is not enough to hold the string
			var strBuf = new byte[length];
			var strBufActualLen = 0;
			do {
				// append buf to str buf
				var copyLen = end - start;
				if (strBufActualLen + copyLen >= strBuf.Length) {
					copyLen = strBuf.Length - strBufActualLen;
				}
				Buffer.BlockCopy(buf, start, strBuf, strBufActualLen, copyLen);
				start += copyLen;
				strBufActualLen += copyLen;
				if (strBufActualLen >= strBuf.Length) {
					break;
				}
				// receive more
				FillRecvBuf(client, buf, ref start, ref end);
			} while (true);
			// also eat CRLF
			var leaveLen = end - start;
			if (leaveLen >= 2) {
				start += 2;
			} else {
				start = end = 0;
				FillRecvBuf(client, buf, ref start, ref end);
				start += 2 - leaveLen;
			}
			return new BufferSpan(strBuf, false, 0, strBufActualLen);
		}

		/// <summary>
		/// Read RESP object
		/// For
		/// - simple string, return byte[]
		/// - error, return Exception
		/// - integer, return int
		/// - bulk string, return byte[]
		/// - array, return object[]
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static object ReadRESP(Socket client, byte[] buf, ref int start, ref int end) {
			// if there nothing in the buffer, read something first
			FillRecvBuf(client, buf, ref start, ref end);
			// check the RESP type
			var type = buf[start++];
			object result;
			if (type == '+') {
				// simple string, return byte[]
				result = ReadUntilCRLF(client, buf, ref start, ref end).ToBytes();
			} else if (type == '-') {
				// error, return Exception
				var error = ReadUntilCRLF(client, buf, ref start, ref end).ToBytes();
				var errorStr = ObjectCache.UTF8Encoding.GetString(error);
				result = new RedisClientException($"Redis server error: {errorStr}");
			} else if (type == ':') {
				// integer, return int
				result = ReadUntilCRLF(client, buf, ref start, ref end).ToInt();
			} else if (type == '$') {
				// bulk string, return byte[]
				var length = ReadUntilCRLF(client, buf, ref start, ref end).ToInt();
				if (length > MaxAllowedBulkStringLength) {
					throw new RedisClientException($"Redis client error: bulk string too long, length is '{length}'");
				}
				result = ReadUntilLength(client, buf, ref start, ref end, length).ToBytes();
			} else if (type == '*') {
				// array, return object[]
				var size = ReadUntilCRLF(client, buf, ref start, ref end).ToInt();
				if (size > MaxAllowedArraySize) {
					throw new RedisClientException($"Redis client error: array too long, size is '{size}'");
				}
				var resultArray = new object[size];
				for (var x = 0; x < size; ++x) {
					resultArray[x] = ReadRESP(client, buf, ref start, ref end);
				}
				result = resultArray;
			} else {
				throw new RedisClientException($"Redis client error: wrong redis type '{(char)type}'");
			}
			return result;
		}
	}
}
