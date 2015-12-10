using RailTools.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RailTools
{
    /// <summary>
    /// Represents an image file in the WCG format.
    /// Internally, the image is managed as a System.Drawing.Bitmap.
    /// </summary>
    public class WCGImage : IDisposable
    {
        private Bitmap _Bitmap;

        private WCGImage() { }
        public WCGImage(Stream fileStream)
        {
            Parse(fileStream);
        }

        public int Width
        {
            get
            {
                return _Bitmap.Width;
            }
        }
        public int Height
        {
            get
            {
                return _Bitmap.Height;
            }
        }

        /// <summary>
        /// Creates a new WCG object from a WCG file.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static WCGImage FromFile(string file)
        {
            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read))
            {
                return new WCGImage(stream);
            }
        }

        public static WCGImage FromImage(string file)
        {
            using (var img = Image.FromFile(file))
            {
                return WCGImage.FromImage(img);
            }
        }

        /// <summary>
        /// Creates a new WCG object from a bitmap.
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static WCGImage FromImage(Image img) {
            //var pxl = img.PixelFormat;
            return new WCGImage()
            {
                _Bitmap = new Bitmap(img)
            };
        }

        public Image ToImage()
        {
            return new Bitmap(_Bitmap);
        }

        private void Parse(Stream fileStream)
        {
            using (var reader = new BinaryReader(fileStream, Encoding.GetEncoding(932)))
            {
                string magic = new string(reader.ReadChars(2));
                if (magic != "WG") throw new BadImageFormatException("Unsupported file format.");

                reader.BaseStream.Seek(2, SeekOrigin.Current);

                var depth = reader.ReadUInt16();

                reader.BaseStream.Seek(2, SeekOrigin.Current);

                var width = reader.ReadUInt32();
                var height = reader.ReadUInt32();

                var canvasSize = width * height;
                var output = new byte[canvasSize * 4];
                var data = new BitmapData();
                
                Decompress(reader, output, 2, 4, 2);
                Decompress(reader, output, 0, 4, 2);

                for (int i = 0; i < output.Length; i += 4)
                {
                    output[i + 3] ^= 0xFF;
                }

                _Bitmap = BuildBitmap((int)width, (int)height, output);
            }
        }

        public void Save(string path)
        {
            path = Path.GetFullPath(path);
            if (File.Exists(path)) File.Delete(path);
            
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var stream = File.OpenWrite(path))
            {
                Save(stream);
            }
           
        }

        public void Save(Stream stream)
        {
            using (var writer = new BinaryWriter(stream))
            {
                /* WRITE FILE HEADER */
                writer.Write(new char[] { 'W', 'G' }); // Header
                writer.Write(new byte[] { 0x71, 0x02 }); // Dummy

                writer.Write((ushort)32); // Depth
                writer.Write(new byte[] { 0, 0x40 }); // Dummy
                writer.Write(_Bitmap.Width);
                writer.Write(_Bitmap.Height);

                var imgData = _Bitmap.LockBits(new Rectangle(0, 0, _Bitmap.Width, _Bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                var data = new byte[_Bitmap.Width * _Bitmap.Height * 4];
                Marshal.Copy(imgData.Scan0, data, 0, data.Length);
                _Bitmap.UnlockBits(imgData);

                Encompress(writer, true, data);
                Encompress(writer, false, data);

                writer.Flush();
            }
        }

        private void Encompress(BinaryWriter writer, bool low, byte[] data)
        {
            int pixels = data.Length;
            int shift = low ? 2 : 0;

            List<ushort> palette = new List<ushort>();
            var LUT = new Dictionary<ushort, long>();

            using (var memory = new MemoryStream())
            using (var memoryWriter = new BitWriter(memory))
            {
                int paletteSize = 0x1000;

                var bitSize = paletteSize < 0x1000 ? 3 : 4;
                var loopThreshold = (1 << bitSize) - 2;

                for (int px = shift; px < pixels; px += 4)
                {
                    // Invert for correct endianness
                    if (low) data[px+1] ^= 0xFF; // Flip

                    var color = BitConverter.ToUInt16(new byte[] { data[px], data[px+1] }, 0);
                    
                    long paletteOffset;
                    if (LUT.ContainsKey(color))
                    {
                        paletteOffset = LUT[color];
                    }
                    else
                    {
                        paletteOffset = palette.Count;
                        palette.Add(color);
                        LUT[color] = paletteOffset;
                    }

                    byte paletteOffsetSize =  (byte)(paletteOffset == 0 
                        ? 1
                        : (Math.Floor(Math.Log(paletteOffset, 2)) + 1));

                    //byte paletteOffsetSize = 6;
                    memoryWriter.WriteWithBits((byte)(Math.Min( loopThreshold + 1 , (int)paletteOffsetSize)), bitSize);

                    //memoryWriter.WriteBits(byte.MinValue, BitNum.MinValue); // Break
                    if (paletteOffsetSize > 1)
                    {
                        // Ignore first bit
                        //0b10 -> 0b00 or 0b11 -> 0b01
                        paletteOffsetSize--;

                        if (paletteOffsetSize > loopThreshold - 1)
                        {
                            if (paletteOffsetSize > loopThreshold)
                                memoryWriter.WriteWithBits(uint.MaxValue, paletteOffsetSize - loopThreshold);
                                //memoryWriter.WriteBits(byte.MaxValue, (BitNum)(paletteOffsetSize - loopThreshold - 1));
                             memoryWriter.WriteBits(byte.MinValue, BitNum.MinValue); // Break
                        }

                        paletteOffset &= (int)((int)(1 << paletteOffsetSize) ^ uint.MaxValue);
                    }

                    memoryWriter.WriteWithBits((uint)paletteOffset, paletteOffsetSize);
                }

                memoryWriter.Flush();
                var memoryArray = memory.ToArray();

                while (palette.Count <= paletteSize)
                {
                    // Add dummies
                    palette.Add(0x00);
                }

                //for (int i = 0; i < 32; i++) palette.Insert(0, 0); // Add dummy values

                // Write RLE header
                writer.Write((uint)(data.Length / 2)); // full size
                writer.Write((uint)(memoryArray.Length));  // data size
                writer.Write((ushort)(palette.Count)); // palette size

                writer.Write((ushort)0);

                // Write palette
                foreach (var col in palette)
                { 
                    writer.Write(col);
                }

                // Write actual data
                writer.Write(memoryArray);
            }
        }


        private void Decompress(BinaryReader reader, byte[] output, int outputOffset, int outputShift, int inputShift)
        {
        //    auto output_ptr = output.get<u8>() + output_offset;
        //    const auto output_end = output.end<const u8>();
            int outputPtr = outputOffset;
           
            if(outputShift < inputShift)
                throw new Exception("Invalid shift");

            var sizeOrig = reader.ReadUInt32();
            var sizeComp = reader.ReadUInt32();
            var tableSize = reader.ReadUInt16();

            if (tableSize == 0)
                throw new Exception("No palette entries found.");
        //    if (size_orig != ((output.size() / output_shift) * input_shift))
        //        throw err::BadDataSizeError();

            //var junk1 = reader.ReadByte();
            //var junk2 = reader.ReadByte();
            var junk = reader.ReadUInt16();

            var palette = reader.ReadBytes((int)(tableSize * inputShift)); // read #tableSize uint16s

            //var unk1 = tableSize < 0x1000 ? 6 : 0xE;
            var bitSize = tableSize < 0x1000 ? 3 : 4;
            var unk1 = (1 << bitSize) - 2;

            //var body = reader.ReadBytes((int)sizeComp);  //    io::BitReader bit_reader(input_stream.read(size_comp));
            var bytes = reader.ReadBytes((int)sizeComp);
            using(var bodyStream = new MemoryStream(bytes)) 
            {
                var bitReader = new SimpleBitReader(bodyStream.ToArray());

                while (outputPtr < output.Length)
                {
                    try
                    {
                        uint sequenceSize = 1u;
                        uint paletteOffsetSize = bitReader.ReadUInt(bitSize);
                        
                        if (paletteOffsetSize == 0)
                        {
                            sequenceSize = bitReader.ReadUInt(4) + 2;
                            paletteOffsetSize = bitReader.ReadUInt(bitSize);
                        }
                        if (paletteOffsetSize == 0)
                        {
                            throw new BadImageFormatException("Bad data size");
                        }

                        var paletteOffset = 0u;
                        if (paletteOffsetSize == 1)
                        {
                            paletteOffset = bitReader.ReadUInt(1);
                        }
                        else
                        {
                            paletteOffsetSize--;
                            if (paletteOffsetSize >= unk1)
                            {
                                while (bitReader.ReadBool())
                                    paletteOffsetSize++;
                            }
                            paletteOffset = 1u << (int)paletteOffsetSize;
                            paletteOffset |= bitReader.ReadUInt((int)paletteOffsetSize);
                        }
                        if ((paletteOffset + 1) * inputShift > palette.Length)
                        {
                            throw new Exception("Offset error.");
                        }

                        var inputChunk = new byte[inputShift];
                        Buffer.BlockCopy(palette, (int)(paletteOffset * inputShift), inputChunk, 0, inputShift);
                        //var inputChunk = palette.Skip((int)(paletteOffset * inputShift)).Take(inputShift).ToArray();

                        for (int i = 0; i < sequenceSize; i++)
                        {
                            for (int j = 0; j < inputShift; j++)
                            {
                                if (outputPtr >= output.Length)
                                    break;
                                output[outputPtr++] = inputChunk[j];
                            }
                            outputPtr += outputShift - inputShift;
                        }
                    }
                    catch (Exception e)
                    {
                        break;
                    }

                }
            }
           
        }

        private Bitmap BuildBitmap(int width, int height, byte[] imageData)
        {
            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            using (var stream = new MemoryStream(imageData))
            {
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0,
                                                                bmp.Width,
                                                                bmp.Height),
                                                  ImageLockMode.WriteOnly,
                                                  bmp.PixelFormat);

                Marshal.Copy(imageData, 0, bmpData.Scan0, imageData.Length);

                bmp.UnlockBits(bmpData);
            }
            return bmp;
        }


        public void Dispose()
        {
            _Bitmap.Dispose();
        }


    }
}
