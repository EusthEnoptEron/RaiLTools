using System;
using System.Drawing;
using System.IO;
using System.Linq;
using RaiLTools;

namespace railiar
{
    class Program
    {
        const int OLD_WIDTH = 800;
        const int OLD_HEIGHT = 600;

        const int NEW_WIDTH = 1280;
        const int NEW_HEIGHT = 960;

        static readonly byte[] OGG_MAGIC = {0x4F, 0x67, 0x67, 0x53};

        const int OGG_OFFSET = 66;
        //const int NEW_WIDTH = 800;
        //const int NEW_HEIGHT = 600;


        static void Main(string[] args)
        {
            if (args.Length == 0 || args.Contains("-h"))
            {
                Console.WriteLine($"railtools.exe file1 file2 file3...");
                Console.WriteLine($"");
                Console.WriteLine($"*.xfl => Extract archive into folder with same name");
                Console.WriteLine($"*.wcg => Convert to *.png");
                Console.WriteLine($"*.gsc => Convert to *.txt for translation");
                Console.WriteLine($"*.txt => Convert back to *.gsc");
                Console.WriteLine($"*.png => Convert to *.gsc");
                Console.WriteLine($"*.jpg => Convert to *.gsc");
                Console.WriteLine($"*.bmp => Convert to *.gsc");
                Console.WriteLine($"*.wav => Extract playable *.ogg if detected");
                Console.WriteLine($"Directory => Pack to *.xfl");

                return;
            }

            foreach (var path in args)
            {
                DealWithFile(Path.GetFullPath(path));
            }
        }

        static void DealWithFile(string path)
        {
            string parentDir = Path.GetDirectoryName(path);
            string target = Path.Combine(parentDir, Path.GetFileNameWithoutExtension(path));

            if (File.Exists(path))
            {
                if (path.EndsWith(".wcg"))
                {
                    // -> Convert to PNG
                    WCG2PNG(path, target + ".png");
                }
                else if (path.EndsWith(".xfl"))
                {
                    var archive = XflArchive.FromFile(path);
                    archive.ExtractToDirectory(target);

                    Console.WriteLine("Extracted {0} to {1}", Path.GetFileNameWithoutExtension(path), target);
                }
                else if (path.EndsWith(".gsc"))
                {
                    target += ".txt";

                    var scenario = TransFile.FromGSC(path);
                    scenario.Save(target);

                    Console.WriteLine("Extracted {0} to {1}", Path.GetFileNameWithoutExtension(path), target);
                }
                else if (path.EndsWith(".txt"))
                {
                    target += ".gsc";

                    var scenario = TransFile.FromFile(path);
                    var gsc = scenario.ToGSC(target);

                    gsc.Save(target);
                }
                else if (path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".bmp"))
                {
                    PNG2WCG(path, target + ".wcg");
                }
                else if (path.EndsWith(".wav"))
                {
                    WAV2OGG(path);
                }
                else
                {
                    Console.Error.WriteLine("Unsupported file format.");
                }
            }
            else if (Directory.Exists(path))
            {
                if (File.Exists(Path.Combine(path, ".meta.xml")))
                {
                    // -> Pack LWG
                    target += ".lwg";

                    var canvas = LwgCanvas.FromDirectory(path, true);
                    canvas.Save(target);
                }
                else
                {
                    // -> Pack XFL
                    target += ".xfl";

                    var archive = new XflArchive();
                    archive.AddDirectory(path);
                    archive.Save(target);
                }

                Console.WriteLine("Packed {0} to {1}", Path.GetFileName(path), target);
            }
            else
            {
                Console.Error.WriteLine("\"{0}\" is not a valid file resource!", path);
            }
        }

        static void PatchCanvas(string srcCanvas, string srcFolder, string dstCanvas)
        {
            Console.WriteLine("Patching {0}...", Path.GetFileName(srcCanvas));

            double scale = NEW_WIDTH / (double) OLD_WIDTH;

            var canvas = new LwgCanvas(srcCanvas);

            canvas.Width = (int) (canvas.Width * scale);
            canvas.Height = (int) (canvas.Height * scale);

            foreach (var file in Directory.GetFiles(srcFolder))
            {
                string path = Path.GetFileNameWithoutExtension(file);

                if (!file.EndsWith(".png")) continue;

                canvas.ReplaceImage(
                    path,
                    WcgImage.FromImage(file)
                );

                var item = canvas.GetEntry(path);

                if (item.Flag == 0) item.Flag = byte.MaxValue;

                item.X = (int) (item.X * scale);
                item.Y = (int) (item.Y * scale);
            }

            canvas.Save(dstCanvas);
        }


        static void WCG2PNG(string from, string to)
        {
            Console.WriteLine("Converting {0} to {1}...", Path.GetFileName(from), Path.GetFileName(to));
            using (var wcg = WcgImage.FromFile(from))
            using (var img = wcg.ToImage())
            {
                img.Save(to);
            }
        }

        static void PNG2WCG(string from, string to)
        {
            Console.WriteLine("Converting {0} to {1}...", Path.GetFileName(from), Path.GetFileName(to));

            using (var img = Image.FromFile(from))
            using (var wcg = WcgImage.FromImage(img))
            {
                wcg.Save(to);
            }
        }

        static void WAV2OGG(string path)
        {
            Console.WriteLine($"Processing {Path.GetFileName(path)}...");
            using (var streamIn = File.OpenRead(path))
            using (var reader = new BinaryReader(streamIn))
            {
                streamIn.Position = OGG_OFFSET;
                var bytes = reader.ReadBytes(4);

                if (bytes.SequenceEqual(OGG_MAGIC))
                {
                    Console.WriteLine($"Ogg Vorbis header found! Extracting...");

                    // Embedded Ogg Vorbis
                    streamIn.Position = OGG_OFFSET;

                    using (var streamOut = File.OpenWrite(Path.ChangeExtension(path, ".ogg")))
                    {
                        streamOut.SetLength(0);

                        while (TryCopyPage(streamIn, streamOut))
                        {
                        }
                    }
                }
            }
        }

        static bool TryCopyPage(Stream streamIn, Stream streamOut)
        {
            var startPos = streamIn.Position;

            // Skip to segment table
            if (streamIn.Seek(26, SeekOrigin.Current) == startPos)
            {
                return false;
            }

            var segmentCount = streamIn.ReadByte();

            if (segmentCount == -1)
            {
                return false;
            }

            var segmentLengths = new byte[segmentCount];
            streamIn.Read(segmentLengths, 0, segmentCount);

            var offset = segmentLengths.Sum(b => b);
            var length = (int) (streamIn.Position - startPos + offset);

            if (segmentCount == 1 && offset == 0)
            {
                // Skip this packet.
                streamIn.Seek(offset, SeekOrigin.Current);
            }
            else
            {
                // Copy to output
                streamIn.Position = startPos;
                streamIn.CopyStream(streamOut, length);
            }

            return true;
        }
    }
}