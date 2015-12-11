using NHyphenator;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RaiLTools
{
    public class TransFile
    {
        private List<string> Strings = new List<string>();
        //private String ReferenceText = "人々の営みの息吹の中に、届く筈もないのに、";
        private String ReferenceText = "more impressive and lustrous than this p";
        //private String ReferenceText2 = "　However, this story's start lies elsew";
        //private String ReferenceText3 = "of a beautifully strong, dark green―eas";
        //private String ReferenceText4 = "town's night, despite all the lights and";
        //private String ReferenceText5 = "　Black wings he saw, formed by layers of";
        //private Font Font = new Font("ＭＳ ゴシック", 12f);
        private Font Font = new Font("ＭＳ ゴシック", 12f);
        private double LineLength = 0;

        private TransFile()
        {
            LineLength = MeasureString(ReferenceText);
            /*double l2 = MeasureString(ReferenceText2);
            double l3 = MeasureString(ReferenceText3);
            double l4 = MeasureString(ReferenceText4);
            double l5 = MeasureString(ReferenceText5);
            */
        }

        public static TransFile FromFile(string transFile)
        {
            using (var fileStream = File.OpenRead(transFile))
            {
                var trans = new TransFile();
                trans.PopulateFrom(fileStream);
                return trans;
            }
        }

        public static TransFile FromGSC(GscFile file)
        {
            var trans = new TransFile();
            trans.PopulateFrom(file);

            return trans;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="refGsc">Path to the reference GSC.</param>
        /// <returns></returns>
        public GscFile ToGSC(string refGsc)
        {
            var gsc = GscFile.FromFile(refGsc);
            Populate(gsc);

            return gsc;
        }

        public void PopulateFrom(GscFile file)
        {
            Strings = file.Strings.ToList();
        }

        public void PopulateFrom(Stream stream)
        {

            string ja = "";
            string en = "";
            Strings.Clear();
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line.StartsWith("#"))
                    {
                        ja = line.Substring(1);
                    }
                    else if (line.StartsWith(">"))
                    {
                        en = Regex.Replace(line.Substring(1), @"""(.+?)""", "“$1”");
                        if (en.Length == 0 && ja.Length > 0)
                        {
                            Strings.Add(ConvertToInternal(ja));
                            ja = "";
                        }
                        else
                        {
                            AddWithWordWrapping(ConvertToInternal(en));
                            en = "";
                            ja = "";
                        }
                    }
                }
            }
        }

        private string ReplaceUmlauts(string str)
        {
            return str.Replace('Ä', '`').Replace('ä', '<').Replace('Ö', '>').Replace('ö', '\\').Replace('Ü', '{').Replace('ü', '}');
        }

        private void AddWithWordWrapping(String l)
        {
            string[] lines = l.Split(new string[] { "^n" }, StringSplitOptions.None);
            List<string> dynLines = new List<string>();
            foreach (string line in lines)
            {
                dynLines.Add(ReplaceUmlauts(string.Join("^n", WrapText(line, LineLength, Font.FontFamily.Name, Font.Size))));
            }

            Strings.Add(string.Join("^n", dynLines));
        }

        public void Populate(GscFile file)
        {
            for (int i = 0; i < Strings.Count && i < file.Strings.Length; i++)
            {
                file.Strings[i] = Strings[i];
            }
        }

        public void Save(string location)
        {
            if (File.Exists(location)) File.Delete(location);

            using (var stream = File.OpenWrite(location))
            {
                Save(stream);
            }
        }

        public void Save(Stream output)
        {
            using (var writer = new StreamWriter(output))
            {
                foreach (string str in Strings)
                {
                    writer.WriteLine("#" + ConvertToExternal(str));
                    writer.WriteLine(">");
                }
            }
        }

        private string ConvertToExternal(string str)
        {
            return str.Replace("　", "^r").Replace("^", "\\");
        }
        private string ConvertToInternal(string str)
        {
            return str.Replace("\\", "^").Replace("^r", "　");
        }

        private string GetVisibleString(string str)
        {
            return (Regex.Replace(str, @"(\^[a-z]|\|.+?\[.+?\])", "", RegexOptions.IgnoreCase));
        }

        double MeasureString(string str)
        {
            FormattedText formatted = new FormattedText(str,
                   CultureInfo.CurrentCulture,
                   System.Windows.FlowDirection.LeftToRight,
                   new Typeface(Font.FontFamily.Name), Font.Size, System.Windows.Media.Brushes.Black);

            return formatted.Width;
        }

        private List<string> WrapText(string text, double pixels, string fontFamily,
    float emSize)
        {
            Hyphenator hyphenator = new Hyphenator(HyphenatePatternsLanguage.EnglishUs, "-");

            string[] originalLines = text.Split(new string[] { " " },
                StringSplitOptions.None);

            List<string> wrappedLines = new List<string>();

            StringBuilder actualLine = new StringBuilder();
            double actualWidth = 0;

            foreach (var itm in originalLines)
            {
                var item = itm;
                actualWidth = MeasureString(GetVisibleString(actualLine.ToString() + item));

                if (actualWidth > pixels)
                {
                    if (!item.Contains("-"))
                    {
                        // Okay, we have something that's too long. Let's try to hyphenate it.
                        var result = hyphenator.HyphenateText(item);
                        string[] parts = result.Split(new string[] { "-" }, StringSplitOptions.None);

                        for (int i = parts.Length - 1; i > 0; i--)
                        {
                            var right = parts[i];
                            var left = "";
                            for (int j = 0; j < i; j++) left += parts[j];
                            left += "-";

                            if (MeasureString(GetVisibleString(actualLine.ToString() + left)) <= pixels)
                            {
                                wrappedLines.Add(actualLine.ToString() + left);
                                actualLine.Clear();

                                item = right;
                                break;
                            }
                        }

                    }

                    if (actualLine.Length > 0)
                        wrappedLines.Add(actualLine.ToString());

                    actualLine.Clear();
                    actualLine.Append(item + " ");
                    actualWidth = 0;
                }
                else
                {

                    actualLine.Append(item + " ");
                }
            }

            if (actualLine.Length > 0)
                wrappedLines.Add(actualLine.ToString());

            return wrappedLines;

        }


        public static TransFile FromGSC(string file)
        {
            return TransFile.FromGSC(GscFile.FromFile(file));
        }
    }
}
