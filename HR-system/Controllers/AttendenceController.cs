using HR_system.DTOs.Attendence;
using HR_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HR_system.Controllers
{
    public class AttendenceController : Controller
    {
        private readonly IAttendenceService _attendenceService;
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IShiftService _shiftService;

        public AttendenceController(
            IAttendenceService attendenceService, 
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            IShiftService shiftService)
        {
            _attendenceService = attendenceService;
            _employeeService = employeeService;
            _departmentService = departmentService;
            _shiftService = shiftService;
        }

        // GET: Attendence
        public async Task<IActionResult> Index(int? employeeId, DateTime? date)
        {
            var filterDate = date ?? DateTime.Today;
            ViewBag.SelectedDate = filterDate.ToString("yyyy-MM-dd");
            ViewBag.EmployeeFilter = employeeId;
            
            await LoadEmployeesAsync(employeeId);
            
            IEnumerable<AttendenceDto> attendances;
            if (employeeId.HasValue)
            {
                var single = await _attendenceService.GetByEmployeeAndDateAsync(employeeId.Value, filterDate);
                attendances = single != null ? new[] { single } : Array.Empty<AttendenceDto>();
            }
            else
            {
                attendances = await _attendenceService.GetByDateAsync(filterDate);
            }

            return View(attendances);
        }

        // GET: Attendence/Records - View/Search all attendance records
        public IActionResult Records()
        {
            return View();
        }

        // GET: Attendence/Summary - Monthly attendance summary
        public IActionResult Summary()
        {
            return View();
        }

        // GET: Attendence/GetSummary - Get monthly summary for an employee
        [HttpGet]
        public async Task<IActionResult> GetSummary(int employeeId, int month, int year)
        {
            try
            {
                var summary = await _attendenceService.GetMonthlySummaryAsync(employeeId, month, year);
                return Json(new { success = true, data = summary });
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

        #region API Endpoints for AJAX

        // GET: Attendence/GetAll - Get attendance records with filters
        [HttpGet]
        public async Task<IActionResult> GetAll(int? employeeId, DateTime? startDate, DateTime? endDate, int? departmentId, int? shiftId)
        {
            try
            {
                IEnumerable<AttendenceDto> attendances;
                
                // If date range is provided
                if (startDate.HasValue && endDate.HasValue)
                {
                    attendances = await _attendenceService.GetByDateRangeAsync(startDate.Value, endDate.Value);
                }
                // Default to all records
                else
                {
                    attendances = await _attendenceService.GetAllAsync();
                }
                
                // Filter by employee if specified
                if (employeeId.HasValue)
                {
                    attendances = attendances.Where(a => a.Employee_id == employeeId.Value);
                }
                
                // Filter by department if specified
                if (departmentId.HasValue)
                {
                    attendances = attendances.Where(a => a.Department_id == departmentId.Value);
                }
                
                // Filter by shift if specified
                if (shiftId.HasValue)
                {
                    attendances = attendances.Where(a => a.Shift_id == shiftId.Value);
                }

                return Json(new { success = true, data = attendances });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Attendence/GetById/5
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var attendance = await _attendenceService.GetByIdAsync(id);
                if (attendance == null)
                {
                    return Json(new { success = false, message = "السجل غير موجود" });
                }
                return Json(new { success = true, data = attendance });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Attendence/CreateAjax
        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] CreateAttendenceDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var result = await _attendenceService.CreateAsync(dto);
                return Json(new { success = true, message = "تم تسجيل الحضور بنجاح", data = result });
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

        // PUT: Attendence/UpdateAjax/5
        [HttpPut]
        public async Task<IActionResult> UpdateAjax(int id, [FromBody] UpdateAttendenceDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var result = await _attendenceService.UpdateAsync(id, dto);
                if (result == null)
                {
                    return Json(new { success = false, message = "السجل غير موجود" });
                }

                return Json(new { success = true, message = "تم تحديث الحضور بنجاح", data = result });
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

        // DELETE: Attendence/DeleteAjax/5
        [HttpDelete]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            try
            {
                var result = await _attendenceService.DeleteAsync(id);
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

        // GET: Attendence/GetEmployees
        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            try
            {
                var employees = await _employeeService.GetAllAsync();
                var activeEmployees = employees.Where(e => e.Status == "Active")
                    .Select(e => new { 
                        id = e.Id, 
                        name = e.Emp_name, 
                        code = e.Code,
                        shiftName = e.Shift_name,
                        display = $"{e.Code} - {e.Emp_name}"
                    });
                return Json(new { success = true, data = activeEmployees });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Attendence/GetDepartments
        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            try
            {
                var departments = await _departmentService.GetAllAsync();
                var data = departments.Select(d => new { 
                    id = d.Id, 
                    name = d.Department_name 
                });
                return Json(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Attendence/GetShifts
        [HttpGet]
        public async Task<IActionResult> GetShifts()
        {
            try
            {
                var shifts = await _shiftService.GetAllAsync();
                var data = shifts.Select(s => new { 
                    id = s.Id, 
                    name = s.Shift_name,
                    start = s.Start_time.ToString(@"hh\:mm"),
                    end = s.End_time.ToString(@"hh\:mm")
                });
                return Json(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Helper Methods

        private async Task LoadEmployeesAsync(int? selectedEmployeeId = null)
        {
            var employees = await _employeeService.GetAllAsync();
            ViewBag.Employees = new SelectList(
                employees.Where(e => e.Status == "Active").Select(e => new { e.Id, Display = $"{e.Code} - {e.Emp_name}" }),
                "Id", "Display", selectedEmployeeId);
        }

        #endregion
    }
}
