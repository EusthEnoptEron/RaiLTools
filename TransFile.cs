using NHyphenator;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace RaiLTools
{
    /// <summary>
    /// Provides a way to convert GSC files into translatable text files and to turn them back.
    /// Generally requires a "reference" gsc to exist, which stores stuff like sound effect, etc.
    /// </summary>
    public class TransFile
    {
        private List<string> _Strings = new List<string>();
        //private String ReferenceText = "人々の営みの息吹の中に、届く筈もないのに、";
        private String _ReferenceText = "more impressive and lustrous than this p";
        private Font _Font = new Font("ＭＳ ゴシック", 12f);
        private double _LineLength = 0;

        private TransFile()
        {
            _LineLength = MeasureString(_ReferenceText);
            /*double l2 = MeasureString(ReferenceText2);
            double l3 = MeasureString(ReferenceText3);
            double l4 = MeasureString(ReferenceText4);
            double l5 = MeasureString(ReferenceText5);
            */
        }

        #region Public API

        /// <summary>
        /// Loads a translation file from the file system.
        /// </summary>
        /// <param name="transFile">The file to be opened for reading.</param>
        /// <returns>The instance of the requested translation file.</returns>
        public static TransFile FromFile(string transFile)
        {
            using (var fileStream = File.OpenRead(transFile))
            {
                var trans = new TransFile();
                trans.PopulateFrom(fileStream);
                return trans;
            }
        }

        /// <summary>
        /// Converts a GSC file (internal representation) into a TransFile (easily translatable). 
        /// </summary>
        /// <param name="file">The file to be converted.</param>
        /// <returns>The TransFile representation of the GSC file.</returns>
        public static TransFile FromGSC(GscFile file)
        {
            var trans = new TransFile();
            trans.PopulateFrom(file);

            return trans;
        }

        /// <summary>
        /// Converts a GSC file (internal representation) into a TransFile (easily translatable). 
        /// </summary>
        /// <param name="file">The file to be converted.</param>
        /// <returns>The TransFile representation of the GSC file.</returns>
        public static TransFile FromGSC(string file)
        {
            return TransFile.FromGSC(GscFile.FromFile(file));
        }

        /// <summary>
        /// Turns this translation file into a GSC file that can be used in the game.
        /// </summary>
        /// <param name="refGsc">Path to the reference GSC.</param>
        /// <returns>The GSC representation of this translation file.</returns>
        public GscFile ToGSC(string refGsc)
        {
            var gsc = GscFile.FromFile(refGsc);
            Populate(gsc);

            return gsc;
        }



        /// <summary>
        /// Writes the textual representation to the file system.
        /// </summary>
        /// <param name="location">Where to save the file to.</param>
        public void Save(string location)
        {
            if (File.Exists(location)) File.Delete(location);

            using (var stream = File.OpenWrite(location))
            {
                Save(stream);
            }
        }

        /// <summary>
        /// Writes the textual representation to a stream.
        /// </summary>
        /// <param name="output">Stream to write to.</param>
        public void Save(Stream output)
        {
            using (var writer = new StreamWriter(output))
            {
                foreach (string str in _Strings)
                {
                    writer.WriteLine("#" + ConvertToExternal(str));
                    writer.WriteLine(">");
                }
            }
        }


        #endregion


        #region The heavy lifting

        /// <summary>
        /// Loads the strings defined by a GSC file into this translation file.
        /// </summary>
        /// <param name="file"></param>
        internal void PopulateFrom(GscFile file)
        {
            _Strings = file.Strings.ToList();
        }

        /// <summary>
        /// Parses a translation file and loads the strings into this instance.
        /// </summary>
        /// <param name="stream"></param>
        internal void PopulateFrom(Stream stream)
        {

            string ja = "";
            string en = "";
            _Strings.Clear();
            StringBuilder currentString = new StringBuilder();

            using (var reader = new StreamReader(stream))
            {
                var lines = reader.ReadToEnd().Split('\n');

                string text;
                bool isJapanese;
                int offset = 0;

                string jpText = "";
                while (ReadSection(lines, ref offset, out text, out isJapanese))
                {
                    if (isJapanese) jpText = text;
                    else
                    {
                        // English text
                        if (text.Trim() == "" && jpText.Trim() != "")
                            _Strings.Add(jpText);
                        else
                            AddWithWordWrapping(text);
                    }
                }
            }
        }

        /// <summary>
        /// Populates a GSC file with the strings stored in this translation file.
        /// </summary>
        /// <param name="file"></param>
        internal void Populate(GscFile file)
        {
            for (int i = 0; i < _Strings.Count && i < file.Strings.Length; i++)
            {
                file.Strings[i] = _Strings[i];
            }
        }

        /// <summary>
        /// Reads a section from a translation file (a section starts at ">" or "#" and ends with EOF or the next section)
        /// </summary>
        /// <param name="text">Lines of text.</param>
        /// <param name="offset">Current offset (to the lines).</param>
        /// <param name="result">Target for the section text to be stored.</param>
        /// <param name="japanese">Whether or not the section is Japanese (defined by the sign used to declare the section).</param>
        /// <returns>Whether or not there are more sections to read.</returns>
        private bool ReadSection(string[] text, ref int offset, out string result, out bool japanese)
        {
            bool reading = false;
            result = "";
            japanese = false;
            var output = new List<string>();
            bool eof = true;

            for (; offset < text.Length; offset++)
            {
                var line = text[offset];
                bool ja = line.StartsWith("#");
                bool en = line.StartsWith(">");
                bool isSectionStart = ja || en;

                if(isSectionStart) {
                    if(reading) {
                        eof = false;
                        break;
                    } else {
                        japanese = ja;
                        reading = true;

                        line = line.Substring(1);
                    }
                }

                if (reading)
                {
                    //en = Regex.Replace(line.Substring(1), @"""(.+?)""", "“$1”");

                    output.Add( ConvertToInternal(line) );
                }
            }
            result = string.Join("^n", output);

            return !eof;
        }

        #endregion

        #region Word Wrapping stuff (mostly old code)

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
                dynLines.Add(ReplaceUmlauts(string.Join("^n", WrapText(line, _LineLength, _Font.FontFamily.Name, _Font.Size))));
            }

            _Strings.Add(string.Join("^n", dynLines));
        }

        private string ConvertToExternal(string str)
        {
            str = str.Replace("　", "^t").Replace("^n", "\n").Replace("^", "\\");
            str = Regex.Replace(str, @"^\\t", " ", RegexOptions.Multiline);
            return str;
        }
        private string ConvertToInternal(string str)
        {
            str = str.Replace("\r", ""); // Don't like those.
            str = Regex.Replace(str, @"^ ", "\\t", RegexOptions.Multiline);
            return str.Replace("\\", "^").Replace("^t", "　");
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
                   new Typeface(_Font.FontFamily.Name), _Font.Size, System.Windows.Media.Brushes.Black);

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



        #endregion


    }
}
