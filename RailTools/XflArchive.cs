using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaiLTools
{
    /// <summary>
    /// Represents an entry in a XFL archive.
    /// </summary>
    public class XflArchiveEntry
    {
        internal long Offset;
        internal int Size;

        /// <summary>
        /// Archive this entry belongs to.
        /// </summary>
        public XflArchive Archive { get; internal set; }
        /// <summary>
        /// Path where this entry is stored at.
        /// </summary>
        public string Path { get; internal set; }

        /// <summary>
        /// Binary data of this file.
        /// </summary>
        public byte[] Content { get; set; }


        internal XflArchiveEntry(XflArchive archive) {
            Archive = archive;
        }


        /// <summary>
        /// Removes the entry from the archive.
        /// </summary>
        public void Remove()
        {
            if (Archive != null)
                Archive.Remove(this);
            else
                throw new InvalidOperationException("Entry is not being stored in any archive!");
        }
    }


    /// <summary>
    /// Provides ways to add, remove, and change the contents of an XFL archive.
    /// </summary>
    public class XflArchive
    {
        private Dictionary<string, XflArchiveEntry> _Entries;
        private const string MAGIC = "LB\x01\x00";
        private Encoding _ShiftJIS = Encoding.GetEncoding(932);

        /// <summary>
        /// Creates an empty XFL archive.
        /// </summary>
        public XflArchive()
        {
            _Entries = new Dictionary<string, XflArchiveEntry>();
        }

        #region Public API

        /// <summary>
        /// Creates an XFL archive from its file representation.
        /// </summary>
        /// <param name="path">Location where the file is stored.</param>
        /// <returns>An instance of XflArchive populated with its files.</returns>
        public static XflArchive FromFile(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return XflArchive.FromStream(stream);
            }
        }

        /// <summary>
        /// Creates an XFL archive from its file representation via stream.
        /// </summary>
        /// <param name="input">Stream that holds the contents of the archive.</param>
        /// <returns>An instance of XflArchive populated with its files.</returns>
        public static XflArchive FromStream(Stream input)
        {
            var archive = new XflArchive();
            archive.ReadStream(input);

            return archive;
        }

        /// <summary>
        /// Gets the list of entries stored in this archive.
        /// </summary>
        public ReadOnlyCollection<XflArchiveEntry> Entries
        {
            get
            {
                return new ReadOnlyCollection<XflArchiveEntry>(_Entries.Values.ToList());
            }
        }

        /// <summary>
        /// Gets a certain entry by its path.
        /// </summary>
        /// <param name="path">Internal path where the entry is stored.</param>
        /// <returns>The entry if it has been found, otherwise throws an exception.</returns>
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

        /// <summary>
        /// Creates a new archive entry. Overwrites existing entries.
        /// </summary>
        /// <param name="path">Internal path to store the file at.</param>
        /// <param name="content">Contents of the new file.</param>
        /// <returns>The entry that was added.</returns>
        public XflArchiveEntry CreateEntry(string path, byte[] content = null) {
            var entry = new XflArchiveEntry(this);
            entry.Path = path;
            entry.Content = content ?? new byte[0];

            _Entries[path] = entry;

            return entry;
        }

        /// <summary>
        /// Adds the (1-dimensional) contents of an entire directory.
        /// </summary>
        /// <param name="path">Location of the directory.</param>
        /// <param name="searchPattern">Optional search pattern to filter files.</param>
        public void AddDirectory(string path, string searchPattern = "*")
        {
            foreach (var file in Directory.GetFiles(path, searchPattern))
            {
                string fileName = Path.GetFileName(file);
                CreateEntry(fileName, File.ReadAllBytes(file));
            }
        }

        /// <summary>
        /// Checks whether or not a path exists in the archive.
        /// </summary>
        /// <param name="path">Internal path to check.</param>
        /// <returns>Whether or not path exists.</returns>
        public bool HasEntry(string path)
        {
            return _Entries.ContainsKey(path);
        }


        /// <summary>
        /// Writes archive to a file.
        /// </summary>
        /// <param name="file">Location of the file.</param>
        public void Save(string file)
        {
            if (File.Exists(file)) File.Delete(file);
            using (var stream = File.OpenWrite(file))
            {
                Save(stream);
            }
        }

        /// <summary>
        /// Writes archive to a stream.
        /// </summary>
        /// <param name="output">Stream to write to.</param>
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


        /// <summary>
        /// Removes an entry from the archive.
        /// </summary>
        /// <param name="entry">The entry to be removed.</param>
        public void Remove(XflArchiveEntry entry)
        {
            // Make sure this entry is still stored in this archive
            if (entry.Archive == this)
            {
                // Remove
                _Entries.Remove(entry.Path);
                entry.Archive = null;
            }
        }


        #endregion

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
                    var entry = new XflArchiveEntry(this);

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

    }
}
