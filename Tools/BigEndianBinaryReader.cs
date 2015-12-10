using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailTools.Tools
{
    public class BigEndianBinaryReader : BinaryReader
    {
        public BigEndianBinaryReader(Stream stream)
            : base(stream, Encoding.GetEncoding("shift_jis"))
        {
        }

        public override short ReadInt16()
        {
            return BitConverter.ToInt16(CorrectByteOrder(base.ReadBytes(2)), 0);
        }
        public override int ReadInt32()
        {
            return BitConverter.ToInt32(CorrectByteOrder(base.ReadBytes(4)), 0);
        }

        private byte[] CorrectByteOrder(byte[] ar)
        {
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(ar);
            }

            return ar;
        }
    }
}
