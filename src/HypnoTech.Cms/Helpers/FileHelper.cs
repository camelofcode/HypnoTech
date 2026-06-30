using System.IO;
using System.Linq;

namespace HypnoTech.Cms.Helpers
{
    public static class FileHelper
    {
        public static string GetDeduplicatedPath(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return filePath;
            }

            var info = new FileInfo(filePath);

            //Fortunately this works cos good look deciphering it.
            //All files in directory
            var fileNumbers = Directory.EnumerateFiles(info.Directory?.FullName)
                //where the filename matches the uploaded file without extension
                .Where(p => p.ToLowerInvariant()
                    .StartsWith(Path.Combine(info.Directory?.FullName, info.Name.Replace(info.Extension, string.Empty)).ToLowerInvariant()))
                //get the part of the string after _ if it exists and can be parsed to int
                .Select(p => p
                    .Replace(info.Extension, string.Empty)
                    .Split("_")
                    .LastOrDefault())
                .Select(p =>
                {
                    var success = int.TryParse(p, out var value);
                    return new { value, success };
                }).Where(p => p.success)
                .Select(p => p.value)
                .ToList();

            //If there are no files like file_n, file should be file_1
            var index = 1;
            if (fileNumbers.Count > 0)
            {
                //if there are files like file_n, next file should be file_n+1
                index = fileNumbers.Max() + 1;
            }

            return $"{info.FullName.Replace(info.Extension, string.Empty)}_{index}{info.Extension}";
        }
    }
}
