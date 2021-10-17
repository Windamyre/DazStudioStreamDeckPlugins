using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomShortcut.Support
{
    public static class Utilities
    {

        public static string BitmapToBase64(Bitmap bitmap,ImageFormat imageFormat)
        {
            string retValue = string.Empty;
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, imageFormat);
            ms.Position = 0;
            byte[] buffer = ms.ToArray();
            ms.Close();
            retValue = Convert.ToBase64String(buffer);
            buffer = null;
            return "data:image/png;base64," + retValue;
        }

        /// <summary>
        ///  Word wraps a single-line string based upon character count.  For strings exceeding the maxLineLimit it returns the first several lines, followed by the last line with ellipsis inserted.
        ///  For very long single words, it will truncate the word and add ellipsis.
        /// </summary>
        /// <param name="rawString">String: string to be formatted.</param>
        /// <param name="maxLineCount">Int: The maximum number of lines allowed.</param>
        /// <param name="maxLineLength">Int: the maximum number of characters per line.</param>
        /// <param name="insertEllipsis">Boolean: insert ellipsis in last line if maxLineCount </param>
        /// <returns>String: Formated with \n between lines.</returns>
        public static string WordWrapString(string rawString, int maxLineCount = 4, int maxLineLength = 9, bool insertEllipsis = true)
        {
            List<string> lines = new List<string>();
            StringBuilder newLine = new StringBuilder();
            char[] seperators = { ' ', '\f', '\t', '\n', '\r' };

            string[] words = rawString.Split(seperators, StringSplitOptions.RemoveEmptyEntries);

            foreach (string l in words)
            {
                if (newLine.Length != 0 && newLine.Length + l.Length > maxLineLength) //if the new word is too long for the line
                {
                    lines.Add(newLine.ToString().Trim());
                    newLine = new StringBuilder();
                }
                if (l.Length > (maxLineLength * 1.5))  //truncate really long single words
                {
                    newLine.Append(l.Substring(0, maxLineLength) + "…");
                }
                else
                {
                    newLine.Append(l + " ");
                }
            }

            lines.Add(newLine.ToString().Trim());  //slide in that last line

            if (lines.Count >= maxLineCount) //cut out the stuff in the middle if we're over maxLineCount
            {
                lines.RemoveRange(maxLineCount - 1, lines.Count - maxLineCount);
                if (insertEllipsis)
                {
                    lines[lines.Count - 1] = lines[lines.Count - 1].Insert(0, "…");
                }
                return string.Join("\n", lines.ToArray());
            }
            else
            {
                return string.Join("\n", lines.ToArray());
            }
        }
    }
}
