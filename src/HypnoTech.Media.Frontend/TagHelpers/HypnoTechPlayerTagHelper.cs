using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HypnoTech.Media.Frontend.Services;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace HypnoTech.Media.Frontend.TagHelpers
{
    [HtmlTargetElement("hypnotech-player-config", TagStructure = TagStructure.WithoutEndTag)]
    public class HypnoTechPlayerTagHelper : TagHelper
    {
        private readonly IAudioTrackService _audioTrackService;

        public string PlayerElementId { get; set; }

        public HypnoTechPlayerTagHelper(IAudioTrackService audioTrackService)
        {
            _audioTrackService = audioTrackService;
        }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<script type=\"text/javascript\">");
            sb.Append($"var playerModel = new PlayerViewModel({Serialize()});");
            sb.Append($"ko.applyBindings(playerModel, document.getElementById(\"{PlayerElementId}\"));");
            sb.Append("</script>");
            output.TagName = null;
            output.PostContent.SetHtmlContent(sb.ToString());

            return base.ProcessAsync(context, output);
        }

        private string Serialize()
        {
            return JsonSerializer.Serialize(_audioTrackService.GetTracksForPlayer(), new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
        }
    }
}
