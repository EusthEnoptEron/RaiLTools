using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailTools
{
    public class LWGItem
    {
        public string Path;
        public byte[] Content;

        public int X;
        public int Y;
        public byte Flag;
        
        internal LWGItem() { }
        public LWGItem(string path, byte[] content)
        {
            Path = path;
            Content = content;
        }
    }

    public enum LWGFlags
    {
        Image1 = 41,
        Image2 = 40,
        Image3 = 9,
        String = 56
    }

    public class LWGCanvas : Dictionary<string, LWGItem>
    {
        private const string _Magic = "LG\x01\x00";
        private Encoding _ShiftJIS = Encoding.GetEncoding(932);
        public int Width = 0;
        public int Height = 0;


        private class InternalLWGItem : LWGItem
        {
            public int Offset;
            public int Size;
        }

        public LWGCanvas(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        public LWGCanvas(string path)
        {
            using (var fileStream = File.OpenRead(path))
            {
                this.Decompress(fileStream);
            }
        }
        
        public void Save(string path)
        {
            if (File.Exists(path)) File.Delete(path); // Truncate
            using (var fileStream = File.OpenWrite(path))
            {
                Encompress(fileStream);
            }
        }

        public static LWGCanvas FromFile(string path)
        {
            return new LWGCanvas(path);
        }

        public void AddImage(string path, WCGImage image, int? x = null, int? y = null)
        {
            //Width = Math.Max(image.Width, Width);
            //Height = Math.Max(image.Height, Height);

            byte[] data;
            using (var memory = new MemoryStream())
            {
                image.Save(memory);
                data = memory.ToArray();
            }

            // Chop of .wcg if needed
            if (path.EndsWith(".wcg"))
                path = path.Substring(0, path.Length - 4);

            byte flag = 0;
            if (ContainsKey(path))
            {
                // File already exists
                flag = this[path].Flag;

                if (x == null)
                {
                    x = this[path].X;
                    y = this[path].Y;
                }
            }

            this[path] = new LWGItem(path, data)
            {
                X = x ?? 0,
                Y = y ?? 0,
                Flag = flag
            };
        }


        /// <summary>
        /// Fills the object.
        /// </summary>
        private void Decompress(Stream input)
        {
            using (var reader = new BinaryReader(input))
            {
                // READ HEADER
                var magic = new string(reader.ReadChars(_Magic.Length));
                if (magic != _Magic) throw new InvalidDataException("Not a LWG archive.");

                Height = reader.ReadInt32();
                Width = reader.ReadInt32();
                int fileCount = reader.ReadInt32();

                //var dummy = reader.ReadInt32();
                input.Seek(4, SeekOrigin.Current); // Skip 4

                int tableSize = reader.ReadInt32();
                int fileDataStart = (int)input.Position + tableSize + 4;
                var flagList = new HashSet<byte>();

                // Parse file table
                for (int i = 0; i < fileCount; i++)
                {
                    var item = new InternalLWGItem();

                    item.X = reader.ReadInt32();
                    item.Y = reader.ReadInt32();
                    item.Flag = reader.ReadByte();

                    flagList.Add(item.Flag);

                    item.Offset = fileDataStart + reader.ReadInt32();
                    item.Size = reader.ReadInt32();
                    var nameSize = (int)reader.ReadByte();

                    item.Path = _ShiftJIS.GetString(reader.ReadBytes(nameSize));

                    this[item.Path] = item;
                }

                // Extract actual data
                foreach (var item in this.Values.OfType<InternalLWGItem>() )
                {
                    input.Seek(item.Offset, SeekOrigin.Begin);
                    item.Content = reader.ReadBytes(item.Size);
                }

                var position = input.Position;
                var length = input.Length;

            }

        }

        private void Encompress(Stream output)
        {
            using (var writer = new BinaryWriter(output))
            {
                writer.Write(_Magic.ToArray());
                writer.Write(Height);
                writer.Write(Width);

                writer.Write(Count);

                // Dummy
                writer.Write(new byte[] { 0, 0, 0, 0 });

                var items = Values;
                int tableSize = Values.Aggregate(0, (left, item) => left + _ShiftJIS.GetByteCount(item.Path) + 9 + 4 + 4 + 1);
                writer.Write(tableSize);

                int bytesWritten = 0;

                // Write table
                foreach (var item in items)
                {
                    var itemPath = _ShiftJIS.GetBytes(item.Path);
                    // Write 9 bytes
                    writer.Write(item.X);
                    writer.Write(item.Y);

                    writer.Write(item.Flag);

                    writer.Write(bytesWritten);
                    writer.Write(item.Content.Length);
                    writer.Write((byte)itemPath.Length);
                    writer.Write(itemPath);

                    bytesWritten += item.Content.Length;
                }

                // Write actual content
                writer.Write(0); // Skip 4 bytes
                foreach (var item in items)
                {
                    writer.Write(item.Content);
                }
            }
        }


    }
}
