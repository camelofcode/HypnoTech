using HypnoTech.Cms.Areas.Settings.Validation;
using HypnoTech.Cms.Areas.Settings.ViewModels;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Models;
using HypnoTech.Common.Security;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

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

        public IActionResult Index()
        {
            return View(new EmailTemplatesViewModel(_repository.GetTemplates()));
        }

        public IActionResult Edit(string id)
        {
            return View(_repository.GetTemplateForEdit(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EmailTemplate template)
        {
            var message = template.Validate();
            if (!string.IsNullOrEmpty(message))
            {
                _flashMessage.Danger(message);
                return RedirectToAction("Edit", new { id = template.Id });
            }

            _repository.UpdateTemplate(template);
            _flashMessage.Confirmation($"Updated {template.EmailType}");

            return RedirectToAction("Index");
        }
    }
}