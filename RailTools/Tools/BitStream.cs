using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaiLTools.Tools
{
    /// <summary>
    /// Wrapper for <see cref="Stream"/>s that allows bit-level reads and writes.
    /// </summary>
    internal sealed class BitStream : Stream
    {
        private readonly Stream stream;

        private byte currentByte;

        /// <summary>
        /// Gets or sets the position inside the byte.
        /// <para/>
        /// <see cref="BitNum.MaxValue"/> is the last position before the next byte.
        /// </summary>
        public BitNum BitPosition { get; set; }

        #region Proxy Properties

        public override bool CanRead
        {
            get { return stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return stream.CanSeek; }
        }

        public override bool CanTimeout
        {
            get { return stream.CanTimeout; }
        }

        public override bool CanWrite
        {
            get { return stream.CanWrite; }
        }

        public override long Length
        {
            get { return stream.Length; }
        }

        public override long Position
        {
            get { return stream.Position; }
            set { stream.Position = value; }
        }

        public override int ReadTimeout
        {
            get { return stream.ReadTimeout; }
            set { stream.ReadTimeout = value; }
        }

        public Stream UnderlayingStream
        {
            get { return stream; }
        }

        public override int WriteTimeout
        {
            get { return stream.WriteTimeout; }
            set { stream.WriteTimeout = value; }
        }

        #endregion Proxy Properties

        /// <summary>
        /// Creates a new instance of the <see cref="BitStream"/> class with the given underlaying stream.
        /// </summary>
        /// <param name="underlayingStream">The underlaying stream to work on.</param>
        public BitStream(Stream underlayingStream)
        {
            BitPosition = BitNum.MaxValue;
            stream = underlayingStream;
        }

        #region Proxy Methods

        public override bool Equals(object obj)
        {
            return stream.Equals(obj);
        }

        public override void Flush()
        {
            if (BitPosition != BitNum.MaxValue)
            {
                stream.WriteByte(currentByte);
                currentByte = 0;
            }

            stream.Flush();
        }

        public override int GetHashCode()
        {
            return stream.GetHashCode();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }

        public override string ToString()
        {
            return stream.ToString();
        }

        protected override void Dispose(bool disposing)
        {
            stream.Dispose();
        }

        #endregion Proxy Methods

        #region Read Methods

        /// <summary>
        /// Reads the given number of bytes into the buffer, starting at the given offset and returns how many bytes were read.
        /// <para/>
        /// Any bytes that could not be read will be set to 0.
        /// </summary>
        /// <param name="buffer">The buffer to read into.</param>
        /// <param name="offset">The offset to start writing into the buffer at.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>How many bytes were actually read.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (BitPosition == BitNum.MaxValue)
                return stream.Read(buffer, offset, count);

            return (int)(ReadBits(buffer, offset, (uint)count * BitNum.MaxValue) / BitNum.MaxValue);
        }

        /// <summary>
        /// Reads the given number of bits into the value and returns whether the stream could be read from or not.
        /// </summary>
        /// <param name="value">The value of the read bits.</param>
        /// <param name="bits">The number of bits to read.</param>
        /// <returns>Whether the stream could be read from or not.</returns>
        public bool ReadBits(out byte value, BitNum bits)
        {
            if (BitPosition == BitNum.MaxValue && bits == BitNum.MaxValue)
            {
                var readByte = stream.ReadByte();
                value = (byte)(readByte < 0 ? 0 : readByte);
                currentByte = value;
                return !(readByte < 0);
            }

            value = 0;
            for (byte i = 1; i <= bits; ++i)
            {
                if (BitPosition == BitNum.MaxValue)
                {
                    var readByte = stream.ReadByte();

                    if (readByte < 0)
                        return i > 1;

                    currentByte = (byte)readByte;
                }

                advanceBitPosition();
                value |= getAdjustedValue(currentByte, BitPosition, (BitNum)i);
            }

            return true;
        }

        /// <summary>
        /// Reads the given number of bits into the buffer, starting at the given offset and returns how many bits were read.
        /// <para/>
        /// Any bytes that could not be read will be set to 0.
        /// </summary>
        /// <param name="buffer">The buffer to read into.</param>
        /// <param name="offset">The offset to start writing into the buffer at.</param>
        /// <param name="count">The number of bits to read.</param>
        /// <returns>How many bits were actually read.</returns>
        public ulong ReadBits(byte[] buffer, int offset, ulong count)
        {
            var bitsRead = 0uL;
            while (count > 0)
            {
                byte nextByte;
                var bits = (BitNum)count;

                if (!ReadBits(out nextByte, bits))
                    buffer[offset] = 0;
                else
                {
                    buffer[offset] = (byte)nextByte;
                    bitsRead += bits;
                }

                ++offset;
                count -= bits;
            }

            return bitsRead;
        }

        /// <summary>
        /// Reads a single byte from the stream and returns its value, or -1 if it could not be read.
        /// </summary>
        /// <returns>The value that was read, or -1 if it could not be read.</returns>
        public override int ReadByte()
        {
            byte buffer;
            return ReadBits(out buffer, BitNum.MaxValue) ? buffer : -1;
        }

        #endregion Read Methods

        #region Write Methods

        /// <summary>
        /// Writes the given number of bytes from the buffer, starting at the given offset.
        /// </summary>
        /// <param name="buffer">The buffer to write from.</param>
        /// <param name="offset">The offet to start reading from at.</param>
        /// <param name="count">The number of bytes to write.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (BitPosition == BitNum.MaxValue)
            {
                stream.Write(buffer, offset, count);
                currentByte = 0;
            }
            else
            {
                WriteBits(buffer, offset, (ulong)count * BitNum.MaxValue);
            }
        }

        /// <summary>
        /// Writes the given number of bits from the buffer, starting at the given offset.
        /// </summary>
        /// <param name="buffer">The buffer to write from.</param>
        /// <param name="offset">Te offset to start reading from at.</param>
        /// <param name="count">The number of bits to write.</param>
        public void WriteBits(byte[] buffer, int offset, ulong count)
        {
            while (count > 0)
            {
                var bits = (BitNum)count;

                WriteBits(buffer[offset], bits);

                ++offset;
                count -= bits;
            }
        }

        /// <summary>
        /// Writes the given number of bits from the value.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="bits">The number of bits to write.</param>
        public void WriteBits(byte value, BitNum bits)
        {
            if (BitPosition == BitNum.MaxValue && bits == BitNum.MaxValue)
            {
                stream.WriteByte(value);
                currentByte = 0;
                return;
            }

            for (byte i = 1; i <= bits; ++i)
            {
                advanceBitPosition();
                currentByte |= getAdjustedValue(value, (BitNum)(9-i), (BitNum)(9-BitPosition)); // [CHANGE]

                if (BitPosition == BitNum.MaxValue)
                {
                    stream.WriteByte(currentByte);
                    currentByte = 0;
                }
            }
        }

        /// <summary>
        /// Writes the value.
        /// </summary>
        /// <param name="value">The value to write.</param>
        public override void WriteByte(byte value)
        {
            WriteBits(value, BitNum.MaxValue);
        }

        #endregion Write Methods

        private static byte getAdjustedValue(byte value, BitNum currentPosition, BitNum targetPosition)
        {
            value &= currentPosition.GetBitPos();

            if (currentPosition > targetPosition)
                return (byte)(value >> (currentPosition - targetPosition));
            else if (currentPosition < targetPosition)
                return (byte)(value << (targetPosition - currentPosition));
            else
                return value;
        }

        private void advanceBitPosition()
        {
            if (BitPosition == BitNum.MaxValue)
                BitPosition = BitNum.MinValue;
            else
                BitPosition = (BitNum)(BitPosition + 1);
        }
    }
}
