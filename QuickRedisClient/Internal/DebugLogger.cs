using System;
using System.Runtime.CompilerServices;

namespace QuickRedisClient.Internal {
	/// <summary>
	/// Debug logger
	/// </summary>
	internal static class DebugLogger {
#if RELEASE
		public const bool EnableDebugLog = false;
		public const bool EnableContentsDebugLog = false;
#else
		public const bool EnableDebugLog = true;
		public const bool EnableContentsDebugLog = true;
#endif

#if RELEASE
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Log(string message) {
		}
#else
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Log(string message) {
			Console.WriteLine(message);
		}
#endif

#if RELEASE
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Log(string message, int arg0) {
		}
#else
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Log(string message, int arg0) {
			Console.WriteLine(string.Format(message, arg0));
		}
#endif

#if RELEASE
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Log(string message, params object[] args) {
		}
#else
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Log(string message, params object[] args) {
			Console.WriteLine(string.Format(message, args));
		}
#endif

#if RELEASE
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void LogSendContents(byte[] buf, int start, int len) {
		}
#else
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void LogSendContents(byte[] buf, int start, int len) {
			if (EnableContentsDebugLog) {
				var str = ObjectConverter.BytesToDebugString(buf, start, len);
				Console.WriteLine($"redis client send: {str}");
			}
		}
#endif

#if RELEASE
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void LogRecvContents(byte[] buf, int start, int len) {
		}
#else
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void LogRecvContents(byte[] buf, int start, int len) {
			if (EnableContentsDebugLog) {
				var str = ObjectConverter.BytesToDebugString(buf, start, len);
				Console.WriteLine($"redis client recv: {str}");
			}
		}
#endif
	}
}
