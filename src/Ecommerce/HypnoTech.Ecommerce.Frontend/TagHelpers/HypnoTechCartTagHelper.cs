using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace HypnoTech.Ecommerce.Frontend.TagHelpers
{
    [HtmlTargetElement("hypnotech-cart-config", TagStructure = TagStructure.WithoutEndTag)]
    public class HypnoTechCartTagHelper : TagHelper
    {
        public string CartElementId { get; set; }

        public string CartCounterElementId { get; set; }

        public string CookieName { get; set; }

        public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"<input type=\"hidden\" id=\"cart-cookie-name\" value=\"{CookieName}\" />");

            sb.AppendLine("<script type=\"text/javascript\">");
            if (!string.IsNullOrEmpty(CartCounterElementId))
            {
                sb.AppendLine("var cartCounterModel = new CartCountViewModel();");
                sb.AppendLine($"ko.applyBindings(cartCounterModel, document.getElementById(\"{CartCounterElementId}\"));");
            }

            if (!string.IsNullOrEmpty(CartElementId))
            {
                sb.AppendLine("var cartModel = new CartViewModel();");
                sb.AppendLine($"ko.applyBindings(cartModel, document.getElementById(\"{CartElementId}\"));");
            }

            sb.AppendLine("</script>");
            output.TagName = null;
            output.PostContent.SetHtmlContent(sb.ToString());

            return base.ProcessAsync(context, output);
        }
    }
}
