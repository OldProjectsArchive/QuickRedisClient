using QuickRedisClient.Internal;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace QuickRedisClient.Thread.Messages {
	/// <summary>
	/// Send: [DEL key...]
	/// Recv: Integer reply
	/// </summary>
	internal static class ThreadDelManyMessage {
		private readonly static byte[] MessageHeader =
			ObjectCache.AsciiEncoding.GetBytes("*3\r\n$3\r\nDEL\r\n");

		public static void Send(Socket client, byte[] sendbuf, IEnumerable<byte[]> keys) {
			int len = 0;
			BlockingBufferWriter.WriteRawString(sendbuf, ref len, MessageHeader);
			foreach (var key in keys) {
				if (len + key.Length +
					BlockingBufferWriter.MaxBulkStringAdditionalLength > sendbuf.Length) {
					BlockingBufferWriter.FlushSendBuf(client, sendbuf, ref len);
				}
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
		}

		public static long Recv(Socket client, byte[] recvbuf, ref int start, ref int end) {
			var result = BlockingBufferReader.ReadRESP(client, recvbuf, ref start, ref end);
			if (result is long) {
				return (long)result;
			} else if (result is Exception) {
				throw (Exception)result;
			} else {
				throw new RedisClientException($"Redis client error: Unknow result from DEL response: {result}");
			}
		}
	}
}
