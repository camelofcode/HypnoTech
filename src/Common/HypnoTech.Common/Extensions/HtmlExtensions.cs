using System;
using System.Linq;
using HtmlAgilityPack;

namespace HypnoTech.Common.Extensions
{
    public static class HtmlExtensions
    {
        public static string BreakLongLinks(this string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);

            HtmlNodeCollection nodes = document.DocumentNode.SelectNodes("//a[@href]");
            if (nodes != null)
            {
                foreach (var link in nodes)
                {
                    var text = link.InnerText;
                    if (text.Length > 15 && (text.IndexOf(" ", StringComparison.Ordinal) == -1 || text.IndexOf(" ", StringComparison.Ordinal) > 15))
                    {
                        link.Attributes.Add("class", "break-word");
                    }
                }
            }
            return document.DocumentNode.OuterHtml;
        }

        public static string GetFirstImage(this string html, string baseUrl)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            var nodes = document.DocumentNode.SelectNodes("//img");
            if (nodes != null && nodes.Any())
            {
                var src = nodes.First().Attributes["src"].Value;
                if (src.StartsWith("http"))
                {
                    return src;
                }

                return baseUrl + src;
            }

            return baseUrl + "/img/logo.png";
        }

        public static string GetFirstParagraph(this string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            var nodes = document.DocumentNode.SelectNodes("//p");
            var result = "";
            foreach (var node in nodes)
            {
                result = node.InnerText.Replace("&nbsp;", " ");
                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }
            }

            return result;
        }

        public static string StripTags(this string html)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(html);
            return document.DocumentNode.InnerText;
        }
    }
}
