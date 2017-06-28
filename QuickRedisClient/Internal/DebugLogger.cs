using System;
using System.Runtime.CompilerServices;

namespace QuickRedisClient.Internal {
	/// <summary>
	/// Debug logger
	/// </summary>
	internal static class DebugLogger {
#if RELEASE
		public static bool EnableDebugLog = false;
		public static bool EnableContentsDebugLog = false;
#else
		public static bool EnableDebugLog = true;
		public static bool EnableContentsDebugLog = false;
#endif

#if RELEASE
		[MethodImpl(InlineOptimization.InlineOption)]
		public static void Log(string message) {
		}
#else
		[MethodImpl(InlineOptimization.InlineOption)]
		public static void Log(string message) {
			if (EnableDebugLog) {
				Console.WriteLine(message);
			}
		}
#endif

#if RELEASE
		[MethodImpl(InlineOptimization.InlineOption)]
		public static void Log(string message, int arg0) {
		}
#else
		[MethodImpl(InlineOptimization.InlineOption)]
		public static void Log(string message, int arg0) {
			if (EnableDebugLog) {
				Console.WriteLine(string.Format(message, arg0));
			}
		}
#endif

#if RELEASE
		[MethodImpl(InlineOptimization.InlineOption)]
		public static void Log(string message, params object[] args) {
		}
#else
		[MethodImpl(InlineOptimization.InlineOption)]
		public static void Log(string message, params object[] args) {
			if (EnableDebugLog) {
				Console.WriteLine(string.Format(message, args));
			}
		}
#endif

#if RELEASE
		[MethodImpl(InlineOptimization.InlineOption)]
		public static void LogSendContents(byte[] buf, int start, int len) {
		}
#else
		[MethodImpl(InlineOptimization.InlineOption)]
		public static void LogSendContents(byte[] buf, int start, int len) {
			if (EnableDebugLog && EnableContentsDebugLog) {
				var str = ObjectConverter.BytesToDebugString(buf, start, len);
				Console.WriteLine($"redis client send: {str}");
			}
		}
#endif

#if RELEASE
		[MethodImpl(InlineOptimization.InlineOption)]
		public static void LogRecvContents(byte[] buf, int start, int len) {
		}
#else
		[MethodImpl(InlineOptimization.InlineOption)]
		public static void LogRecvContents(byte[] buf, int start, int len) {
			if (EnableDebugLog && EnableContentsDebugLog) {
				var str = ObjectConverter.BytesToDebugString(buf, start, len);
				Console.WriteLine($"redis client recv: {str}");
			}
		}
#endif
	}
}
