using System.Runtime.CompilerServices;

namespace QuickRedisClient.Internal {
	/// <summary>
	/// Inline options
	/// </summary>
	internal static class InlineOptimization {
		public const MethodImplOptions InlineOption = MethodImplOptions.AggressiveInlining;
	}
}
