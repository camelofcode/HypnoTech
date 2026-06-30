using HypnoTech.Frontend.Ajax;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Frontend.Controllers
{
    public abstract class SpaController : Controller
    {
        public ActionResult AjaxPartialView()
        {
            return AjaxPartialView(ControllerContext.ActionDescriptor.ActionName);
        }

        public ActionResult AjaxPartialView(string viewName)
        {
            return AjaxPartialView(null, viewName);
        }

        public ActionResult AjaxPartialView(object viewModel)
        {
            return AjaxPartialView(viewModel, ControllerContext.ActionDescriptor.ActionName);
        }

        public ActionResult AjaxPartialView(object viewModel, string viewName)
        {
            if (Request.Headers.ContainsKey("X-Partial-Request"))
            {
                return new AjaxPartialViewResult(this, viewModel, viewName);
            }

            return View(viewName, viewModel);
        }
    }
}
