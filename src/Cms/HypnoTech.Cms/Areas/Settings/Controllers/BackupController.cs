using System;
using System.Collections.Generic;
using System.Text;
using HypnoTech.Cms.Services.Interfaces;
using HypnoTech.Common.Security;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace HypnoTech.Cms.Areas.Settings.Controllers
{
    [Area("Settings")]
    [RoleAuthorize("backups")]
    public class BackupController : Controller
    {
        private readonly IBackupService _backupService;

        public BackupController(IBackupService backupService)
        {
            _backupService = backupService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Data(List<string> includedTypes)
        {
            var backup = await _backupService.GetAllDataAsync(
                includedTypes,
                HttpContext.RequestAborted);
            var json = JsonConvert.SerializeObject(backup);
            var filename = $"Backup-{DateTime.Now:ddMMyyyy-hhmmss}-{string.Join('-', includedTypes)}.json";
            return File(Encoding.UTF8.GetBytes(json), "application/json", filename);
        }

        public async Task<IActionResult> Files(List<string> includedTypes)
        {
            var backup = await _backupService.GetAllFilesAsync(
                includedTypes,
                HttpContext.RequestAborted);
            var filename = $"Backup-{DateTime.Now:ddMMyyyy-hhmmss}-{string.Join('-', includedTypes)}.zip";
            return File(backup, "application/zip", filename);
        }
    }
}
