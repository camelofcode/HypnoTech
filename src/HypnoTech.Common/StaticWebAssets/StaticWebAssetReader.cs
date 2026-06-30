using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

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

            try
            {
                var ass = Assembly.GetEntryAssembly();

                var xml = File.ReadAllText(Path.Combine(new FileInfo(ass.Location).Directory.FullName,
                    $"{ass.GetName().Name}.StaticWebAssets.xml"));

                XDocument doc = XDocument.Parse(xml);
                if (doc.Root != null)
                {
                    var elements = doc.Root.Descendants("ContentRoot");

                    foreach (var element in elements)
                    {
                        var item = new StaticFileMapping
                        {
                            Path = (string) element.Attribute("Path"),
                            BasePath = (string) element.Attribute("BasePath")
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
            }
            catch
            {
                return result;
            }

            return result;
        }
    }
}
