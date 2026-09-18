using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace Vereyon.Web
{
    public interface IFlashMessage
    {
        void Confirmation(string message);
        void Confirmation(string title, string message);
        void Danger(string message);
        void Danger(string title, string message);
    }

    public static class FlashMessageServiceCollectionExtensions
    {
        public static IServiceCollection AddFlashMessage(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddTransient<IFlashMessage, FlashMessage>();
            return services;
        }
    }

    internal sealed class FlashMessage : IFlashMessage
    {
        internal const string TempDataKey = "HypnoTech.FlashMessages";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITempDataDictionaryFactory _tempDataFactory;

        public FlashMessage(
            IHttpContextAccessor httpContextAccessor,
            ITempDataDictionaryFactory tempDataFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _tempDataFactory = tempDataFactory;
        }

        public void Confirmation(string message) => Add("success", null, message);

        public void Confirmation(string title, string message) => Add("success", title, message);

        public void Danger(string message) => Add("danger", null, message);

        public void Danger(string title, string message) => Add("danger", title, message);

        private void Add(string type, string title, string message)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return;
            }

            var tempData = _tempDataFactory.GetTempData(httpContext);
            var messages = tempData.TryGetValue(TempDataKey, out var existing)
                ? JsonSerializer.Deserialize<List<FlashMessageModel>>(existing?.ToString() ?? string.Empty)
                : null;

            messages ??= new List<FlashMessageModel>();
            messages.Add(new FlashMessageModel(type, title, message));
            tempData[TempDataKey] = JsonSerializer.Serialize(messages);
        }
    }

    [HtmlTargetElement("flash")]
    public sealed class FlashTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITempDataDictionaryFactory _tempDataFactory;

        public FlashTagHelper(
            IHttpContextAccessor httpContextAccessor,
            ITempDataDictionaryFactory tempDataFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _tempDataFactory = tempDataFactory;
        }

        public bool Dismissable { get; set; } = true;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return;
            }

            var tempData = _tempDataFactory.GetTempData(httpContext);
            if (!tempData.TryGetValue(FlashMessage.TempDataKey, out var storedMessages))
            {
                return;
            }

            var messages = JsonSerializer.Deserialize<List<FlashMessageModel>>(
                storedMessages?.ToString() ?? string.Empty);
            if (messages == null)
            {
                return;
            }

            var html = new System.Text.StringBuilder();
            foreach (var message in messages)
            {
                var dismissableClass = Dismissable ? " alert-dismissible fade show" : string.Empty;
                html.Append("<div class=\"alert alert-")
                    .Append(HtmlEncoder.Default.Encode(message.Type))
                    .Append(dismissableClass)
                    .Append("\" role=\"alert\">");

                if (!string.IsNullOrWhiteSpace(message.Title))
                {
                    html.Append("<strong>")
                        .Append(HtmlEncoder.Default.Encode(message.Title))
                        .Append("</strong> ");
                }

                html.Append(HtmlEncoder.Default.Encode(message.Message));

                if (Dismissable)
                {
                    html.Append("<button type=\"button\" class=\"close\" data-dismiss=\"alert\" aria-label=\"Close\">")
                        .Append("<span aria-hidden=\"true\">&times;</span></button>");
                }

                html.Append("</div>");
            }

            output.Content.SetHtmlContent(html.ToString());
        }
    }

    internal sealed record FlashMessageModel(string Type, string Title, string Message);
}
