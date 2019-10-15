using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace EEUniverse.Library
{
    /// <summary>
	/// A speedy message reader.
	/// </summary>
	public ref struct MessageReader
	{
		private const MethodImplOptions Inlining = MethodImplOptions.AggressiveInlining;

		private ReadOnlySpan<byte> _data;
		private int _i;

		public MessageReader(ReadOnlySpan<byte> data)
		{
			_data = data;
			_i = 0;
		}

        [MethodImpl(Inlining)]
		public bool IsDataLeft()
            => _i < _data.Length;

		[MethodImpl(Inlining)]
		public ConnectionScope ReadConnectionScope()
            => (ConnectionScope)_data[_i++];

		[MethodImpl(Inlining)]
		public MessageType ReadMessageType()
            => (MessageType)Read7BitEncodedInt();

		[MethodImpl(Inlining)]
		public string ReadString()
            => Encoding.UTF8.GetString(ReadBytes());

		[MethodImpl(Inlining)]
		public ReadOnlySpan<byte> ReadBytes()
            => ReadBytes(Read7BitEncodedInt());

		[MethodImpl(Inlining)]
		public double ReadDouble()
			=> BitConverter.ToDouble(ReadBytes(8));

		[MethodImpl]
		public byte ReadByte()
			=> _data[_i++];

		[MethodImpl(Inlining)]
        public ReadOnlySpan<byte> ReadBytes(int length)
        {
			var bytes = _data.Slice(_i, length);

			_i += length;

			return bytes;
		}

		[MethodImpl(Inlining)]
		public int Read7BitEncodedInt()
		{
			// basically copied and pasted from https://source.dot.net/#System.Private.CoreLib/shared/System/IO/BinaryReader.cs,587

			var i = 0;
			var count = 0;
			var shift = 0;
			byte b;

			do
			{
				if (shift == 5 * 7)
				{
					ThrowBadFormat();
				}

				b = _data[i++];

				count |= (b & 0b0111_1111) << shift;
				shift += 7;
			}
			while ((b & 0b1000_0000) != 0b0000_0000);

			_data = _data.Slice(i);
			return count;
		}

		// see: https://reubenbond.github.io/posts/dotnet-perf-tuning
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void ThrowBadFormat()
			=> throw new FormatException("Too many bytes in what should have been a 7 bit encoded Int32.");
	}
}
