using System.Collections.Generic;
using System.Linq;

namespace HypnoTech.Ecommerce.Models
{
    public class DownloadCodeCollection
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string RedemptionUrl { get; set; }

        public List<DownloadCode> Codes { get; set; }

        public int TotalCodes => Codes.Count;

        public int AssignedCodes => Codes.Count(p => p.Assigned);

        public int UnusedCodes => TotalCodes - AssignedCodes;
    }
}
