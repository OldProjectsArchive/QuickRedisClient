using System;
using System.Runtime.CompilerServices;

namespace QuickRedisClient.Internal {
	/// <summary>
	/// Hold a buffer and it's slice
	/// </summary>
	internal struct BufferSpan {
		private byte[] _buffer;
		private bool _bufferMayReuse;
		private int _start;
		private int _len;

		/// <summary>
		/// Initialize
		/// </summary>
		[MethodImpl(InlineOptimization.InlineOption)]
		public BufferSpan(byte[] array, bool bufferMayReuse, int start, int len) {
			_buffer = array;
			_bufferMayReuse = bufferMayReuse;
			_start = start;
			_len = len;
		}

		/// <summary>
		/// Get bytes from this span
		/// </summary>
		/// <returns></returns>
		[MethodImpl(InlineOptimization.InlineOption)]
		public byte[] ToBytes() {
			if (_len == 0) {
				return ObjectCache.Empty;
			} else if (_len == 2 && _buffer[_start] == 'O' && _buffer[_start + 1] == 'K') {
				return ObjectCache.OK;
			} else if (!_bufferMayReuse && _start == 0 && _len == _buffer.Length) {
				return _buffer;
			}
			var result = new byte[_len];
			Buffer.BlockCopy(_buffer, _start, result, 0, _len);
			return result;
		}

		/// <summary>
		/// Get int from this span
		/// Notice: it won't check the buffer is valid not-overflowed int or not
		/// </summary>
		/// <returns></returns>
		[MethodImpl(InlineOptimization.InlineOption)]
		public long ToLong() {
			return ObjectConverter.StringBytesToLong(_buffer, _start, _len);
		}
	}
}
