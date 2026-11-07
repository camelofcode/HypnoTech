using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using HypnoTech.Cms.Extensions;
using HypnoTech.Cms.Services.Interfaces;
using HypnoTech.Common.Dal.Repositories;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;

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

        public Task<dynamic> GetAllDataAsync(List<string> includedTypes, CancellationToken cancellationToken = default)
        {
            return _backupRespository.GetAllDataAsync(includedTypes, cancellationToken);
        }

        public async Task<byte[]> GetAllFilesAsync(
            List<string> includedTypes,
            CancellationToken cancellationToken = default)
        {
            await using var stream = new MemoryStream();
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
                        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                        await using var source = new FileStream(
                            filePath,
                            FileMode.Open,
                            FileAccess.Read,
                            FileShare.Read,
                            81920,
                            useAsync: true);
                        await using var destination = entry.Open();
                        await source.CopyToAsync(destination, cancellationToken);
                    }
                }
            }

            return stream.ToArray();
        }
    }
}
