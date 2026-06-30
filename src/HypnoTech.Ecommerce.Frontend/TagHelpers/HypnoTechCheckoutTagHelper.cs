using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models.Settings;
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

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<script type=\"text/javascript\">");
            if (!string.IsNullOrEmpty(CheckoutElementId))
            {
                sb.AppendLine($"var model = new CheckoutViewModel({Serialize()});");
                sb.AppendLine($"ko.applyBindings(model, document.getElementById(\"{CheckoutElementId}\"));");
            }

            sb.AppendLine("</script>");
            output.TagName = null;
            output.PostContent.SetHtmlContent(sb.ToString());

            return base.ProcessAsync(context, output);
        }

        private string Serialize()
        {
            var countries = _countryRepository.GetCountries();
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
