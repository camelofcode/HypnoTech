using System.Text;
using System.Threading.Tasks;
using HypnoTech.Common.StaticWebAssets;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Hosting;

namespace HypnoTech.Frontend.TagHelpers
{
    [HtmlTargetElement("HypnoTech-Scripts")]
    public class HypnoTechScriptsTagHelper : TagHelper
    {
        private readonly IHostEnvironment _environment;

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public string SiteName { get; set; }

        public HypnoTechScriptsTagHelper(IHostEnvironment environment)
        {
            _environment = environment;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var list = _environment.IsDevelopment() ? StaticFiles.ScriptFiles : StaticFiles.MinifiedScriptFiles;

            StringBuilder sb = new StringBuilder();

            foreach (var file in list)
            {
                sb.AppendLine($"<script src=\"{file}\"></script>");
            }
            sb.AppendLine($"<input type=\"hidden\" id=\"site-name\" value=\"{SiteName}\" />");

            if (ViewContext.ViewData.ContainsKey("script"))
            {
                var script = (string)ViewContext.ViewData["script"];
                sb.AppendLine($"<input type=\"hidden\" value=\"{script}\" id=\"page-script\" />");
            }
            else
            {
                sb.AppendLine($"<input type=\"hidden\" value=\"\" id=\"page-script\" />");
            }

            output.TagName = null;
            output.TagMode = TagMode.StartTagAndEndTag;
            output.PostContent.SetHtmlContent(sb.ToString());

            return base.ProcessAsync(context, output);
        }

    }
}
