using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Formatting = Newtonsoft.Json.Formatting;

namespace HypnoTech.Frontend.Ajax
{
    public class AjaxPartialViewResultExecutor : IActionResultExecutor<AjaxPartialViewResult>
    {
        private static readonly string DefaultContentType = new MediaTypeHeaderValue("application/json")
        {
            Encoding = Encoding.UTF8
        }.ToString();

        private readonly IHttpResponseStreamWriterFactory _writerFactory;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;


        public AjaxPartialViewResultExecutor(IHttpResponseStreamWriterFactory writerFactory, ILogger<AjaxPartialViewResultExecutor> logger, IConfiguration config)
        {
            _writerFactory = writerFactory ?? throw new ArgumentNullException(nameof(writerFactory));
            _config = config;
            _logger = logger;
        }

        public async Task ExecuteAsync(ActionContext context, AjaxPartialViewResult result)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var response = context.HttpContext.Response;

            response.ContentType = DefaultContentType;

            if (result.StatusCode != null)
            {
                response.StatusCode = result.StatusCode.Value;
            }

            var canonicalUrl = $"{_config["CanonicalUrl"].TrimEnd('/')}{new Uri(context.HttpContext.Request.GetDisplayUrl()).PathAndQuery}".ToLowerInvariant();
            var anonyResult = new { result.Html, result.Title, result.Script, result.Description, canonicalUrl };

            _logger.AjaxPartialViewResultExecuting(anonyResult);
            using (var writer = _writerFactory.CreateWriter(response.Body, Encoding.UTF8))
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    jsonWriter.CloseOutput = false;
                    jsonWriter.AutoCompleteOnClose = false;

                    var jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
                    {
                        Formatting = Formatting.None,
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        NullValueHandling = NullValueHandling.Include
                    });
                    jsonSerializer.Serialize(jsonWriter, anonyResult);
                }

                // Perf: call FlushAsync to call WriteAsync on the stream with any content left in the TextWriter's
                // buffers. This is better than just letting dispose handle it (which would result in a synchronous write).
                await writer.FlushAsync();
            }
        }
    }
}
