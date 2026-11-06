using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HypnoTech.Media.Frontend.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace HypnoTech.Media.Frontend.TagHelpers
{
    [HtmlTargetElement("hypnotech-player-config", TagStructure = TagStructure.WithoutEndTag)]
    public class HypnoTechPlayerTagHelper : TagHelper
    {
        private readonly IAudioTrackService _audioTrackService;

        public string PlayerElementId { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public HypnoTechPlayerTagHelper(IAudioTrackService audioTrackService)
        {
            _audioTrackService = audioTrackService;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<script type=\"text/javascript\">");
            sb.Append($"var playerModel = new PlayerViewModel({await SerializeAsync()});");
            sb.Append($"ko.applyBindings(playerModel, document.getElementById(\"{PlayerElementId}\"));");
            sb.Append("</script>");
            output.TagName = null;
            output.PostContent.SetHtmlContent(sb.ToString());

        }

        private async Task<string> SerializeAsync()
        {
            var tracks = await _audioTrackService.GetTracksForPlayerAsync(
                ViewContext.HttpContext.RequestAborted);
            return JsonSerializer.Serialize(tracks, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
        }
    }
}
