using HypnoTech.Common.Dtos.Responses;
using HypnoTech.Common;

namespace HypnoTech.Cms.Contracts.Dtos.Response
{
    public class CmsListReponse<TClass> : PagedListResponse<TClass> where TClass : class
    {
        public string Search { get; set; }

        public bool HasSearch => !string.IsNullOrEmpty(Search);

        public int SortColumn { get; set; }

        public SortType Direction { get; set; }
    }
}
