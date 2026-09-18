using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace HypnoTech.Common.StaticWebAssets
{
    public static class StaticWebAssetReader
    {
        public static IEnumerable<StaticFileMapping> GetMappings()
        {
            var result = new List<StaticFileMapping>();

            var ass = Assembly.GetEntryAssembly();

            var contentFolder = Path.Combine(new FileInfo(ass.Location).Directory.FullName, "wwwroot", "_content");

            foreach (var subfolder in Directory.EnumerateDirectories(contentFolder).Select(p => new DirectoryInfo(p)))
            {
                var item = new StaticFileMapping
                {
                    Path = subfolder.FullName,
                    BasePath = $"_content/{subfolder.Name}"
                };

                if (item.BasePath == "_content/HypnoTech.Frontend")
                {
                    result.Insert(0, item);
                }
                else
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public static IEnumerable<StaticFileMapping> GetLocalMappings()
        {
            var result = new List<StaticFileMapping>();
            var assembly = Assembly.GetEntryAssembly();
            var outputDirectory = new FileInfo(assembly.Location).Directory.FullName;
            var manifestPath = Path.Combine(outputDirectory,
                $"{assembly.GetName().Name}.staticwebassets.runtime.json");

            using (var stream = File.OpenRead(manifestPath))
            using (var document = JsonDocument.Parse(stream))
            {
                var root = document.RootElement;
                var contentRoots = root.GetProperty("ContentRoots")
                    .EnumerateArray()
                    .Select(item => item.GetString())
                    .ToArray();
                var packages = root.GetProperty("Root")
                    .GetProperty("Children")
                    .GetProperty("_content")
                    .GetProperty("Children");

                foreach (var package in packages.EnumerateObject())
                {
                    if (!TryGetContentRootIndex(package.Value, out var contentRootIndex))
                    {
                        continue;
                    }

                    var item = new StaticFileMapping
                    {
                        Path = contentRoots[contentRootIndex],
                        BasePath = $"_content/{package.Name}"
                    };

                    if (item.BasePath == "_content/HypnoTech.Frontend")
                    {
                        result.Insert(0, item);
                    }
                    else
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }

        private static bool TryGetContentRootIndex(JsonElement node, out int contentRootIndex)
        {
            if (node.TryGetProperty("Patterns", out var patterns) &&
                patterns.ValueKind == JsonValueKind.Array)
            {
                foreach (var pattern in patterns.EnumerateArray())
                {
                    if (pattern.TryGetProperty("ContentRootIndex", out var index))
                    {
                        contentRootIndex = index.GetInt32();
                        return true;
                    }
                }
            }

            if (node.TryGetProperty("Asset", out var asset) &&
                asset.ValueKind == JsonValueKind.Object &&
                asset.TryGetProperty("ContentRootIndex", out var assetIndex))
            {
                contentRootIndex = assetIndex.GetInt32();
                return true;
            }

            if (node.TryGetProperty("Children", out var children) &&
                children.ValueKind == JsonValueKind.Object)
            {
                foreach (var child in children.EnumerateObject())
                {
                    if (TryGetContentRootIndex(child.Value, out contentRootIndex))
                    {
                        return true;
                    }
                }
            }

            contentRootIndex = -1;
            return false;
        }
    }
}
