using RaiLTools.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaiLTools
{
    //abstract class CommandType
    //{
    // Text: [x51 x00 x00 x00] [int32 (Voice?)] [int32] [int32] [int32] [int16 (line number)] [int16] [x01 00 00 00]
    // ????: [xC8 00] [int32 ???] [int32 x 10]
    // Event: 1A 00 14 00 [int16 ev num] [int16]
    // Sprite: 1A 00 FF 00 [int32 (on/off? 0 = on, 1 = 0ff)] [int32 (pos?)] [int16 evbu number]
    // Cutin: 1A 00 1E 00 [int32] 0A 00 00 00 [int16 (evcut number)]
    // SFX: 3E 00 ... (e.g. 3E 00 36 0C 00 00 3F 00 00 00 00 00 00 00 00 00 00 00 00 00 )
    // Shake: 17 00 ... (e.g. 17 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 )
    // Fade?: C8 00 ... (e.g. C8 00 40 16 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 )
    // 
    //}


    public class GscFile
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

        public byte[] CommandSection;
        int[] StringLengths;
        public string[] Strings;

        byte[] EndSequence;

        private GscFile() { }
        private GscFile(Stream inputStream)
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
                if (StringLengths.Length > 0)
                {
                    StringLengths[StringLengths.Length - 1] = StringDefinitionLength;
                }

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

        public static GscFile FromStream(Stream inputStream)
        {
            return new GscFile(inputStream);
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

        public static GscFile FromFile(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                return GscFile.FromStream(stream);
            }
        }

        public static GscFile FromBytes(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return GscFile.FromStream(stream);
            }
        }
    }
}
