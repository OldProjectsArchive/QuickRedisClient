using QuickRedisClient.Internal;
using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace QuickRedisClient.Thread.Messages {
	/// <summary>
	/// Send: [SET key value]
	/// Recv: Simple string reply
	/// </summary>
	internal static class ThreadSetMessage {
		private readonly static byte[] MessageHeader =
			ObjectCache.AsciiEncoding.GetBytes("*3\r\n$3\r\nSET\r\n");

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Send(Socket client, byte[] sendbuf, byte[] key, byte[] value) {
			int len = 0;
			BlockingBufferWriter.WriteRawString(sendbuf, ref len, MessageHeader);
			if (key.Length > SmallStringOptimization.SendInsteadOfCopyIfGT) {
				BlockingBufferWriter.WriteBulkStringHeaderOnly(sendbuf, ref len, key);
				BlockingBufferWriter.FlushSendBuf(client, sendbuf, ref len);
				BlockingBufferWriter.FlushSendBuf(client, key);
				BlockingBufferWriter.FlushSendBuf(client, ObjectCache.CRLF);
			} else {
				BlockingBufferWriter.WriteBulkString(sendbuf, ref len, key);
			}
			if (value.Length > SmallStringOptimization.SendInsteadOfCopyIfGT) {
				BlockingBufferWriter.WriteBulkStringHeaderOnly(sendbuf, ref len, value);
				BlockingBufferWriter.FlushSendBuf(client, sendbuf, ref len);
				BlockingBufferWriter.FlushSendBuf(client, value);
				BlockingBufferWriter.FlushSendBuf(client, ObjectCache.CRLF);
			} else {
				BlockingBufferWriter.WriteBulkString(sendbuf, ref len, value);
				BlockingBufferWriter.FlushSendBuf(client, sendbuf, ref len);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Recv(Socket client, byte[] recvbuf, ref int start, ref int end) {
			var result = BlockingBufferReader.ReadRESP(client, recvbuf, ref start, ref end);
			if (result == ObjectCache.OK) {
			} else if (result is Exception) {
				throw (Exception)result;
			} else {
				throw new RedisClientException($"Redis client error: Unknow result from SET response: {result}");
			}
		}
	}
}

