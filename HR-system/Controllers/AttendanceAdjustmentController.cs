using HR_system.DTOs.AttendanceAdjustment;
using HR_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HR_system.Controllers
{
    public class AttendanceAdjustmentController : Controller
    {
        private readonly IAttendanceAdjustmentService _adjustmentService;

        public AttendanceAdjustmentController(IAttendanceAdjustmentService adjustmentService)
        {
            _adjustmentService = adjustmentService;
        }

        // GET: AttendanceAdjustment
        public IActionResult Index()
        {
            return View();
        }

        #region AJAX API Endpoints

        // GET: AttendanceAdjustment/GetEmployeesWithAdjustments?month=3&year=2026
        [HttpGet]
        public async Task<IActionResult> GetEmployeesWithAdjustments(int month, int year)
        {
            try
            {
                var data = await _adjustmentService.GetAllActiveEmployeesWithAdjustmentsAsync(month, year);
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: AttendanceAdjustment/SaveAjax
        [HttpPost]
        public async Task<IActionResult> SaveAjax([FromBody] CreateAttendanceAdjustmentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var result = await _adjustmentService.CreateOrUpdateAsync(dto);
                return Json(new { success = true, message = "تم الحفظ بنجاح", data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // DELETE: AttendanceAdjustment/DeleteAjax?id=5
        [HttpDelete]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            try
            {
                var result = await _adjustmentService.DeleteAsync(id);
                if (!result)
                {
                    return Json(new { success = false, message = "السجل غير موجود" });
                }
                return Json(new { success = true, message = "تم الحذف بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: AttendanceAdjustment/GetByMonth?month=3&year=2026
        [HttpGet]
        public async Task<IActionResult> GetByMonth(int month, int year)
        {
            try
            {
                var data = await _adjustmentService.GetByMonthAsync(month, year);
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion
    }
}
