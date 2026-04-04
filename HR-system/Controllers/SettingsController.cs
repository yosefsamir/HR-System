using HR_system.Data;
using HR_system.DTOs.Settings;
using HR_system.Models;
using HR_system.Services;
using HR_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR_system.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IWhatsAppSettingsService _whatsAppSettingsService;
        private readonly IWebHostEnvironment _environment;

        public SettingsController(
            ApplicationDbContext context,
            IWhatsAppService whatsAppService,
            IWhatsAppSettingsService whatsAppSettingsService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _whatsAppService = whatsAppService;
            _whatsAppSettingsService = whatsAppSettingsService;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "الإعدادات";
            var settings = await GetOrCreateSettingsAsync();
            ViewData["CompanyLogoExists"] = System.IO.File.Exists(GetCompanyLogoPath());
            return View(new SettingsDto
            {
                CompanyName = settings.CompanyName,
                SlipFontSize = settings.SlipFontSize,
                SlipWidthPercent = settings.SlipWidthPercent,
                SlipFooterMessage = settings.SlipFooterMessage
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var settings = await GetOrCreateSettingsAsync();
            return Json(new
            {
                companyName = settings.CompanyName,
                slipFontSize = settings.SlipFontSize,
                slipWidthPercent = settings.SlipWidthPercent,
                slipFooterMessage = settings.SlipFooterMessage
            });
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] SettingsDto dto)
        {
            try
            {
                var settings = await GetOrCreateSettingsAsync();
                
                settings.CompanyName = dto.CompanyName;
                settings.SlipFontSize = dto.SlipFontSize;
                settings.SlipWidthPercent = dto.SlipWidthPercent;
                settings.SlipFooterMessage = dto.SlipFooterMessage;
                settings.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "تم حفظ الإعدادات بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadCompanyLogo(IFormFile logo)
        {
            if (logo == null || logo.Length == 0)
            {
                return Json(new { success = false, message = "يرجى اختيار ملف الشعار" });
            }

            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
            var fileExtension = Path.GetExtension(logo.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return Json(new { success = false, message = "نوع الملف غير مدعوم" });
            }

            const long maxSize = 1 * 1024 * 1024;
            if (logo.Length > maxSize)
            {
                return Json(new { success = false, message = "حجم الشعار يجب أن لا يتجاوز 1 ميجا" });
            }

            var logoPath = GetCompanyLogoPath();
            Directory.CreateDirectory(Path.GetDirectoryName(logoPath)!);

            await using (var stream = System.IO.File.Create(logoPath))
            {
                await logo.CopyToAsync(stream);
            }

            return Json(new { success = true, message = "تم رفع الشعار بنجاح", logoPath = "/assets/company-logo.png" });
        }

        private string GetCompanyLogoPath()
        {
            return Path.Combine(_environment.WebRootPath, "assets", "company-logo.png");
        }


        [HttpGet]
        public IActionResult WhatsApp()
        {
            ViewData["Title"] = "إعدادات واتساب";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FullRelinkWhatsAppSession()
        {
            var closed = await _whatsAppService.LogoutSessionAsync();
            if (!closed)
            {
                return Json(new { success = false, message = "تعذر إغلاق الجلسة الحالية" });
            }

            var started = await _whatsAppService.StartSessionAsync();
            if (!started)
            {
                return Json(new { success = false, message = "تم الإغلاق لكن تعذر بدء جلسة جديدة" });
            }

            var status = await _whatsAppService.GetSessionStatusAsync();
            var qrCode = await _whatsAppService.GetQRCodeAsync();

            return Json(new
            {
                success = true,
                status,
                hasQr = !string.IsNullOrWhiteSpace(qrCode),
                message = !string.IsNullOrWhiteSpace(qrCode)
                    ? "تم إغلاق الجلسة القديمة وبدء جلسة جديدة. امسح QR الآن."
                    : "تم إغلاق الجلسة وبدء جلسة جديدة. انتظر لحظات ثم حدّث الصفحة لظهور QR."
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetWhatsAppConnection()
        {
            var sessionInfo = await _whatsAppService.GetSessionInfoAsync();
            var status = sessionInfo?.Status ?? "STOPPED";

            if (status == "FAILED")
            {
                var restarted = await _whatsAppService.RestartSessionAsync();
                if (restarted)
                {
                    sessionInfo = await _whatsAppService.GetSessionInfoAsync();
                    status = sessionInfo?.Status ?? "STOPPED";
                }
            }

            var isConnected = status == "WORKING" || status == "CONNECTED";
            var shouldLoadQr = status == "SCAN_QR_CODE" || status == "STARTING";
            var qrCode = shouldLoadQr ? await _whatsAppService.GetQRCodeAsync() : null;

            var message = isConnected
                ? "تم ربط واتساب بنجاح"
                : (status == "STOPPED"
                    ? "الجلسة متوقفة. اضغط زر إعادة الربط لتوليد QR جديد."
                    : !string.IsNullOrWhiteSpace(qrCode)
                        ? "يرجى مسح رمز QR لإكمال الربط"
                        : "الجلسة قيد التحضير ولم يتم توليد QR بعد. اضغط تحديث بعد ثوانٍ.");

            return Json(new
            {
                success = true,
                status,
                qrCode,
                session = sessionInfo,
                message
            });
        }

        [HttpPost]
        public async Task<IActionResult> SendTestPdfWhatsApp([FromBody] TestWhatsAppPdfDto dto)
        {
            try
            {
                var result = await _whatsAppSettingsService.SendTestPdfAsync(dto);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<AppSettings> GetOrCreateSettingsAsync()
        {
            var settings = await _context.AppSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new AppSettings();
                _context.AppSettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            return settings;
        }
    }
}
