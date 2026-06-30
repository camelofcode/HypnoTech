using System;
using System.Collections.Generic;
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
        public IActionResult Index(CmsListRequest request)
        {
            var model = new CmsListViewModel<TClass>(_service.GetPaged<TClass>(request), HttpContext);
            ViewBag.Controller = _controller;
            return View("Cms/Index", model);
        }

        [Route("MoveUp/{id}")]
        [HttpGet]
        public IActionResult MoveUp(string id)
        {
            ViewBag.Controller = _controller;
            var mapping = FormClassMapping.GetMapping<TClass>();
            if (!mapping.Ordered)
            {
                throw new NotSupportedException($"Ordering is not supported for {mapping.DisplayName}");
            }

            _service.MoveUp(id);
            return RedirectToIndex();
        }

        [Route("MoveDown/{id}")]
        [HttpGet]
        public IActionResult MoveDown(string id)
        {
            ViewBag.Controller = _controller;
            var mapping = FormClassMapping.GetMapping<TClass>();
            if (!mapping.Ordered)
            {
                throw new NotSupportedException($"Ordering is not supported for {mapping.DisplayName}");
            }

            _service.MoveDown(id);
            return RedirectToIndex();
        }

        [Route("Add")]
        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Controller = _controller;
            var mapping = FormClassMapping.GetMapping<TClass>();
            if (!mapping.CanAdd)
            {
                throw new NotSupportedException($"Add is not supported for {mapping.DisplayName}" );
            }

            var model = _service.BuildAddForm();

            return View("Cms/Add", model);
        }

        [Route("Add")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(TClass model, List<IFormFile> files, List<string> addedFiles)
        {
            if (!_service.Validate(model, false, addedFiles, out Form form))
            {
                return View("Cms/Add", form);
            }

            var mapping = FormClassMapping.GetMapping<TClass>();
            ViewBag.Controller = _controller;
            if (!mapping.CanAdd)
            {
                throw new NotSupportedException($"Add is not supported for {mapping.DisplayName}");
            }

            _service.Add(model, files, addedFiles);

            _flashMessage.Confirmation($"Added '{mapping.GetTitle(model)}'");

            return RedirectToIndex();
        }

        [Route("Duplicate/{id}")]
        [HttpGet]
        public IActionResult Duplicate(string id)
        {
            ViewBag.Controller = _controller;
            var mapping = FormClassMapping.GetMapping<TClass>();
            if (!mapping.CanDuplicate)
            {
                throw new NotSupportedException($"Duplicate is not supported for {mapping.DisplayName}");
            }

            var model = _service.BuildDuplicateForm(id);

            return View("Cms/Duplicate", model);
        }

        [Route("Duplicate")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Duplicate(TClass model, List<IFormFile> files, List<string> addedFiles)
        {
            if (!_service.Validate(model, false, addedFiles, out Form form))
            {
                return View("Cms/Duplicate", form);
            }

            var mapping = FormClassMapping.GetMapping<TClass>();
            ViewBag.Controller = _controller;
            if (!mapping.CanDuplicate)
            {
                throw new NotSupportedException($"Duplicate is not supported for {mapping.DisplayName}");
            }

            _service.Duplicate(model, files, addedFiles);

            _flashMessage.Confirmation($"Added '{mapping.GetTitle(model)}'");

            return RedirectToIndex();
        }

        [Route("Edit/{id}")]
        [HttpGet]
        public IActionResult Edit(string id)
        {
            ViewBag.Controller = _controller;
            var mapping = FormClassMapping.GetMapping<TClass>();
            if (!mapping.CanEdit)
            {
                throw new NotSupportedException($"Edit is not supported for {mapping.DisplayName}");
            }

            var model = _service.BuildEditForm(id);
            return View("Cms/Edit", model);
        }

        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TClass model, List<IFormFile> files, List<string> addedFiles, List<string> removedFiles)
        {
            if (!_service.Validate(model, true, addedFiles, out Form form))
            {
                return View("Cms/Edit", form);
            }

            var mapping = FormClassMapping.GetMapping<TClass>();
            if (!mapping.CanEdit)
            {
                throw new NotSupportedException($"Edit is not supported for {mapping.DisplayName}");
            }

            _service.Edit(model, files, addedFiles, removedFiles);

            _flashMessage.Confirmation($"Updated '{mapping.GetTitle(model)}'");

            return RedirectToIndex();
        }

        [Route("Remove/{id}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(string id)
        {
            var mapping = FormClassMapping.GetMapping<TClass>();
            if (!mapping.CanRemove)
            {
                throw new NotSupportedException($"Remove is not supported for {mapping.DisplayName}");
            }

            TClass model = _service.GetById(id);

            _service.Remove(id);

            _flashMessage.Confirmation($"Removed '{mapping.GetTitle(model)}'");

            return RedirectToIndex();
        }

        private RedirectResult RedirectToIndex()
        {
            return Redirect($"/Cms/{_controller}");
        }
    }
}
