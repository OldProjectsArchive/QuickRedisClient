namespace QuickRedisClient {
	/// <summary>
	/// Interface for redis client factory
	/// </summary>
	public interface IRedisClientFactory {
		/// <summary>
		/// Create redis client
		/// </summary>
		IRedisClient Create(RedisClientConfiguration configuration);
	}
}
