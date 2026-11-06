using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Hosting;

namespace HypnoTech.Frontend.Caching
{
    internal static class SiteAssetManifest
    {
        private static readonly ConcurrentDictionary<string, SiteAssets> Manifests =
            new ConcurrentDictionary<string, SiteAssets>(StringComparer.Ordinal);

        public static void Register(IHostEnvironment environment)
        {
            Manifests[GetKey(environment)] = Build(environment);
        }

        public static IReadOnlyList<SiteAsset> GetScripts(IHostEnvironment environment) =>
            Get(environment).Scripts;

        public static IReadOnlyList<SiteAsset> GetStyles(IHostEnvironment environment) =>
            Get(environment).Styles;

        private static SiteAssets Get(IHostEnvironment environment) =>
            Manifests.GetOrAdd(GetKey(environment), _ => Build(environment));

        private static string GetKey(IHostEnvironment environment) =>
            $"{Path.GetFullPath(environment.ContentRootPath)}|{environment.EnvironmentName}";

        private static SiteAssets Build(IHostEnvironment environment)
        {
            var webRoot = Path.Combine(environment.ContentRootPath, "wwwroot");
            return new SiteAssets(
                GetAssets(Path.Combine(webRoot, "js"), "*.js", environment.IsDevelopment()),
                GetAssets(Path.Combine(webRoot, "css"), "*.css", environment.IsDevelopment()));
        }

        private static IReadOnlyList<SiteAsset> GetAssets(
            string folder,
            string pattern,
            bool development)
        {
            if (!Directory.Exists(folder))
            {
                return Array.Empty<SiteAsset>();
            }

            return Directory.EnumerateFiles(folder, pattern)
                .Where(path => development
                    ? !path.EndsWith(".min" + Path.GetExtension(path), StringComparison.OrdinalIgnoreCase)
                    : path.EndsWith(".min" + Path.GetExtension(path), StringComparison.OrdinalIgnoreCase))
                .Select(path => new SiteAsset(Path.GetFileName(path), HashCache.GetHash(path)))
                .ToList();
        }

        private sealed record SiteAssets(
            IReadOnlyList<SiteAsset> Scripts,
            IReadOnlyList<SiteAsset> Styles);
    }

    internal sealed record SiteAsset(string Name, string Hash);
}
