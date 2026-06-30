using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Extensions.Hosting;

namespace HypnoTech.Common.StaticWebAssets
{
    public static class StaticFiles
    {
        public static List<string> StyleFiles = new List<string>();
        public static List<string> MinifiedStyleFiles = new List<string>();
        public static List<string> ScriptFiles = new List<string>();
        public static List<string> MinifiedScriptFiles = new List<string>();

        public static void RegisterStaticFiles(IHostEnvironment env)
        {
            var mappings = env.IsDevelopment() ? StaticWebAssetReader.GetLocalMappings() : StaticWebAssetReader.GetMappings();

            foreach (var mapping in mappings)
            {
                var package = mapping.BasePath.Replace("_content/", string.Empty);

                var jsFolder = Path.Combine(mapping.Path, "js");

                if (Directory.Exists(jsFolder))
                {
                    var jsFiles = Directory.EnumerateFiles(jsFolder, "*.js", SearchOption.AllDirectories);

                    foreach (var jsFile in jsFiles)
                    {
                        AddScript(package, jsFile.Replace(jsFolder, string.Empty).Replace("\\", "/").TrimStart('/'), GenerateHash(jsFile));
                    }
                }

                var cssFolder = Path.Combine(mapping.Path, "css");

                if (Directory.Exists(cssFolder))
                {
                    var cssFiles = Directory.EnumerateFiles(cssFolder, "*.css", SearchOption.AllDirectories);

                    foreach (var cssFile in cssFiles)
                    {
                        AddStyle(package, cssFile.Replace(cssFolder, string.Empty).Replace("\\", "/").TrimStart('/'), GenerateHash(cssFile));
                    }
                }
            }

        }

        private static void AddScript(string package, string script, string hash)
        {
            var path = $"/_content/{package}/js/{script}?_={hash}";
            if (script.EndsWith("min.js"))
            {
                MinifiedScriptFiles.Add(path);
            }
            else
            {
                ScriptFiles.Add(path);
            }
        }

        private static void AddStyle(string package, string script, string hash)
        {
            var path = $"/_content/{package}/css/{script}?_={hash}";
            if (script.EndsWith("min.css"))
            {
                MinifiedStyleFiles.Add(path);
            }
            else
            {
                StyleFiles.Add(path);
            }
        }

        private static string GenerateHash(string path)
        {
            try
            {
                var md5 = MD5.Create();
                var bytes = File.ReadAllBytes(path);

                return BitConverter.ToString(md5.ComputeHash(bytes)).Replace("-", string.Empty).ToLower();
            }
            catch
            {
                return DateTime.Now.Millisecond.ToString();
            }
        }
    }
}
