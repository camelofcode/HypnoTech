using System;
using System.Collections.Generic;
using System.Text;
using HypnoTech.Cms.Services.Interfaces;
using HypnoTech.Common.Security;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

        public IActionResult Data(List<string> includedTypes)
        {
            var backup = _backupService.GetAllData(includedTypes);
            var json = JsonConvert.SerializeObject(backup);
            var filename = $"Backup-{DateTime.Now:ddMMyyyy-hhmmss}-{string.Join('-', includedTypes)}.json";
            return File(Encoding.UTF8.GetBytes(json), "application/json", filename);
        }

        public IActionResult Files(List<string> includedTypes)
        {
            var backup = _backupService.GetAllFiles(includedTypes);
            var filename = $"Backup-{DateTime.Now:ddMMyyyy-hhmmss}-{string.Join('-', includedTypes)}.zip";
            return File(backup, "application/zip", filename);
        }
    }
}
