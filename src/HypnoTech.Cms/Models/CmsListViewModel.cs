using System.Linq;
using HypnoTech.Cms.Forms.Tables;
using Microsoft.AspNetCore.Http;
using HypnoTech.Common.Models;
using HypnoTech.Cms.Contracts.Dtos.Response;

namespace HypnoTech.Cms.Models
{
    public class CmsListViewModel
    {
        protected string Controller;

        public string Search { get; set; }

        public bool HasSearch => !string.IsNullOrEmpty(Search);

        public Table Listing { get; protected set; }

        public bool CanAdd { get; protected set; }

        public bool CanEdit { get; protected set; }

        public bool CanRemove { get; protected set; }    

        public bool CanDuplicate { get; protected set; }    

        public string DisplayName { get; set; }

        public string SingularDisplayName { get; set; }

        public string PluralDisplayName { get; set; }

        public string AddUrl => $"/Cms/{Controller}/Add";

        public string EditUrl => $"/Cms/{Controller}/Edit/{{0}}";

        public string DuplicateUrl => $"/Cms/{Controller}/Duplicate/{{0}}";

        public string RemoveUrl => $"/Cms/{Controller}/Remove/";

        public string MoveUpUrl => $"/Cms/{Controller}/MoveUp/{{0}}";

        public string MoveDownUrl => $"/Cms/{Controller}/MoveDown/{{0}}";

        public bool Ordered { get; set; }

        public PagerViewModel Pager { get; set; }
    }

    public class CmsListViewModel<TClass> : CmsListViewModel where TClass : class
    {
        public CmsListViewModel(CmsListReponse<TClass> response)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            DisplayName = mapping.DisplayName;
            SingularDisplayName = mapping.SingularDisplayName;
            PluralDisplayName = mapping.PluralDisplayName;
            DisplayName = mapping.DisplayName;
            Controller = mapping.ControllerName;
            CanAdd = mapping.CanAdd;
            CanEdit = mapping.CanEdit;
            CanRemove = mapping.CanRemove;
            CanDuplicate = mapping.CanDuplicate;
            Search = response.Search;
            Ordered = mapping.Ordered;
            Listing = mapping.BuildTable(response.Items.ToList(), response.SortColumn, response.Direction);
        }

        public CmsListViewModel(CmsListReponse<TClass> response, HttpContext context) : this(response)
        {
            Pager = new PagerViewModel(response.CurrentPage, response.TotalPages, context);
        }
    }
}
