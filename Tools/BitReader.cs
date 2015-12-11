using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaiLTools.Tools
{
    internal class SimpleBitReader
    {
        int _bit;
        byte _currentByte;
        Stream _stream;
        byte[] bytes;
        int totalbits;

        public SimpleBitReader(byte[] bytes)
        { 
            this.bytes = bytes;
            totalbits = bytes.Length * 8;

        }

        //public bool? ReadBit(bool bigEndian = false)
        //{
        //    if (_bit == 8)
        //    {

        //        var r = _stream.ReadByte();
        //        if (r == -1) return null;
        //        _bit = 0;
        //        _currentByte = (byte)r;
        //    }
        //    bool value;
        //    if (!bigEndian)
        //        value = (_currentByte & (1 << _bit)) > 0;
        //    else
        //        value = (_currentByte & (1 << (7 - _bit))) > 0;

        //    _bit++;
        //    return value;
        //}

        public uint ReadUInt(int bitcount)
        {
            uint rt = 0;
            while ( (bitcount--) > 0)
            {
                if (_bit >= totalbits)
                    return 0;

                rt <<= 1;

                var calc = bytes[_bit >> 3] & (1 << (7 - (_bit & 7)));
                if (calc != 0)
                    rt |= 1;

                _bit++;
            }
            return rt;
        }

        public bool ReadBool()
        {
            return ReadUInt(1) == 1;
        }
    }
    public class BitReader : IDisposable
    {
        /// <summary>
        /// The stream being read from.
        /// </summary>
        public Stream BaseStream { get; private set; }

        /// <summary>
        /// The encoding of characters in the underlying stream.
        /// </summary>
        public Encoding Encoding;

        /// <summary>
        /// The data buffer.
        /// </summary>
        private byte[] buffer;

        /// <summary>
        /// Amount of available bits in the buffer.
        /// </summary>
        private long bufferAvailableBits;

        /// <summary>
        /// The current bit in the buffer.
        /// </summary>
        private long _bufferCurrentPosition;
        private long bufferCurrentPosition
        {
            get { return _bufferCurrentPosition; }
            set
            {
                _bufferCurrentPosition = value;
                if (_bufferCurrentPosition >= (long)BufferSize * 8)
                    _bufferCurrentPosition = 0;
            }
        }

        /// <summary>
        /// Total number of bits read from the stream.
        /// </summary>
        public ulong BitsRead { get; private set; }

        /// <summary>
        /// Total number of bytes read from the stream.
        /// </summary>
        public uint BytesRead { get { return (uint)(BitsRead / 8); } }

        private int bufferSize = 0;
        /// <summary>
        /// Number of bytes to read from the stream into the local buffer.
        /// </summary>
        public int BufferSize
        {
            get { return bufferSize; }
            set
            {
                //Clamp to range [minBufferSize, intMax]
                value = Math.Max(1, value);

                if (bufferSize == value)
                    return;
                int oldBufferSize = bufferSize;
                bufferSize = value;

                //Reset the buffer
                BaseStream.Position -= (long)(bufferAvailableBits / 8);
                if (oldBufferSize > 0)
                    bufferCurrentPosition %= (long)oldBufferSize;
                bufferAvailableBits = 0;

                Array.Resize(ref buffer, bufferSize);
            }
        }

        public BitReader()
            : this(new MemoryStream())
        { }

        public BitReader(Stream stream)
            : this(stream, new UTF8Encoding(false, true))
        { }

        public BitReader(Stream stream, Encoding encoding)
        {
            this.Encoding = encoding;

            BaseStream = stream;
            BufferSize = 16;
        }

        /// <summary>
        /// Reads a bit from the stream.
        /// </summary>
        /// <returns>1 or 0.</returns>
        public byte ReadBit()
        {
            EnsureCapacity(1);

            bool res = (buffer[bufferCurrentPosition / 8] & ((1 << (int)(7 - bufferCurrentPosition % 8)))) != 0;
            

            bufferCurrentPosition++;
            bufferAvailableBits--;
            BitsRead++;

            return res ? (byte)1 : (byte)0;
        }

        /// <summary>
        /// Reads a sequence of bits from the stream.
        /// </summary>
        /// <param name="count">The number of bits to read.</param>
        /// <returns>Bits ORed together into a sequence of bytes.</returns>
        public byte[] ReadBits(int count)
        {
            byte[] bytes = new byte[(int)Math.Ceiling(count / 8f)];
            int bit = 0;

            while (bit < count)
            {
                bool read = false;
                //Todo: Reimplement this
                //if (bufferCurrentPosition % 8 == 0)
                //{
                //    //Optimization when byte-aligned, read directly from buffer
                //    while (count - bit >= 8)
                //    {
                //        EnsureCapacity(8);

                //        bytes[bit / 8] = buffer[bufferCurrentPosition / 8];
                //        bufferCurrentPosition += 8;
                //        bufferAvailableBits -= 8;
                //        BitsRead += 8;
                //        bit += 8;
                //        read = true;
                //    }
                //}

                if (!read)
                {
                    bytes[bit / 8] |= (byte)(ReadBit() << (bit % 8));
                    bit++;
                }
            }

            return bytes;
        }

        /// <summary>
        /// Reads a byte from the stream.
        /// </summary>
        /// <returns>The byte.</returns>
        public byte ReadByte()
        {
            return ReadBits(8)[0];
        }

        /// <summary>
        /// Reads a sequence of bytes from the stream.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The bytes.</returns>
        public byte[] ReadBytes(int count)
        {
            return ReadBits(count * 8);
        }

        /// <summary>
        /// Reads a one-bit-encoded boolean from the stream.
        /// </summary>
        /// <returns>The bool.</returns>
        public bool ReadBool()
        {
            return ReadBit() == 1;
        }

        /// <summary>
        /// Reads an unsigned integer of a specific bit-length from the stream.
        /// </summary>
        /// <param name="numBits">The number of bits to read.</param>
        /// <returns>The unsigned integer.</returns>
        public uint ReadUInt(int numBits = 32)
        {
            if (numBits > 32 || numBits < 1)
                throw new ArgumentException("numBits");

            byte[] bytes = ReadBits(numBits);

            int retVal = 0;
            for (int i = 0; i < bytes.Length; i++)
                retVal |= bytes[i] << i * 8;

            return (uint)retVal;
        }

        /// <summary>
        /// Reads an unsigned long of a specific bit-length from the stream.
        /// </summary>
        /// <param name="numBits">The number of bits to read.</param>
        /// <returns>The unsigned long.</returns>
        public ulong ReadULong(int numBits = 64)
        {
            if (numBits > 64 || numBits < 1)
                throw new ArgumentException("numBits");

            byte[] bytes = ReadBits(numBits);

            long retVal = 0;
            for (int i = 0; i < bytes.Length; i++)
                retVal |= (long)bytes[i] << i * 8;

            return (ulong)retVal;
        }

        /// <summary>
        /// Reads a signed integer of a specific bit-length from the stream.
        /// </summary>
        /// <param name="numBits">The number of bits to read.</param>
        /// <returns>The signed integer.</returns>
        public int ReadInt(int numBits = 32)
        {
            if (numBits > 32 || numBits < 1)
                throw new ArgumentException("numBits");

            byte msb = ReadBit();
            numBits--;

            uint ret = ReadUInt(numBits);
            if (msb == 1)
            {
                for (int i = numBits; i <= 31; i++)
                    ret |= (uint)1 << i;
            }

            return (int)ret;
        }

        /// <summary>
        /// Reads a signed long of a specific bit-length from the stream.
        /// </summary>
        /// <param name="numBits">The number of bits to read.</param>
        /// <returns>The signed long.</returns>
        public long ReadLong(int numBits = 64)
        {
            if (numBits > 64 || numBits < 1)
                throw new ArgumentException("numBits");

            byte msb = ReadBit();
            numBits--;

            ulong ret = ReadULong(numBits);

            if (msb == 1)
            {
                for (int i = numBits; i <= 63; i++)
                    ret |= (ulong)1 << i;
            }

            return (long)ret;
        }

        ///// <summary>
        ///// Reads a float of a specific bit-length from the stream.
        ///// </summary>
        ///// <param name="numBits">The number of bits to read.</param>
        ///// <returns>The float.</returns>
        //public unsafe float ReadFloat(int numBits = 32)
        //{
        //    if (numBits > 32 || numBits < 1)
        //        throw new ArgumentException("numBits");

        //    uint tmp = ReadUInt(numBits);
        //    return *(float*)&tmp;
        //}

        /// <summary>
        /// Reads a double of a specific bit-length from the stream.
        /// </summary>
        /// <param name="numBits">The number of bits to read.</param>
        /// <returns>The double.</returns>
        //public unsafe double ReadDouble(int numBits = 64)
        //{
        //    if (numBits > 64 || numBits < 1)
        //        throw new ArgumentException("numBits");

        //    ulong tmp = ReadULong(numBits);
        //    return *(double*)&tmp;
        //}

        /// <summary>
        /// Reads a decimal of a specific bit-length from the stream.
        /// </summary>
        /// <param name="numBits">The number of bits to read.</param>
        /// <returns>The decimal.</returns>
        public decimal ReadDecimal(int numBits = 128)
        {
            if (numBits > 128 || numBits < 1)
                throw new ArgumentException("numBits");

            int[] bits = new int[4];
            for (int i = 0; i < numBits; i++)
                bits[i / 32] |= ReadBit() << i % 32;

            return new decimal(bits);
        }

        /// <summary>
        /// Reads one character from the stream.
        /// </summary>
        /// <returns>The char.</returns>
        public char ReadChar()
        {
            if (Encoding.GetType() == typeof(ASCIIEncoding))
                return (char)ReadUInt(8);
            else if (Encoding.GetType() == typeof(UTF8Encoding))
            {
                byte firstByte = ReadByte();
                if (firstByte >> 7 == 0)
                    return Encoding.GetString(new[] { firstByte })[0];

                int bytesToRead = 0;
                while ((firstByte & (1 << (6 - bytesToRead))) > 0 && bytesToRead < 7)
                    bytesToRead++;

                if (bytesToRead == 0)
                    throw new Exception("Malformed unicode format.");

                byte[] bytes = new byte[bytesToRead + 1];
                bytes[0] = firstByte;

                int ptr = 1;
                while (ptr <= bytesToRead)
                {
                    bytes[ptr] = ReadByte();
                    ptr++;
                }

                return Encoding.GetString(bytes)[0];
            }
            else if (Encoding.GetType() == typeof(UnicodeEncoding))
            {
                int w1 = (int)ReadUInt(16);
                if (w1 < 0xD800 || w1 > 0xDFFF)
                    return Encoding.GetString(BitConverter.GetBytes(w1))[0];
                int w2 = (int)ReadUInt(16);
                if (w2 < 0xDC00 || w2 > 0xDFFF)
                    throw new Exception("Malformed unicode format.");
                
                byte[] w1Bytes = BitConverter.GetBytes(w1);
                byte[] w2Bytes = BitConverter.GetBytes(w2);

                string s = Encoding.GetString(new[] { w1Bytes[0], w1Bytes[1], w2Bytes[0], w2Bytes[1] });
                return '0';
            }
            else if (Encoding.GetType() == typeof(UTF32Encoding))
                return Encoding.GetString(ReadBytes(4))[0];
            return '\0';
        }

        /// <summary>
        /// Reads a sequence of characters from the stream.
        /// </summary>
        /// <param name="count">The number of characters to read.</param>
        /// <returns>The chars.</returns>
        public char[] ReadChars(int count)
        {
            //Todo: Inefficient
            char[] chars = new char[count];
            for (int i = 0; i < count; i++)
                chars[i] = ReadChar();

            return chars;
        }

        /// <summary>
        /// Reads a variable-length encoded string from the stream.
        /// </summary>
        /// <returns>The string.</returns>
        public string ReadString()
        {
            return new string(ReadChars(Read7BitEncodedInt()));
        }

        protected void EnsureCapacity(long bitCount)
        {
            if (bufferAvailableBits - bitCount < 0)
                bufferAvailableBits = (long)BaseStream.Read(buffer, 0, BufferSize - (int)(bufferAvailableBits / 8)) * 8;
        }

        /// <summary>
        /// See http://referencesource.microsoft.com/#mscorlib/system/io/binaryreader.cs,569
        /// </summary>
        protected int Read7BitEncodedInt()
        {
            int count = 0;
            int shift = 0;
            byte b;
            do
            {
                b = ReadByte();
                count |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);
            return count;
        }

        #region Disposal

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            buffer = null;

            if (BaseStream != null)
                BaseStream.Dispose();

            BaseStream = null;
        }

        ~BitReader()
        {
            Dispose(false);
        }

        #endregion
    }
}
