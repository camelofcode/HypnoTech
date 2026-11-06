using System.Collections.Generic;

namespace HypnoTech.Cms.Services.Interfaces
{
    public interface IBackupService
    {
        dynamic GetAllData(List<string> includedTypes);

        byte[] GetAllFiles(List<string> includedTypes);
    }
}
