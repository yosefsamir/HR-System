using HR_system.DTOs.Salary;
using HR_system.DTOs.PayRoll;
using HR_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HR_system.Controllers
{
    public class PayRollController : Controller
    {
        private readonly ISalaryService _salaryService;

        public PayRollController(ISalaryService salaryService)
        {
            _salaryService = salaryService;
        }

        // GET: PayRoll
        public IActionResult Index()
        {
            return View();
        }

        // GET: PayRoll/Saved
        public IActionResult Saved()
        {
            return View();
        }

        // POST: PayRoll/Calculate
        [HttpPost]
        public async Task<IActionResult> Calculate([FromBody] SalaryCalculationRequestDto request)
        {
            try
            {
                if (request.WorkingDaysInMonth <= 0)
                {
                    return Json(new { success = false, message = "عدد أيام العمل يجب أن يكون أكبر من صفر" });
                }

                if (request.Month < 1 || request.Month > 12)
                {
                    return Json(new { success = false, message = "الشهر غير صحيح" });
                }

                var result = await _salaryService.CalculateAllEmployeesSalariesAsync(request);

                if (result.Employees.Count == 0)
                {
                    return Json(new { success = false, message = "لا يوجد موظفين لديهم سجلات في هذا الشهر" });
                }

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: PayRoll/Check?month=X&year=Y
        [HttpGet]
        public async Task<IActionResult> Check(int month, int year)
        {
            try
            {
                var result = await _salaryService.PayRollExistsAsync(month, year);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { exists = false, error = ex.Message });
            }
        }

        // POST: PayRoll/Save
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] SavePayRollRequestDto request)
        {
            try
            {
                var result = await _salaryService.SavePayRollAsync(request);
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new SavePayRollResponseDto { Success = false, Message = ex.Message });
            }
        }

        // GET: PayRoll/GetSaved?month=X&year=Y
        [HttpGet]
        public async Task<IActionResult> GetSaved(int month, int year)
        {
            try
            {
                var result = await _salaryService.GetSavedPayRollAsync(month, year);
                if (result == null)
                {
                    return Json(new { success = false, message = "لا يوجد بيانات محفوظة لهذا الشهر" });
                }
                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: PayRoll/UpdatePaid
        [HttpPost]
        public async Task<IActionResult> UpdatePaid([FromBody] UpdatePaidSalaryDto request)
        {
            try
            {
                var success = await _salaryService.UpdatePaidSalaryAsync(request);
                return Json(new { success, message = success ? "تم التحديث بنجاح" : "حدث خطأ في التحديث" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // DELETE: PayRoll/Delete?month=X&year=Y
        [HttpDelete]
        public async Task<IActionResult> Delete(int month, int year)
        {
            try
            {
                var success = await _salaryService.DeleteMonthPayRollAsync(month, year);
                return Json(new { success, message = success ? "تم حذف بيانات الشهر بنجاح" : "لا يوجد بيانات لهذا الشهر" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
