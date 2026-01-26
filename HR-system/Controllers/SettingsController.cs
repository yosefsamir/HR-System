using HR_system.Data;
using HR_system.DTOs.Settings;
using HR_system.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR_system.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "الإعدادات";
            var settings = await GetOrCreateSettingsAsync();
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
