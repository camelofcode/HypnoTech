using System.Collections.Generic;

namespace HypnoTech.Common.Dal.Repositories
{
    public interface IBackupRespository
    {
        dynamic GetAllData(List<string> includedTypes);
    }
}
