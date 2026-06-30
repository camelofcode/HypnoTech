using System;
using System.Collections.Generic;
using System.Linq;
using HypnoTech.Ecommerce.Models;

namespace HypnoTech.Ecommerce.Areas.Settings.Models
{
    public class AddCodeCollectionViewModel
    {
        public string Name { get; set; }

        public string RedemptionUrl { get; set; }

        public string Codes { get; set; }

        public string Validate()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return "Name is required";
            }
            if (string.IsNullOrEmpty(RedemptionUrl))
            {
                return "Redemption Url is required";
            }
            if (string.IsNullOrEmpty(Codes))
            {
                return "Codes are required";
            }

            return null;
        }

        public DownloadCodeCollection ToCodeCollection()
        {
            var collection = new DownloadCodeCollection
            {
                Name = Name,
                RedemptionUrl = RedemptionUrl,
                Codes = new List<DownloadCode>()
            };
            collection.Codes = Codes.Split(Environment.NewLine).Select(p => new DownloadCode
            {
                Code = p.Trim()
            }).ToList();

            return collection;
        }
    }
}
