using System.IO;
using System.Text;

namespace EEUniverse.Library
{
    /// <summary>
    /// Wrapper for BinaryReader that exposes the Read7BitEncodedInt method.
    /// </summary>
    internal class BitEncodedStreamReader : BinaryReader
    {
        internal BitEncodedStreamReader(MemoryStream stream, bool leaveOpen) : base(stream, Encoding.UTF8, leaveOpen) { }

        /// <summary>
        /// Reads in a 32-bit integer in a compressed format.
        /// </summary>
        internal new int Read7BitEncodedInt() => base.Read7BitEncodedInt();
    }

    /// <summary>
    /// Wrapper for BinaryWriter that exposes the Read7BitEncodedInt method.
    /// </summary>
    internal class BitEncodedStreamWriter : BinaryWriter
    {
        internal BitEncodedStreamWriter(MemoryStream stream) : base(stream) { }

        /// <summary>
        /// Writes a 32-bit integer in a compressed format.
        /// </summary>
        internal new void Write7BitEncodedInt(int value) => base.Write7BitEncodedInt(value);
    }
}
