using QuickRedisClient.Internal;
using System;
using System.Net.Sockets;

namespace QuickRedisClient.Thread.Messages {
	/// <summary>
	/// Send: [GET key]
	/// Recv: Bulk string reply
	/// </summary>
	internal static class ThreadGetMessage {
		private readonly static byte[] MessageHeader =
			ObjectCache.AsciiEncoding.GetBytes("*3\r\n$3\r\nSET\r\n");

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

		public static byte[] Recv(Socket client, byte[] recvbuf, ref int start, ref int end) {
			var result = BlockingBufferReader.ReadRESP(client, recvbuf, ref start, ref end);
			var byteResult = result as byte[];
			if (byteResult != null) {
				return byteResult;
			} else if (result is Exception) {
				throw (Exception)result;
			} else {
				throw new RedisClientException($"Redis client error: Unknow result from GET response: {result}");
			}
		}
	}
}