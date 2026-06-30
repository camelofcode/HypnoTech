using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zip;
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
                var zipFile = new ZipFile();

                foreach (var mapping in FileUploadFolderCache.Mappings.Where(p => includedTypes.Contains(p.Folder)))
                {
                    var folderPath = Path.Combine(_basePath, mapping.Folder);
                    if (Directory.Exists(folderPath))
                    {
                        zipFile.AddDirectory(folderPath, new DirectoryInfo(folderPath).Name);
                    }
                }

                zipFile.Save(stream);

                return stream.ToArray();
            }
        }
    }
}
