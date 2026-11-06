using HypnoTech.Cms.Areas.Settings.ViewModels;
using HypnoTech.Common.Dal.Repositories;
using HypnoTech.Common.Dtos.Requests;
using HypnoTech.Common.Security;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Index(int page = 1)
        {
            var logs = await _repository.GetLogsAsync(new PagedRequest
            {
                Page = page,
                PageSize = 25
            }, HttpContext.RequestAborted);
            return View(new LoggingViewModel(logs, HttpContext));
        }

        public async Task<IActionResult> Details(string id)
        {
            return View(await _repository.GetLogAsync(id, HttpContext.RequestAborted));
        }
    }
}
