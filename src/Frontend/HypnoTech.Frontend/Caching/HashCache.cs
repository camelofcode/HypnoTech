using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace HypnoTech.Frontend.Caching
{
    public static class HashCache
    {
        private static readonly Dictionary<string, string> Cache = new Dictionary<string, string>();

        public static string GetHash(string file)
        {
            if (Cache.ContainsKey(file))
            {
                return Cache[file];
            }

            var md5 = MD5.Create();
            var hash = BitConverter.ToString(md5.ComputeHash(File.ReadAllBytes(file))).Replace("-", string.Empty).ToLower();
            
            Cache.Add(file, hash);

            return hash;
        }
    }
}
