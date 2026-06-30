using System.Text;
using System.Threading.Tasks;
using HypnoTech.Common.StaticWebAssets;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Hosting;

namespace HypnoTech.Frontend.TagHelpers
{
    [HtmlTargetElement("HypnoTech-Styles")]
    public class HypnoTechStylesTagHelper : TagHelper
    {
        private readonly IHostEnvironment _environment;

        public HypnoTechStylesTagHelper(IHostEnvironment environment)
        {
            _environment = environment;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var list = _environment.IsDevelopment() ? StaticFiles.StyleFiles : StaticFiles.MinifiedStyleFiles;

            StringBuilder sb = new StringBuilder();
            foreach (var file in list)
            {
                sb.AppendLine($"<link rel=\"stylesheet\" href=\"{file}\" />");
            }

            output.TagName = null;
            output.TagMode = TagMode.StartTagAndEndTag;
            output.PostContent.SetHtmlContent(sb.ToString());

            return base.ProcessAsync(context, output);
        }

    }
}
