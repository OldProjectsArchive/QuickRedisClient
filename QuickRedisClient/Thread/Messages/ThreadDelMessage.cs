using QuickRedisClient.Internal;
using System;
using System.Net.Sockets;

namespace QuickRedisClient.Thread.Messages {
	/// <summary>
	/// Send: [DEL key]
	/// Recv: Integer reply
	/// </summary>
	internal static class ThreadDelMessage {
		private readonly static byte[] MessageHeader =
			ObjectCache.AsciiEncoding.GetBytes("*2\r\n$3\r\nDEL\r\n");

		public static void Send(Socket client, byte[] sendbuf, byte[] key) {
			int len = 0;
			BlockingBufferWriter.WriteRawString(sendbuf, ref len, MessageHeader);
			if (key.Length > SmallStringOptimization.SendInsteadOfCopyIfGT) {
				BlockingBufferWriter.WriteBulkStringHeaderOnly(sendbuf, ref len, key);
				BlockingBufferWriter.FlushSendBuf(client, sendbuf, ref len);
				BlockingBufferWriter.FlushSendBuf(client, key);
				BlockingBufferWriter.FlushSendBuf(client, ObjectCache.CRLF);
			} else {
				BlockingBufferWriter.WriteBulkString(sendbuf, ref len, key);
				BlockingBufferWriter.FlushSendBuf(client, sendbuf, ref len);
			}
		}

		public static bool Recv(Socket client, byte[] recvbuf, ref int start, ref int end) {
			var result = BlockingBufferReader.ReadRESP(client, recvbuf, ref start, ref end);
			if (result is long) {
				return (long)result > 0;
			} else if (result is Exception) {
				throw (Exception)result;
			} else {
				throw new RedisClientException($"Redis client error: Unknow result from DEL response: {result}");
			}
		}
	}
}
