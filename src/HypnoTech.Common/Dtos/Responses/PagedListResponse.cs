using System.Collections.Generic;

namespace HypnoTech.Common.Dtos.Responses
{
    public class PagedListResponse<T> where T : class
    {
        public ICollection<T> Items { get; set; }

        public int TotalPages { get; set; }

        public int CurrentPage { get; set; }
    }
}
