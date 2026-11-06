using HypnoTech.Common.Dtos.Requests;
using HypnoTech.Common;

namespace HypnoTech.Cms.Contracts.Dtos.Request
{
    public class CmsListRequest : PagedRequest
    {
        public string Search { get; set; }

        public bool HasSearch => !string.IsNullOrEmpty(Search);

        public int? SortColumn { get; set; }

        public SortType Direction { get; set; }

        public bool IsDescending => Direction == SortType.Descending;

        public CmsListRequest()
        {
            Direction = SortType.Unsorted;
            PageSize = 10;
            Page = 1;
        }
    }
}
