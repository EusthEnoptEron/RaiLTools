using RaiLTools;
using RaiLTools.RScript;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace railiar
{
    class Program
    {
        const int OLD_WIDTH = 800;
        const int OLD_HEIGHT = 600;

        const int NEW_WIDTH = 1280;
        const int NEW_HEIGHT = 960;

        //const int NEW_WIDTH = 800;
        //const int NEW_HEIGHT = 600;


        static void Main(string[] args)
        {
            //args = new string[] { @"D:\Visual Novels\Binaries\raiL\紅殻町博物誌\scr\1101.gsc" };
            //new CommandTranslator(GscFile.FromFile(args[0])).GetCommands().ToList();
            //foreach (var path in args)
            //{
            //    DealWithFile(Path.GetFullPath(path));
            //}
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

        static void Main2(string[] args)
        {
            string from = @"D:\Novels\raiL\信天翁航海録\grpo_translated";
            string to   = @"D:\Novels\raiL\信天翁航海録\grpo";

            //PNG2WCG(@"C:\Users\Simon\Pictures\Saya\CGSY31.png", @"D:\Novels\raiL\信天翁航海録\grpe\6105.wcg");

            //var canvas = LwgCanvas.FromFile(@"D:\Novels\raiL\信天翁航海録\grps-sources\excompane.lwg");
            //canvas.ExportToDirectory(@"D:\Novels\raiL\信天翁航海録\grps-translated\excompane", true);

            //var canvas2 = LwgCanvas.FromDirectory(@"D:\Novels\raiL\信天翁航海録\grps-translated\excompane", true);
            //canvas2.Save(@"D:\Novels\raiL\信天翁航海録\grps\excompane.lwg");



            //canvas.ExtractToDirectory("...");            
            //canvas.ImportDictionary("...");


           
            // EXTRACT GSCs
            //foreach (var file in Directory.GetFiles(@"D:\Novels\raiL\信天翁航海録\scr-sources"))
            //{
            //    string target = @"D:\Novels\raiL\信天翁航海録\scr-translated\" + Path.GetFileNameWithoutExtension(file) + ".txt";
            //    var transFile = TransFile.FromGSC(file);
            //    transFile.Save(target);
            //}

            // REPACK THEM
            //foreach (var file in Directory.GetFiles(@"D:\Novels\raiL\信天翁航海録\scr-translated"))
            //{
            //    var reference = @"D:\Novels\raiL\信天翁航海録\scr-sources\" + Path.GetFileNameWithoutExtension(file) + ".gsc";
            //    var target = @"D:\Novels\raiL\信天翁航海録\scr\" + Path.GetFileNameWithoutExtension(file) + ".gsc";

            //    var gscFile = TransFile.FromFile(file).ToGSC(reference);

            //    gscFile.Save(target);
            //}

           // PNG2WCG(
           //     @"D:\Novels\raiL\信天翁航海録\grpe~.xfl\0001.jpg",
           //     @"D:\Novels\raiL\信天翁航海録\grpe\0001.wcg"
           // );
           // PNG2WCG(
           //     @"D:\Novels\raiL\信天翁航海録\grpe~.xfl\0002.jpg",
           //     @"D:\Novels\raiL\信天翁航海録\grpe\0002.wcg"
           //);

            // Patch
            //PatchCanvas(
            //    @"D:\Novels\raiL\信天翁航海録\grps-sources\compane.lwg"
            //    , @"D:\Novels\raiL\信天翁航海録\grps-edited\compane.lwg"
            //    , @"D:\Novels\raiL\信天翁航海録\grps\compane.lwg"
            //);

            //PatchCanvas(
            //    @"D:\Novels\raiL\信天翁航海録\grps-sources\confscrn.lwg"
            //    , @"D:\Novels\raiL\信天翁航海録\grps-edited\confscrn.lwg"
            //    , @"D:\Novels\raiL\信天翁航海録\grps\confscrn.lwg"
            //);

            //PatchCanvas(
            //    @"D:\Novels\raiL\信天翁航海録\grps-sources\excompane.lwg"
            //    , @"D:\Novels\raiL\信天翁航海録\grps-edited\excompane.lwg"
            //    , @"D:\Novels\raiL\信天翁航海録\grps\excompane.lwg"
            //);

            //PatchCanvas(
            //  @"D:\Novels\raiL\信天翁航海録\grps-sources\fontcompane.lwg"
            //      , @"D:\Novels\raiL\信天翁航海録\grps-edited\fontcompane.lwg"
            //      , @"D:\Novels\raiL\信天翁航海録\grps\fontcompane.lwg"
            //);

            //PatchCanvas(
            //  @"D:\Novels\raiL\信天翁航海録\grps-sources\tbox01.lwg"
            //      , @"D:\Novels\raiL\信天翁航海録\grps-edited\tbox01.lwg"
            //      , @"D:\Novels\raiL\信天翁航海録\grps\tbox01.lwg"
            //);

            //PatchCanvas(
            //  @"D:\Novels\raiL\信天翁航海録\grps-sources\logbar_h.lwg"
            //      , @"D:\Novels\raiL\信天翁航海録\grps-edited\logbar_h.lwg"
            //      , @"D:\Novels\raiL\信天翁航海録\grps\logbar_h.lwg"
            //);

            //PatchCanvas(
            //  @"D:\Novels\raiL\信天翁航海録\grps-sources\logbar_v.lwg"
            //      , @"D:\Novels\raiL\信天翁航海録\grps-edited\logbar_v.lwg"
            //      , @"D:\Novels\raiL\信天翁航海録\grps\logbar_v.lwg"
            //);


            //Parallel.ForEach(Directory.GetFiles(@"D:\Novels\raiL\信天翁航海録\grpo_bu-resized", "*.png"), (file) =>
            //{
            //    var output = @"D:\Novels\raiL\信天翁航海録\grpo_bu\" + Path.GetFileNameWithoutExtension(file) + ".wcg";
            //    PNG2WCG(file, output);
            //});

            //foreach (var file in Directory.GetFiles(from, "*.png"))
            //{
            //    PNG2WCG(file, Path.Combine(to, Path.GetFileNameWithoutExtension(file) + ".wcg"));
            //} 




            //string originalFile = @"D:\Novels\raiL\信天翁航海録\grpo\0001_orig.wcg";
            //originalFile = @"D:\Novels\raiL\信天翁航海録\grpe\0014.wcg";
            //WCG2PNG(originalFile, "step1.png");
            //PNG2WCG("step1.png", "step2.wcg");
            //WCG2PNG("step2.wcg", "step3.png");

            //PNG2WCG("step3.png", @"D:\Novels\raiL\信天翁航海録\grpo\0001.wcg");

        }

        static void PatchCanvas(string srcCanvas, string srcFolder, string dstCanvas)
        {
            Console.WriteLine("Patching {0}...", Path.GetFileName(srcCanvas));

            double scale = NEW_WIDTH / (double)OLD_WIDTH;

            var canvas = new LwgCanvas(srcCanvas);

            canvas.Width = (int)(canvas.Width * scale);
            canvas.Height = (int)(canvas.Height * scale);

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

                item.X = (int)(item.X * scale);
                item.Y = (int)(item.Y * scale);
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
    }
}
