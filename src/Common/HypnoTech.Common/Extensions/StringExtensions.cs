using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace HypnoTech.Common.Extensions
{
    public static class StringExtensions
    {
        public static string GenerateSlug(this string phrase)
        {
            string str = phrase.RemoveDiacritics().ToLower();
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= 60 ? str.Length : 60).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   

            while (str.Contains("--"))
            {
                str = str.Replace("--", "-");
            }

            return str;
        }

        public static string RemoveDiacritics(this string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string UrlToFilePath(this string input)
        {
            return input.Replace("/", Path.DirectorySeparatorChar.ToString());
        }

        public static string FilePathToUrl(this string input)
        {
            return input.Replace(Path.DirectorySeparatorChar.ToString(), "/");
        }

        public static string LineBreaksToHtml(this string input)
        {
            input = input.Replace("\r\n", "<br />");
            input = input.Replace("\r", "<br />");
            input = input.Replace("\n", "<br />");
            return input;
        }
    }
}
