using System.IO;
using System.Linq;
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

            var files = Directory.EnumerateFiles(Path.Combine("wwwroot", "css"), "*.css");

            files = _environment.IsDevelopment() ? files.Where(p => !p.ToLowerInvariant().EndsWith(".min.css")) : files.Where(p => p.ToLowerInvariant().EndsWith(".min.css"));

            foreach (var file in files)
            {
                var hash = HashCache.GetHash(file);
                var fileInfo = new FileInfo(file);
                sb.AppendLine($"<link rel=\"stylesheet\" href=\"/css/{fileInfo.Name}?_={hash}\" />");
            }

            output.TagName = null;
            output.TagMode = TagMode.StartTagAndEndTag;
            output.PostContent.SetHtmlContent(sb.ToString());

            return base.ProcessAsync(context, output);
        }
    }
}
