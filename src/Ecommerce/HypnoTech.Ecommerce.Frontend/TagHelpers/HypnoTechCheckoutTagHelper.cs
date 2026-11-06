using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace HypnoTech.Ecommerce.Frontend.TagHelpers
{
    [HtmlTargetElement("hypnotech-checkout-config", TagStructure = TagStructure.WithoutEndTag)]
    public class HypnoTechCheckoutTagHelper : TagHelper
    {
        private readonly ICountryRepository _countryRepository;

        public HypnoTechCheckoutTagHelper(ICountryRepository countryRepository)
        {
            _countryRepository = countryRepository;
        }

        public string CheckoutElementId { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<script type=\"text/javascript\">");
            if (!string.IsNullOrEmpty(CheckoutElementId))
            {
                sb.AppendLine($"var model = new CheckoutViewModel({await SerializeAsync()});");
                sb.AppendLine($"ko.applyBindings(model, document.getElementById(\"{CheckoutElementId}\"));");
            }

            sb.AppendLine("</script>");
            output.TagName = null;
            output.PostContent.SetHtmlContent(sb.ToString());
        }

        private async Task<string> SerializeAsync()
        {
            var countries = await _countryRepository.GetCountriesAsync(
                ViewContext.HttpContext.RequestAborted);
            countries.Insert(0, new Country
            {
                DisplayText = "Please select your country"
            });

            return JsonSerializer.Serialize(countries, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
        }
    }
}
