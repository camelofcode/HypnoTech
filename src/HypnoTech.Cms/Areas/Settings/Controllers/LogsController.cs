using HypnoTech.Cms.Areas.Settings.ViewModels;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Dtos.Requests;
using HypnoTech.Common.Security;
using Microsoft.AspNetCore.Mvc;

namespace HypnoTech.Cms.Areas.Settings.Controllers
{
    [Area("Settings")]
    [RoleAuthorize("logs")]
    public class LogsController : Controller
    {
        private readonly ILoggingRepository _repository;

        public LogsController(ILoggingRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index(int page = 1)
        {
            return View(new LoggingViewModel(_repository.GetLogs(new PagedRequest
            {
                Page = page,
                PageSize = 25
            }), HttpContext));
        }

        public IActionResult Details(string id)
        {
            return View(_repository.GetLog(id));
        }
    }
}