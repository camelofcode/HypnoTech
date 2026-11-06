namespace HypnoTech.Common.Dtos.Requests
{
    public class PagedRequest
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public PagedRequest()
        {
            Page = 1;
            PageSize = 25;
        }

        public int Skip => ItemCountStart - 1;

        public int ItemCountStart => PageSize * (Page - 1) + 1;

        public int ItemCountEnd => PageSize * Page;

        public static PagedRequest Single => new PagedRequest
        {
            Page = 1,
            PageSize = 1
        };
    }
}
