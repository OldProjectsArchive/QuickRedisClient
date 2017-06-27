namespace QuickRedisClient.Internal {
	/// <summary>
	/// SSO options
	/// </summary>
	internal static class SmallStringOptimization {
		/// <summary>
		/// Send string instead of copy if greater then this value
		/// </summary>
		public const int SendInsteadOfCopyIfGT = 128;
	}
}
