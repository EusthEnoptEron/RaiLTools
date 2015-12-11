using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaiLTools
{
    public class XflArchiveEntry
    {
        internal long Offset;
        internal int Size;

        internal XflArchiveEntry() { }

        public string Path { get; internal set; }
        public byte[] Content { get; set; }

    }


    public class XflArchive
    {
        private Dictionary<string, XflArchiveEntry> _Entries;
        private const string MAGIC = "LB\x01\x00";
        private Encoding _ShiftJIS = Encoding.GetEncoding(932);

        public XflArchive()
        {
            _Entries = new Dictionary<string, XflArchiveEntry>();
        }

        public static XflArchive FromFile(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return XflArchive.FromStream(stream);
            }
        }
        public static XflArchive FromStream(Stream input)
        {
            var archive = new XflArchive();
            archive.ReadStream(input);

            return archive;
        }

        public ReadOnlyCollection<XflArchiveEntry> Entries
        {
            get
            {
                return new ReadOnlyCollection<XflArchiveEntry>(_Entries.Values.ToList());
            }
        }

        public XflArchiveEntry GetEntry(string path)
        {
            return _Entries[path];
        }

        /// <summary>
        /// Extracts all the files in the xfl archive to a directory on the file system.
        /// </summary>
        /// <param name="path"></param>
        public void ExtractToDirectory(string path)
        {
            path = Path.GetFullPath(path);

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            foreach (var entry in _Entries.Values)
            {
                var ePath = Path.Combine(path, entry.Path);
                
                if (File.Exists(ePath)) File.Delete(ePath);

                File.WriteAllBytes(ePath, entry.Content);
            }
        }

        public XflArchiveEntry CreateEntry(string path, byte[] content = null) {
            var entry = new XflArchiveEntry();
            entry.Path = path;
            entry.Content = content ?? new byte[0];

            _Entries[path] = entry;

            return entry;
        }

        /// <summary>
        /// Adds the (1-dimensional) contents of an entire directory.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        public void AddDirectory(string path, string searchPattern = "*")
        {
            foreach (var file in Directory.GetFiles(path, searchPattern))
            {
                string fileName = Path.GetFileName(file);
                CreateEntry(fileName, File.ReadAllBytes(file));
            }
        }

        public bool HasEntry(string path)
        {
            return _Entries.ContainsKey(path);
        }

        private void ReadStream(Stream input)
        {
            using (var reader = new BinaryReader(input))
            {
                // READ HEADER
                var magic = new string(reader.ReadChars(MAGIC.Length));
                if (magic != MAGIC) throw new FileFormatException("Not an XFL archive.");

                var tableSize = reader.ReadInt32();
                var fileCount = reader.ReadInt32();
                var fileStart = input.Position + tableSize;
                var entries = new List<XflArchiveEntry>();

                // READ TABLE
                for (int i = 0; i < fileCount; i++)
                {
                    var entry = new XflArchiveEntry();

                    var bytes = reader.ReadBytes(0x20).TakeWhile(v => v != 0).ToArray();
                    entry.Path = _ShiftJIS.GetString(bytes);
                    entry.Offset = fileStart + reader.ReadInt32();
                    entry.Size = reader.ReadInt32();

                    entries.Add(entry);
                }

                // READ FILES
                foreach (var entry in entries)
                {
                    input.Seek(entry.Offset, SeekOrigin.Begin);
                    entry.Content = reader.ReadBytes(entry.Size);

                    _Entries[entry.Path] = entry;
                }
            }
        }

        public void Save(string file)
        {
            if (File.Exists(file)) File.Delete(file);
            using(var stream = File.OpenWrite(file))
            {
                Save(stream);
            }
        }

        public void Save(Stream output)
        {
            int tableEntrySize = 0x20 + 4 + 4;
            using (var writer = new BinaryWriter(output))
            {
                writer.Write(MAGIC.ToCharArray());
                writer.Write(tableEntrySize * _Entries.Count);
                writer.Write(_Entries.Count);

                uint memoryWritten = 0;

                // Build table
                foreach (var entry in _Entries.Values)
                {
                    var nameBytes = _ShiftJIS.GetBytes(entry.Path);
                    writer.Write(nameBytes);
                    writer.Write(new byte[0x20 - nameBytes.Length]); // Fill space with 0s

                    writer.Write(memoryWritten);
                    writer.Write(entry.Content.Length);

                    memoryWritten += (uint)entry.Content.Length;
                }

                // Write actual content
                foreach (var entry in _Entries.Values)
                {
                    writer.Write(entry.Content);
                }
            }
        }
    }
}
