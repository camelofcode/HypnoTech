using System.Text.RegularExpressions;
using HypnoTech.Common.Security;
using HypnoTech.Ecommerce.Areas.Settings.Models;
using HypnoTech.Ecommerce.Dal.Repositories;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;
using System.Threading.Tasks;

namespace HypnoTech.Ecommerce.Areas.Settings.Controllers
{
    [Area("Settings")]
    [RoleAuthorize("downloadcodes")]
    public class CodesController : Controller
    {
        private readonly IDownloadCodeRepository _repository;
        private readonly IFlashMessage _flashMessage;

        public CodesController(IDownloadCodeRepository repository, IFlashMessage flashMessage)
        {
            _repository = repository;
            _flashMessage = flashMessage;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DownloadCodesViewModel
            {
                CodeCollections = await _repository.GetAllCodeCollectionsAsync(
                    HttpContext.RequestAborted)
            };
            return View(model);
        }

        public IActionResult Add()
        {
            return View(new AddCodeCollectionViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddCodeCollectionViewModel model)
        {
            var message = model.Validate();
            if (!string.IsNullOrEmpty(message))
            {
                _flashMessage.Danger(message);
                return RedirectToAction("Add");
            }

            await _repository.AddCodeCollectionAsync(
                model.ToCodeCollection(),
                HttpContext.RequestAborted);
            _flashMessage.Confirmation($"Download Code Collection '{model.Name}' added");
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(string id)
        {
            return View(await _repository.GetDownloadCodeCollectionAsync(
                id,
                HttpContext.RequestAborted));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignCode(string emailAddress, string id)
        {
            Regex regex = new Regex(@"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*"")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\[(?:(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9]))\.){3}(?:(2(5[0-5]|[0-4][0-9])|1[0-9][0-9]|[1-9]?[0-9])|[a-z0-9-]*[a-z0-9]:(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21-\x5a\x53-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])+)\])");
            if (!regex.IsMatch(emailAddress))
            {
                _flashMessage.Danger("Email address is not valid");
                return RedirectToAction("Edit", new { id });
            }


            var code = await _repository.AssignDownloadCodeAsync(
                id,
                emailAddress,
                HttpContext.RequestAborted);
            _flashMessage.Confirmation($"Assigned code {code} to {emailAddress}");
            return RedirectToAction("Edit", new { id });
        }
    }
}
