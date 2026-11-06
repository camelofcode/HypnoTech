using System.Collections.Generic;

namespace HypnoTech.Ecommerce.Models
{
    public class MerchItem
    {
        public virtual string Id { get; set; }

        public string ImagePath { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }

        public string SubCategory { get; set; }

        public string Price { get; set; }

        public string FormatId { get; set; }

        public string PostageProfile { get; set; }

        public List<MerchOption> Options { get; set; }

        public string DownloadCodeId { get; set; }

        public string Slug { get; set; }

        public string FullUrl { get; set; }

        public void SetFullUrl(string siteUrl)
        {
            FullUrl = $"{siteUrl}/Merch/{Slug}".ToLowerInvariant();
        }
    }
}
