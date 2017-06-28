using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickRedisClient {
	/// <summary>
	/// Interface for redis client
	/// </summary>
	public interface IRedisClient : IDisposable {
		/// <summary>
		/// Set key to hold the string value
		/// </summary>
		void Set(RedisObject key, RedisObject value);
		/// <summary>
		/// Set key to hold the string value
		/// </summary>
		Task SetAsync(RedisObject key, RedisObject value);

		/// <summary>
		/// Get the value of key
		/// </summary>
		RedisObject Get(RedisObject key);
		/// <summary>
		/// Get the value of key
		/// </summary>
		Task<RedisObject> GetAsync(RedisObject key);

		/// <summary>
		/// Removes the specified key
		/// </summary>
		bool Del(RedisObject key);
		/// <summary>
		/// Removes the specified key
		/// </summary>
		Task<bool> DelAsync(RedisObject key);

		/// <summary>
		/// Removes the specified keys
		/// </summary>
		long DelMany(IList<RedisObject> keys);
		/// <summary>
		/// Removes the specified keys
		/// </summary>
		Task<long> DelManyAsync(IList<RedisObject> keys);
	}
}
