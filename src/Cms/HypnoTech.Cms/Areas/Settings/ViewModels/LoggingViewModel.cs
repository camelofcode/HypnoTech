using System.Collections.Generic;
using System.Linq;
using HypnoTech.Common.Dtos.Responses;
using HypnoTech.Common.Models;
using HypnoTech.Common.Models.Settings;
using Microsoft.AspNetCore.Http;

namespace HypnoTech.Cms.Areas.Settings.ViewModels
{
    public class LoggingViewModel
    {
        public List<SerilogEntry> Items { get; set; }

        public PagerViewModel Pager { get; set; }

        public LoggingViewModel(PagedListResponse<SerilogEntry> logs, HttpContext context)
        {
            Items = logs.Items.ToList();
            Pager = new PagerViewModel(logs.CurrentPage, logs.TotalPages, context);
        }
    }
}
