using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using HypnoTech.Cms.Extensions;
using HypnoTech.Cms.Services.Interfaces;
using HypnoTech.Common.Dal.Repositories;
using Microsoft.Extensions.Configuration;

namespace HypnoTech.Cms.Services
{
    public class BackupService : IBackupService
    {
        private readonly IBackupRespository _backupRespository;
        private readonly string _basePath;

        public BackupService(IBackupRespository backupRespository, IConfiguration config)
        {
            _backupRespository = backupRespository;
            _basePath = config.UploadPath();
        }

        public dynamic GetAllData(List<string> includedTypes)
        {
            return _backupRespository.GetAllData(includedTypes);
        }

        public byte[] GetAllFiles(List<string> includedTypes)
        {
            using (var stream = new MemoryStream())
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                {
                    foreach (var mapping in FileUploadFolderCache.Mappings.Where(p => includedTypes.Contains(p.Folder)))
                    {
                        var folderPath = Path.Combine(_basePath, mapping.Folder);
                        if (!Directory.Exists(folderPath))
                        {
                            continue;
                        }

                        foreach (var filePath in Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories))
                        {
                            var relativePath = Path.GetRelativePath(folderPath, filePath);
                            var entryName = Path.Combine(mapping.Folder, relativePath).Replace('\\', '/');
                            archive.CreateEntryFromFile(filePath, entryName, CompressionLevel.Optimal);
                        }
                    }
                }

                return stream.ToArray();
            }
        }
    }
}
