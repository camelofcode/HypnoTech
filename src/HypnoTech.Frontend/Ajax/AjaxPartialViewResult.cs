using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace HypnoTech.Frontend.Ajax
{
    public class AjaxPartialViewResult : ActionResult, IStatusCodeActionResult
    {
        private readonly string _viewName;
        private readonly Controller _controller;
        public int? StatusCode => _viewName.Equals("NotFound") || _controller.Response.StatusCode == 404 ? 404 : 200;

        public string ContentType { get; set; }

        public string Html { get; private set; }

        public string Title { get; private set; }

        public string Script { get; private set; }

        public string CanonicalUrl { get; private set; }

        public string Description { get; private set; }

        public AjaxPartialViewResult(Controller controller, string viewName)
        {
            _viewName = viewName;
            _controller = controller;
            _controller.ViewBag.IsPartial = true;
        }

        public AjaxPartialViewResult(Controller controller, object viewModel, string viewName)
        {
            _viewName = viewName;
            _controller = controller;
            if (viewModel != null)
            {
                _controller.ViewData.Model = viewModel;
            }
            _controller.ViewBag.IsPartial = true;
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            IViewEngine viewEngine = context.HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
            IActionResultExecutor<AjaxPartialViewResult> executor = context.HttpContext.RequestServices.GetRequiredService<IActionResultExecutor<AjaxPartialViewResult>>();

            if (viewEngine == null)
            {
                throw new Exception("View Engine Not Found");
            }

            using (StringWriter writer = new StringWriter())
            {
                ViewEngineResult viewResult = _viewName.EndsWith(".cshtml")
                    ? viewEngine.GetView(_viewName, _viewName, false)
                    : viewEngine.FindView(_controller.ControllerContext, _viewName, false);

                ViewContext viewContext = new ViewContext(
                    _controller.ControllerContext,
                    viewResult.View,
                    _controller.ViewData,
                    _controller.TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                viewResult.View.RenderAsync(viewContext).Wait();

                Title = viewContext.ViewData["Title"]?.ToString();
                Script = viewContext.ViewData["Script"]?.ToString();
                Description = viewContext.ViewData["Description"]?.ToString();

                Html = writer.GetStringBuilder().ToString();
                Html = Html.Replace("\r", "").Replace("\n", "").Replace("\t", "");

                context.HttpContext.Response.Headers[HeaderNames.CacheControl] = "no-store, no-cache, must-revalidate, post-check=0, precheck=0";
                context.HttpContext.Response.Headers[HeaderNames.Pragma] = "no-cache";
                context.HttpContext.Response.Headers[HeaderNames.Expires] = "Thu, 1 Jan 1970 00:00:00 GMT";

                return executor.ExecuteAsync(context, this);
            }
        }

    }
}
