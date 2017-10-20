using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace RaiLTools
{

    [XmlType("Item")]
    public class LwgEntry
    {

        [XmlText]
        public string Path;

        [XmlIgnore]
        public byte[] Content;

        [XmlAttribute("x")]
        public int X;
        [XmlAttribute("y")]
        public int Y;
        [XmlAttribute("flag")]
        public byte Flag;

        internal int Offset;
        internal int Size;


        internal LwgEntry() { }
        
        public LwgEntry(string path, byte[] content)
        {
            Path = path;
            Content = content;
        }

        public bool IsImage
        {
            get
            {
                return Flag != (byte)LWGFlags.String && Content.Length > 0;
            }
        }

        public WcgImage ToWCG()
        {
            using(var memoryStream = new MemoryStream(Content)) {
                return WcgImage.FromStream(memoryStream);
            }
        }

        public string GuessExtension()
        {
            if (Content[0] == 0x57 && Content[1] == 0x47)
                return ".wcg";
            if (Content[0] == 0x42 && Content[1] == 0x4D)
                return ".msk";
            return "";
        }
    }

    public enum LWGFlags
    {
        Image1 = 41,
        Image2 = 40,
        Image3 = 9,
        String = 56
    }

    [XmlType("Canvas")]
    public class LwgCanvas
    {
        private const string _Magic = "LG\x01\x00";
        private Encoding _ShiftJIS = Encoding.GetEncoding(932);
        public int Width = 0;
        public int Height = 0;

        private List<LwgEntry> _Entries = new List<LwgEntry>();

        [XmlArray("Items")]
        public List<LwgEntry> Entries
        {
            get
            {
                return _Entries;
            }
            set
            {
                _Entries = value;
            }
        }

        private LwgCanvas() : this(0, 0) { }

        public LwgCanvas(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        public LwgCanvas(string path)
        {
            using (var fileStream = File.OpenRead(path))
            {
                this.Decode(fileStream);
            }
        }
        
        public void Save(string path)
        {
            if (File.Exists(path)) File.Delete(path); // Truncate
            using (var fileStream = File.OpenWrite(path))
            {
                Encode(fileStream);
            }
        }


        public bool HasEntry(string path)
        {
            path = Path.GetFileNameWithoutExtension(path);
            return _Entries.Any(e => e.Path == path);
        }

        public LwgEntry GetEntry(string path)
        {
            path = Path.GetFileNameWithoutExtension(path);
            return _Entries.First(e => e.Path == path);
        }

        private void SaveMetaFile(string path)
        {
            var serializer = new XmlSerializer(typeof(LwgCanvas));

            if(File.Exists(path)) File.Delete(path);

            using(var stream = File.OpenWrite(path)) {
                serializer.Serialize(stream, this);
            }
        }

        public void ExportToDirectory(string path, bool aggressive = false) {
            path = Path.GetFullPath(path);

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            // Write meta file
            SaveMetaFile(Path.Combine(path, ".meta.xml"));
            foreach (var entry in _Entries)
            {
                if (entry.IsImage)
                {
                    var extension = entry.GuessExtension();
                    var content   = entry.Content;

                    if (aggressive && extension == ".wcg")
                    {
                        extension = ".png";
                        using(var memory = new MemoryStream()) {
                            entry.ToWCG().ToImage().Save(memory, ImageFormat.Png);
                            content = memory.ToArray();
                        }
                    }

                    var ePath = Path.Combine(path, entry.Path + extension);
                    if(File.Exists(ePath)) File.Delete(ePath);

                    File.WriteAllBytes(ePath, content);
                }
            }
        }

        public static LwgCanvas FromDirectory(string path, bool aggressive = false)
        {
            path = Path.GetFullPath(path);
            var metaPath = Path.Combine(path, ".meta.xml");

            if (File.Exists(metaPath))
            {
                var serializer = new XmlSerializer(typeof(LwgCanvas));

                using (var stream = File.OpenRead(metaPath))
                {
                    var canvas = serializer.Deserialize(stream) as LwgCanvas;

                    foreach (var entry in canvas._Entries)
                    {
                        FillEntry(entry, path, aggressive);
                    }

                    return canvas;
                }
            }
            else
            {
                throw new Exception("Meta file not found!");

                //var files = Directory.GetFiles(path).Select(f => Path.GetFileNameWithoutExtension(f)).Distinct();
                //foreach (var file in files)
                //{
                //    var ePath = Path.Combine(path, file);
                //    var entry = new LwgEntry();
                //    entry.Path = file;
                //    entry.Flag = (byte)LWGFlags.Image2;

                //    FillEntry(entry, path, aggressive);
                //}
            }
        }


        private static void FillEntry(LwgEntry entry, string path, bool aggressive)
        {
            string[] fileEndings = new string[] { ".wcg", ".msk" };
            entry.Content = new byte[0];

            if (entry.Flag != (byte)LWGFlags.String)
            {
                string ePath = Path.Combine(path, entry.Path);
                if (aggressive && File.Exists(ePath + ".png"))
                {
                    var img = WcgImage.FromImage(ePath + ".png");
                    using (var memory = new MemoryStream())
                    {
                        img.Save(memory);
                        entry.Content = memory.ToArray();
                    }
                }
                else
                {
                    foreach (var ext in fileEndings)
                    {
                        if (File.Exists(ePath + ext))
                        {
                            entry.Content = File.ReadAllBytes(ePath + ext);
                        }
                    }
                }
            }
        }

        public static LwgCanvas FromFile(string path)
        {
            return new LwgCanvas(path);
        }

        public void ReplaceImage(string path, WcgImage image, int? x = null, int? y = null)
        {
            //Width = Math.Max(image.Width, Width);
            //Height = Math.Max(image.Height, Height);
            path = Path.GetFileNameWithoutExtension(path);

            byte[] data;
            using (var memory = new MemoryStream())
            {
                image.Save(memory);
                data = memory.ToArray();
            }


            byte flag = 0;
            LwgEntry entry = null;
            if (HasEntry(path))
            {
                entry = GetEntry(path);
            }
            else
            {
                entry = new LwgEntry();
                entry.Path = path;

                _Entries.Add(entry);
            }

            entry.Content = data;
            entry.X = x ?? entry.X;
            entry.Y = y ?? entry.Y; 
        }


        /// <summary>
        /// Fills the object.
        /// </summary>
        private void Decode(Stream input)
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
                    var item = new LwgEntry();

                    item.X = reader.ReadInt32();
                    item.Y = reader.ReadInt32();
                    item.Flag = reader.ReadByte();

                    flagList.Add(item.Flag);

                    item.Offset = fileDataStart + reader.ReadInt32();
                    item.Size = reader.ReadInt32();
                    var nameSize = (int)reader.ReadByte();

                    item.Path = _ShiftJIS.GetString(reader.ReadBytes(nameSize));

                    _Entries.Add(item);
                }

                // Extract actual data
                foreach (var item in _Entries)
                {
                    input.Seek(item.Offset, SeekOrigin.Begin);
                    item.Content = reader.ReadBytes(item.Size);
                }

                var position = input.Position;
                var length = input.Length;

            }

        }

        private void Encode(Stream output)
        {
            using (var writer = new BinaryWriter(output))
            {
                writer.Write(_Magic.ToArray());
                writer.Write(Height);
                writer.Write(Width);

                writer.Write(_Entries.Count);

                // Dummy
                writer.Write(new byte[] { 0, 0, 0, 0 });

                int tableSize = _Entries.Aggregate(0, (left, item) => left + _ShiftJIS.GetByteCount(item.Path) + 9 + 4 + 4 + 1);
                writer.Write(tableSize);

                int bytesWritten = 0;

                // Write table
                foreach (var item in _Entries)
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
                foreach (var item in _Entries)
                {
                    writer.Write(item.Content);
                }
            }
        }


    }
}
