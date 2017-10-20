using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaiLTools.Tools
{
    class BigEndianBinaryWriter : BinaryWriter
    {
        public BigEndianBinaryWriter(Stream stream)
            : base(stream, Encoding.GetEncoding("shift_jis"))
        {
        }


        public override void Write(int value)
        {
            byte[] bytes = BitConverter.GetBytes((int)value);
            if (!BitConverter.IsLittleEndian)
                bytes.Reverse();

            base.Write(bytes);
        }
    }
}
