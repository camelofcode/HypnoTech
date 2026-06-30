using System.Collections.Generic;

namespace HypnoTech.Ecommerce.Models
{
    public class AssignDownloadCodesResponse
    {
        public List<AssignedDownloadCode> AssignedDownloadCodes { get; set; }

        public List<string> ExpiredCollections { get; set; }
    }
}
