using System;
using System.Threading.Tasks;

namespace QuickRedisClient {
	/// <summary>
	/// Interface for redis client
	/// </summary>
	public interface IRedisClient : IDisposable {
		/// <summary>
		/// Set key to hold the string value
		/// </summary>
		void Set(byte[] key, byte[] value);
		/// <summary>
		/// Set key to hold the string value
		/// </summary>
		Task SetAsync(byte[] key, byte[] value);

		/// <summary>
		/// Get the value of key
		/// </summary>
		byte[] Get(byte[] key);
		/// <summary>
		/// Get the value of key
		/// </summary>
		Task<byte[]> GetAsync(byte[] key);

		/// <summary>
		/// Removes the specified key
		/// </summary>
		void Del(byte[] key);
		/// <summary>
		/// Removes the specified key
		/// </summary>
		Task DelAsync(byte[] key);

		/// <summary>
		/// Removes the specified keys
		/// </summary>
		void Del(byte[][] keys);
		/// <summary>
		/// Removes the specified keys
		/// </summary>
		Task DelAsync(byte[][] keys);
	}
}
