using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace EEUniverse.Library
{
    public ref struct MessageWriter
    {
        private const MethodImplOptions Inlining = MethodImplOptions.AggressiveInlining;

        private readonly Span<byte> _target;
        private int _i;

        public MessageWriter(Span<byte> target)
        {
            _target = target;
            _i = 0;
        }

        [MethodImpl(Inlining)]
        public void Write(byte value)
            => _target[_i++] = value;

        // as of present day, it is currently entirely safe to cast these.
        // in the future, a 7 bit encoded int will be needed if there are more

        [MethodImpl(Inlining)]
        public void Write(ConnectionScope value)
            => Write((byte)value);

        [MethodImpl(Inlining)]
        public void Write(MessageType value)
            => Write((byte)value);

        [MethodImpl(Inlining)]
        public void Write(double value)
        {
            // BitConverter doesn't have a method to do this
            // thus, following ToDouble and working backwards https://source.dot.net/#System.Private.CoreLib/shared/System/BitConverter.cs,354
            // this is here

            var position = _target.Slice(_i);

            Unsafe.WriteUnaligned<double>(ref MemoryMarshal.GetReference(position), value);

            _i += sizeof(double);
        }

        [MethodImpl(Inlining)]
        public void Write(string value)
        {
            Write(value.Length);
            Encoding.UTF8.GetBytes(value, _target.Slice(_i));
            _i += value.Length;
        }

        [MethodImpl(Inlining)]
        public void Write(byte[] value)
            => Write(new ReadOnlySpan<byte>(value));

        [MethodImpl(Inlining)]
        public void Write(ReadOnlySpan<byte> value)
        {
            Write(value.Length);
            WriteBytes(value);
        }

        [MethodImpl(Inlining)]
        public void WriteBytes(ReadOnlySpan<byte> value)
        {
            value.CopyTo(_target.Slice(_i));
            _i += value.Length;
        }

        [MethodImpl(Inlining)]
        public void Write(int value)
        {
            // nearly copied from https://source.dot.net/#System.Private.CoreLib/shared/System/IO/BinaryWriter.cs,456

            while (value >= 0b1_000_0000)
            {
                Write((byte)(value | 0b1_000_0000));
                value >>= 7;
            }

            Write((byte)value);
        }
    }
}
