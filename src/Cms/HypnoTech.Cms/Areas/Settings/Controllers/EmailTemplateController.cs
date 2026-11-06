using HypnoTech.Cms.Areas.Settings.Validation;
using HypnoTech.Cms.Areas.Settings.ViewModels;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models;
using HypnoTech.Common.Security;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using System.Threading.Tasks;

namespace HypnoTech.Cms.Areas.Settings.Controllers
{
    [Area("Settings")]
    [RoleAuthorize("emailtemplates")]
    public class EmailTemplateController : Controller
    {
        private readonly IEmailTemplateAdminRepository _repository;
        private readonly IFlashMessage _flashMessage;

        public EmailTemplateController(IEmailTemplateAdminRepository repository, IFlashMessage flashMessage)
        {
            _repository = repository;
            _flashMessage = flashMessage;
        }

        public async Task<IActionResult> Index()
        {
            return View(new EmailTemplatesViewModel(
                await _repository.GetTemplatesAsync(HttpContext.RequestAborted)));
        }

        public async Task<IActionResult> Edit(string id)
        {
            return View(await _repository.GetTemplateForEditAsync(id, HttpContext.RequestAborted));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmailTemplate template)
        {
            var message = template.Validate();
            if (!string.IsNullOrEmpty(message))
            {
                _flashMessage.Danger(message);
                return RedirectToAction("Edit", new { id = template.Id });
            }

            await _repository.UpdateTemplateAsync(template, HttpContext.RequestAborted);
            _flashMessage.Confirmation($"Updated {template.EmailType}");

            return RedirectToAction("Index");
        }
    }
}
