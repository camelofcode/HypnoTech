using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;

namespace HypnoTech.Frontend.Caching
{
    public static class HashCache
    {
        private static readonly ConcurrentDictionary<string, string> Cache =
            new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        public static string GetHash(string file)
        {
            var path = Path.GetFullPath(file);
            return Cache.GetOrAdd(path, static value =>
                Convert.ToHexString(MD5.HashData(File.ReadAllBytes(value))).ToLowerInvariant());
        }
    }
}
