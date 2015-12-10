using RailTools.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailTools
{
    public class GSCFile
    {
        private Encoding JIS = Encoding.GetEncoding("shift_jis");

        public int FileLength;
        public int HeaderLength;
        public int CommandLength;
        public int StringDeclarationLength;
        public int StringDefinitionLength;
        public int Unknown1;
        public int Unknown2;
        public int Unknown3;
        public int Unknown4;

        byte[] CommandSection;
        int[] StringLengths;
        public string[] Strings;

        byte[] EndSequence;

        private GSCFile() { }
        private GSCFile(Stream inputStream)
        {
            using (var reader = new BigEndianBinaryReader(inputStream))
            {
                // -- HEADER
                FileLength = reader.ReadInt32();
                HeaderLength = reader.ReadInt32();
                CommandLength = reader.ReadInt32();
                StringDeclarationLength = reader.ReadInt32();
                StringDefinitionLength = reader.ReadInt32();
                Unknown1 = reader.ReadInt32();
                Unknown2 = reader.ReadInt32();
                Unknown3 = reader.ReadInt32();
                Unknown4 = reader.ReadInt32();

                // -- COMMANDSECTION
                CommandSection = reader.ReadBytes(CommandLength);

                // -- STRING DECLARATION
                reader.ReadBytes(8); // Skip first 8 bytes

                StringLengths = new int[StringDeclarationLength / 4 - 2 + 1];
                Strings = new string[StringLengths.Length];

                for (int i = 0; i < StringLengths.Length - 1; i++)
                {
                    StringLengths[i] = reader.ReadInt32();
                }
                // We need to determine the last one ourselves
                StringLengths[StringLengths.Length - 1] = StringDefinitionLength;

                reader.ReadByte(); // Skip 0 byte

                int toread, read = 1;

                for (int i = 0; i < Strings.Length; i++)
                {
                    toread = StringLengths[i];
                    byte[] bytes = reader.ReadBytes(toread - read - 1);

                    Strings[i] = JIS.GetString(bytes);
                    reader.ReadByte(); // Trailing 0


                    read = toread;
                }



                List<byte> endSequence = new List<byte>();
                byte[] buffer = new byte[0xFF];
                while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    byte[] readBytes = new byte[read];
                    Array.Copy(buffer, readBytes, read);
                    endSequence.AddRange(readBytes);
                }

                EndSequence = endSequence.ToArray();
            }
        }

        public static GSCFile FromStream(Stream inputStream)
        {
            return new GSCFile(inputStream);
        }

        public void Save(string path)
        {
            if (File.Exists(path)) File.Delete(path);
            using (var stream = File.OpenWrite(path))
            {
                Save(stream);
            }
        }
        public void Save(Stream outputStream)
        {
            using (var writer = new BigEndianBinaryWriter(outputStream))
            {
                // -- WRITE HEADER
                writer.Write(FileLength);
                writer.Write(HeaderLength);
                writer.Write(CommandLength);
                writer.Write(StringDeclarationLength);
                writer.Write(CalculatedStringDefinitionLength);
                writer.Write(Unknown1);
                writer.Write(Unknown2);
                writer.Write(Unknown3);
                writer.Write(Unknown4);

                // -- WRITE COMMAND SECTION
                writer.Write(CommandSection);

                // -- WRITE STRING LENGTHS
                writer.Write(0);
                writer.Write(1);

                int bytes = 1;
                for (int i = 0; i < Strings.Length - 1; i++)
                {
                    bytes += JIS.GetByteCount(Strings[i]) + 1;
                    writer.Write(bytes);
                }

                writer.Write((byte)0);

                foreach (String str in Strings)
                {
                    writer.Write(JIS.GetBytes(str));
                    writer.Write((byte)0);
                }

                writer.Write(EndSequence);

                long length = writer.BaseStream.Position;
                writer.Seek(0, SeekOrigin.Begin);
                writer.Write((int)length);
            }
        }

        private int CalculatedStringDefinitionLength
        {
            get
            {
                int l = 1;
                foreach (string str in Strings)
                {
                    l += JIS.GetByteCount(str) + 1;
                }

                return l;
            }
        }

        public static GSCFile FromFile(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return GSCFile.FromStream(stream);
            }
        }
    }
}
