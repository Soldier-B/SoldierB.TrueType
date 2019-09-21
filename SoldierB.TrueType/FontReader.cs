using System;
using System.IO;
using System.Text;

namespace SoldierB.TrueType
{
    internal class FontReader : BinaryReader
    {
        public FontReader(Stream input) : base(input) { }
        public FontReader(Stream input, Encoding encoding) : base(input, encoding) { }
        public FontReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen) { }

        public override ushort ReadUInt16()
        {
            byte[] data = ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToUInt16(data, 0);
        }

        public override uint ReadUInt32()
        {
            byte[] data = ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToUInt32(data, 0);
        }

        public override ulong ReadUInt64()
        {
            byte[] data = ReadBytes(8);
            Array.Reverse(data);
            return BitConverter.ToUInt64(data, 0);
        }

        public string ReadTag()
        {
            byte[] data = ReadBytes(4);
            return Encoding.ASCII.GetString(data);
        }
    }
}
