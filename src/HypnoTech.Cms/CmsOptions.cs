using System.Collections.Generic;
using HypnoTech.Cms.Mvc;

namespace HypnoTech.Cms
{
    public class CmsOptions
    {
        public string SiteName { get; set; }

        public string AdminUrl { get; set; }

        public List<AdminSettingsLink> SettingsPages { get; set; }
    }
}
