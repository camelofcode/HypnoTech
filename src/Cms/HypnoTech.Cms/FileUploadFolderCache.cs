using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HypnoTech.Cms
{
    public static class FileUploadFolderCache
    {
        public static List<ModelFolder> Mappings = new List<ModelFolder>();

        static FileUploadFolderCache()
        {
            Add("Rich Text Editor Uploads", Path.Combine("Images", "RichTextImages"));
        }

        public static void Add(string displayName, string folder)
        {
            var existing = Mappings.FirstOrDefault(p => p.ModelDisplayName == displayName);
            if (existing != null)
            {
                existing.Folder = folder;
                return;
            }

            Mappings.Add(new ModelFolder
            {
                ModelDisplayName = displayName,
                Folder = folder
            });
        }
    }

    public class ModelFolder
    {
        public string ModelDisplayName { get; set; }

        public string Folder { get; set; }
    }
}
