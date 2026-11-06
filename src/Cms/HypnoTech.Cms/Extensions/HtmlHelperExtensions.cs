using Microsoft.AspNetCore.Mvc.Rendering;

namespace HypnoTech.Cms.Extensions
{
    public static class HtmlHelperExtensions
    {
        public static string OptionFor(this IHtmlHelper html, string value, string text, bool selected)
        {
            return $"<option value=\"{value}\" {(selected ? "selected" : string.Empty)}>{text}</option>";
        }
    }
}
