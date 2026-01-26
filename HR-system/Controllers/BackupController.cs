using HR_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HR_system.Controllers
{
    public class BackupController : Controller
    {
        private readonly IBackupService _backupService;
        private readonly ILogger<BackupController> _logger;

        public BackupController(IBackupService backupService, ILogger<BackupController> logger)
        {
            _backupService = backupService;
            _logger = logger;
        }

        // GET: Backup
        public async Task<IActionResult> Index()
        {
            var backups = await _backupService.GetBackupsAsync();
            ViewBag.BackupFolder = _backupService.GetBackupFolder();
            return View(backups);
        }

        // POST: Backup/Create
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            try
            {
                var result = await _backupService.CreateBackupAsync();
                return Json(new
                {
                    success = result.Success,
                    message = result.Message,
                    path = result.FilePath
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating backup");
                return Json(new { success = false, message = $"خطأ: {ex.Message}" });
            }
        }

        // POST: Backup/Restore
        [HttpPost]
        public async Task<IActionResult> Restore([FromForm] string backupPath)
        {
            try
            {
                if (string.IsNullOrEmpty(backupPath))
                {
                    return Json(new { success = false, message = "مسار الملف مطلوب" });
                }

                var result = await _backupService.RestoreBackupAsync(backupPath);
                return Json(new
                {
                    success = result.Success,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring backup");
                return Json(new { success = false, message = $"خطأ: {ex.Message}" });
            }
        }

        // GET: Backup/Download
        public IActionResult Download(string path)
        {
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                return NotFound("الملف غير موجود");
            }

            // Security check: ensure file is in backup folder
            var backupFolder = _backupService.GetBackupFolder();
            if (!path.StartsWith(backupFolder, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            var fileName = Path.GetFileName(path);
            var contentType = path.EndsWith(".zip") ? "application/zip" : "application/octet-stream";

            return PhysicalFile(path, contentType, fileName);
        }

        // POST: Backup/Upload
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile backupFile)
        {
            if (backupFile == null || backupFile.Length == 0)
            {
                return Json(new { success = false, message = "لم يتم اختيار ملف" });
            }

            // Validate file extension
            var extension = Path.GetExtension(backupFile.FileName).ToLowerInvariant();
            if (extension != ".bak" && extension != ".zip")
            {
                return Json(new { success = false, message = "نوع الملف غير مدعوم. يجب أن يكون .bak أو .zip" });
            }

            try
            {
                var backupFolder = _backupService.GetBackupFolder();
                
                // Generate unique filename to avoid conflicts
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var fileName = $"Uploaded_{timestamp}_{backupFile.FileName}";
                var filePath = Path.Combine(backupFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await backupFile.CopyToAsync(stream);
                }

                _logger.LogInformation($"Backup file uploaded: {filePath}");

                return Json(new
                {
                    success = true,
                    message = "تم رفع الملف بنجاح",
                    path = filePath,
                    fileName = fileName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading backup file");
                return Json(new { success = false, message = $"خطأ: {ex.Message}" });
            }
        }

        // POST: Backup/Delete
        [HttpPost]
        public async Task<IActionResult> Delete([FromForm] string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    return Json(new { success = false, message = "مسار الملف مطلوب" });
                }

                // Security check: ensure file is in backup folder
                var backupFolder = _backupService.GetBackupFolder();
                if (!path.StartsWith(backupFolder, StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { success = false, message = "غير مسموح بحذف هذا الملف" });
                }

                var result = await _backupService.DeleteBackupAsync(path);

                return Json(new
                {
                    success = result,
                    message = result ? "تم حذف النسخة الاحتياطية بنجاح" : "فشل في حذف النسخة الاحتياطية"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting backup");
                return Json(new { success = false, message = $"خطأ: {ex.Message}" });
            }
        }
    }
}
