using HR_system.DTOs.MonthlyAttendance;
using HR_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HR_system.Controllers
{
    public class MonthlyAttendanceController : Controller
    {
        private readonly IMonthlyAttendanceService _monthlyAttendanceService;
        private readonly IEmployeeService _employeeService;

        public MonthlyAttendanceController(
            IMonthlyAttendanceService monthlyAttendanceService,
            IEmployeeService employeeService)
        {
            _monthlyAttendanceService = monthlyAttendanceService;
            _employeeService = employeeService;
        }

        // GET: MonthlyAttendance
        public IActionResult Index()
        {
            return View();
        }

        // GET: MonthlyAttendance/Records
        public IActionResult Records()
        {
            return View();
        }

        // GET: MonthlyAttendance/GetAll?month=X&year=Y
        [HttpGet]
        public async Task<IActionResult> GetAll(int month, int year)
        {
            try
            {
                var records = await _monthlyAttendanceService.GetByMonthAsync(month, year);
                return Json(new { success = true, data = records });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: MonthlyAttendance/GetById/5
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var record = await _monthlyAttendanceService.GetByIdAsync(id);
                if (record == null)
                {
                    return Json(new { success = false, message = "السجل غير موجود" });
                }
                return Json(new { success = true, data = record });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: MonthlyAttendance/CreateAjax
        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] CreateMonthlyAttendanceDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var result = await _monthlyAttendanceService.CreateAsync(dto);
                return Json(new { success = true, message = "تم حفظ الحضور الشهري بنجاح", data = result });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ: " + ex.Message });
            }
        }

        // PUT: MonthlyAttendance/UpdateAjax/5
        [HttpPut]
        public async Task<IActionResult> UpdateAjax(int id, [FromBody] CreateMonthlyAttendanceDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var result = await _monthlyAttendanceService.UpdateAsync(id, dto);
                if (result == null)
                {
                    return Json(new { success = false, message = "السجل غير موجود" });
                }

                return Json(new { success = true, message = "تم تحديث الحضور الشهري بنجاح", data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ: " + ex.Message });
            }
        }

        // DELETE: MonthlyAttendance/DeleteAjax/5
        [HttpDelete]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            try
            {
                var result = await _monthlyAttendanceService.DeleteAsync(id);
                if (!result)
                {
                    return Json(new { success = false, message = "السجل غير موجود" });
                }
                return Json(new { success = true, message = "تم حذف السجل بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ: " + ex.Message });
            }
        }

        // GET: MonthlyAttendance/GetEmployees
        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            try
            {
                var employees = await _employeeService.GetAllAsync();
                var activeEmployees = employees.Where(e => e.Status == "Active")
                    .Select(e => new
                    {
                        id = e.Id,
                        name = e.Emp_name,
                        code = e.Code,
                        shiftName = e.Shift_name,
                        departmentName = e.Department_name,
                        display = $"{e.Code} - {e.Emp_name}"
                    });
                return Json(new { success = true, data = activeEmployees });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
