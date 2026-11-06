using System.Text;
using System.Threading.Tasks;
using HypnoTech.Frontend.Caching;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Hosting;

namespace HypnoTech.Frontend.TagHelpers
{
    [HtmlTargetElement("Site-Styles")]
    public class SiteStylesTagHelper : TagHelper
    {
        private readonly IHostEnvironment _environment;

        public SiteStylesTagHelper(IHostEnvironment environment)
        {
            _environment = environment;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var asset in SiteAssetManifest.GetStyles(_environment))
            {
                sb.AppendLine($"<link rel=\"stylesheet\" href=\"/css/{asset.Name}?_={asset.Hash}\" />");
            }

            output.TagName = null;
            output.TagMode = TagMode.StartTagAndEndTag;
            output.PostContent.SetHtmlContent(sb.ToString());

            return base.ProcessAsync(context, output);
        }
    }
}
