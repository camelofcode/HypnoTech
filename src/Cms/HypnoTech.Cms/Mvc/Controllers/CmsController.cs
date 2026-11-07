using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HypnoTech.Cms.Contracts.Dtos.Request;
using HypnoTech.Cms.Forms;
using HypnoTech.Cms.Models;
using HypnoTech.Cms.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Vereyon.Web;

namespace HypnoTech.Cms.Mvc.Controllers
{
    [Route("cms/[controller]")]
    [CmsRoleAuthorize]
    public class CmsController<TClass> : Controller where TClass : class
    {
        private readonly ICmsService<TClass> _service;
        private readonly IFlashMessage _flashMessage;
        private readonly string _controller;

        public CmsController(ICmsService<TClass> service, IFlashMessage flashMessage)
        {
            _service = service;
            _flashMessage = flashMessage;
            _controller = FormClassMapping.GetMapping<TClass>().ControllerName;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CmsListRequest request)
        {
            var response = await _service.GetPagedAsync<TClass>(request, HttpContext.RequestAborted);
            var model = new CmsListViewModel<TClass>(response, HttpContext);
            ViewBag.Controller = _controller;
            return View("Cms/Index", model);
        }

        [Route("MoveUp/{id}")]
        [HttpGet]
        public async Task<IActionResult> MoveUp(string id)
        {
            ViewBag.Controller = _controller;
            var mapping = FormClassMapping.GetMapping<TClass>();
            if (!mapping.Ordered)
            {
                throw new NotSupportedException($"Ordering is not supported for {mapping.DisplayName}");
            }

            await _service.MoveUpAsync(id, HttpContext.RequestAborted);
            return RedirectToIndex();
        }

        [Route("MoveDown/{id}")]
        [HttpGet]
        public async Task<IActionResult> MoveDown(string id)
        {
            ViewBag.Controller = _controller;
            var mapping = FormClassMapping.GetMapping<TClass>();
            if (!mapping.Ordered)
            {
                throw new NotSupportedException($"Ordering is not supported for {mapping.DisplayName}");
            }

            await _service.MoveDownAsync(id, HttpContext.RequestAborted);
            return RedirectToIndex();
        }

        [Route("Add")]
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            ViewBag.Controller = _controller;
            var mapping = FormClassMapping.GetMapping<TClass>();
            if (!mapping.CanAdd)
            {
                throw new NotSupportedException($"Add is not supported for {mapping.DisplayName}");
            }

            var model = await _service.BuildAddFormAsync(HttpContext.RequestAborted);

            return View("Cms/Add", model);
        }

        [Route("Add")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(TClass model, List<IFormFile> files, List<string> addedFiles)
        {
            var validation = await _service.ValidateAsync(
                model,
                false,
                addedFiles,
                HttpContext.RequestAborted);
            if (!validation.IsValid)
            {
                return View("Cms/Add", validation.Form);
            }

            var mapping = FormClassMapping.GetMapping<TClass>();
            ViewBag.Controller = _controller;
            if (!mapping.CanAdd)
            {
                throw new NotSupportedException($"Add is not supported for {mapping.DisplayName}");
            }

            await _service.AddAsync(model, files, addedFiles, HttpContext.RequestAborted);

            _flashMessage.Confirmation($"Added '{mapping.GetTitle(model)}'");

            return RedirectToIndex();
        }

        [Route("Duplicate/{id}")]
        [HttpGet]
        public async Task<IActionResult> Duplicate(string id)
        {
            ViewBag.Controller = _controller;
            var mapping = FormClassMapping.GetMapping<TClass>();
            if (!mapping.CanDuplicate)
            {
                throw new NotSupportedException($"Duplicate is not supported for {mapping.DisplayName}");
            }

            var model = await _service.BuildDuplicateFormAsync(id, HttpContext.RequestAborted);

            return View("Cms/Duplicate", model);
        }

        [Route("Duplicate")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duplicate(TClass model, List<IFormFile> files, List<string> addedFiles)
        {
            var validation = await _service.ValidateAsync(
                model,
                false,
                addedFiles,
                HttpContext.RequestAborted);
            if (!validation.IsValid)
            {
                return View("Cms/Duplicate", validation.Form);
            }

            var mapping = FormClassMapping.GetMapping<TClass>();
            ViewBag.Controller = _controller;
            if (!mapping.CanDuplicate)
            {
                throw new NotSupportedException($"Duplicate is not supported for {mapping.DisplayName}");
            }

            await _service.DuplicateAsync(model, files, addedFiles, HttpContext.RequestAborted);

            _flashMessage.Confirmation($"Added '{mapping.GetTitle(model)}'");

            return RedirectToIndex();
        }

        [Route("Edit/{id}")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            ViewBag.Controller = _controller;
            var mapping = FormClassMapping.GetMapping<TClass>();
            if (!mapping.CanEdit)
            {
                throw new NotSupportedException($"Edit is not supported for {mapping.DisplayName}");
            }

            var model = await _service.BuildEditFormAsync(id, HttpContext.RequestAborted);
            return View("Cms/Edit", model);
        }

        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TClass model, List<IFormFile> files, List<string> addedFiles, List<string> removedFiles)
        {
            var validation = await _service.ValidateAsync(
                model,
                true,
                addedFiles,
                HttpContext.RequestAborted);
            if (!validation.IsValid)
            {
                return View("Cms/Edit", validation.Form);
            }

            var mapping = FormClassMapping.GetMapping<TClass>();
            if (!mapping.CanEdit)
            {
                throw new NotSupportedException($"Edit is not supported for {mapping.DisplayName}");
            }

            await _service.EditAsync(
                model,
                files,
                addedFiles,
                removedFiles,
                HttpContext.RequestAborted);

            _flashMessage.Confirmation($"Updated '{mapping.GetTitle(model)}'");

            return RedirectToIndex();
        }

        [Route("Remove/{id}")]
        [Route("Remove")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(string id)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            if (!mapping.CanRemove)
            {
                throw new NotSupportedException($"Remove is not supported for {mapping.DisplayName}");
            }

            TClass model = await _service.GetByIdAsync(id, HttpContext.RequestAborted);

            await _service.RemoveAsync(id, HttpContext.RequestAborted);

            _flashMessage.Confirmation($"Removed '{mapping.GetTitle(model)}'");

            return RedirectToIndex();
        }

        private RedirectResult RedirectToIndex()
        {
            return Redirect($"/Cms/{_controller}");
        }
    }
}
