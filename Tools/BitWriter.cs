using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaiLTools.Tools
{
    internal class BitWriter : BinaryWriter
    {
        private BitStream _bitStream;
        public BitWriter(Stream stream) : base( new BitStream(stream) )
        {
            _bitStream = (BitStream)BaseStream;
        }

        public void WriteBits(byte b, BitNum bits) {
            _bitStream.WriteBits(b, bits);
        }

        public void WriteWithBits(byte number, int bitsToUse)
        {
            int availableBits = 8;
            int shift = availableBits - bitsToUse;

            byte newNumber = (byte)(number << shift);

            _bitStream.WriteBits(newNumber, (BitNum)(bitsToUse));
        }

        public void WriteWithBits(uint number, int bitsToUse)
        {
            int availableBits = 32;
            int shift = availableBits - bitsToUse;
            uint newNumber = number << shift;

            byte[] bytes = BitConverter.GetBytes(newNumber).Reverse().ToArray();
            _bitStream.WriteBits(bytes, 0, (ulong)bitsToUse);
        }
    }
}
