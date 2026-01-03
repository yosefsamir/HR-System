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
        private readonly IAttendanceExcelService _attendanceExcelService;

        public AttendenceController(
            IAttendenceService attendenceService, 
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            IShiftService shiftService,
            IAttendanceExcelService attendanceExcelService)
        {
            _attendenceService = attendenceService;
            _employeeService = employeeService;
            _departmentService = departmentService;
            _shiftService = shiftService;
            _attendanceExcelService = attendanceExcelService;
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

        // GET: Attendence/DownloadTemplate
        [HttpGet]
        public async Task<IActionResult> DownloadTemplate(DateTime? date)
        {
            try
            {
                var workDate = date ?? DateTime.Today;
                var fileBytes = await _attendanceExcelService.GenerateTemplateAsync(workDate);
                var fileName = $"قالب_الحضور_{workDate:yyyy-MM-dd}.xlsx";
                
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ: " + ex.Message });
            }
        }

        // POST: Attendence/ImportFromExcel
        [HttpPost]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "الرجاء اختيار ملف" });
                }

                if (!file.FileName.EndsWith(".xlsx") && !file.FileName.EndsWith(".xls"))
                {
                    return Json(new { success = false, message = "الرجاء اختيار ملف Excel صحيح (.xlsx أو .xls)" });
                }

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                var result = await _attendanceExcelService.ImportFromExcelAsync(stream);

                var message = $"تم استيراد {result.SuccessCount} سجل بنجاح";
                if (result.FailedCount > 0)
                {
                    message += $" | فشل {result.FailedCount} سجل";
                }
                if (result.SkippedCount > 0)
                {
                    message += $" | تم تخطي {result.SkippedCount} سجل";
                }

                return Json(new
                {
                    success = true,
                    message = message,
                    data = result.ImportedRecords,
                    totalRows = result.TotalRows,
                    successCount = result.SuccessCount,
                    failedCount = result.FailedCount,
                    skippedCount = result.SkippedCount,
                    errors = result.Errors.Take(20).ToList(),
                    warnings = result.Warnings.Take(20).ToList(),
                    hasMoreErrors = result.Errors.Count > 20
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ في قراءة الملف: " + ex.Message });
            }
        }

        #endregion

        #region Recalculation

        // POST: Attendence/RecalculateAttendance
        [HttpPost]
        public async Task<IActionResult> RecalculateAttendance([FromBody] RecalculateRequest request)
        {
            try
            {
                // Validation
                if (request.StartDate > request.EndDate)
                {
                    return Json(new { success = false, message = "تاريخ البداية يجب أن يكون قبل تاريخ النهاية" });
                }

                // Limit date range to prevent very long operations (max 3 months)
                var maxDays = 93; // ~3 months
                if ((request.EndDate - request.StartDate).TotalDays > maxDays)
                {
                    return Json(new { success = false, message = $"نطاق التاريخ يجب أن لا يتجاوز {maxDays} يوم" });
                }

                // Get all attendance records in the date range
                var attendances = await _attendenceService.GetByDateRangeAsync(request.StartDate, request.EndDate);
                
                // Apply optional filters
                if (request.EmployeeId.HasValue)
                {
                    attendances = attendances.Where(a => a.Employee_id == request.EmployeeId.Value);
                }
                
                if (request.DepartmentId.HasValue)
                {
                    attendances = attendances.Where(a => a.Department_id == request.DepartmentId.Value);
                }

                var attendanceList = attendances.ToList();
                int totalRecords = attendanceList.Count;
                int successCount = 0;
                int skippedCount = 0;
                var errors = new List<string>();

                foreach (var attendance in attendanceList)
                {
                    try
                    {
                        // Skip records that are marked as absent (no calculation needed)
                        // OR skip records with missing check times when not absent (corrupt data)
                        if (!attendance.Is_Absent && (!attendance.Check_In_time.HasValue || !attendance.Check_out_time.HasValue))
                        {
                            skippedCount++;
                            errors.Add($"الصف {attendance.Employee_code} ({attendance.Work_date:yyyy-MM-dd}): بيانات الحضور غير مكتملة");
                            continue;
                        }

                        // Create update DTO with current data to trigger recalculation
                        var updateDto = new UpdateAttendenceDto
                        {
                            Work_date = attendance.Work_date,
                            Is_absent = attendance.Is_Absent,
                            Check_In_time = attendance.Check_In_time,
                            Check_out_time = attendance.Check_out_time,
                            Permission_minutes = attendance.Permission_time
                        };

                        // Update the record (this will trigger recalculation in the service)
                        var result = await _attendenceService.UpdateAsync(attendance.Id, updateDto);
                        
                        if (result != null)
                        {
                            successCount++;
                        }
                        else
                        {
                            skippedCount++;
                            errors.Add($"الصف {attendance.Employee_code} ({attendance.Work_date:yyyy-MM-dd}): فشل التحديث");
                        }
                    }
                    catch (Exception ex)
                    {
                        skippedCount++;
                        errors.Add($"الصف {attendance.Employee_code} ({attendance.Work_date:yyyy-MM-dd}): {ex.Message}");
                        
                        // Log for debugging
                        Console.WriteLine($"Error recalculating attendance {attendance.Id}: {ex.Message}");
                    }
                }

                // Build response message
                var message = $"تم إعادة حساب {successCount} سجل بنجاح";
                if (skippedCount > 0)
                {
                    message += $" | تم تخطي {skippedCount} سجل";
                }

                return Json(new { 
                    success = true, 
                    message = message,
                    totalRecords = totalRecords,
                    successCount = successCount,
                    skippedCount = skippedCount,
                    errors = errors.Take(10).ToList(), // Return first 10 errors
                    hasMoreErrors = errors.Count > 10
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"حدث خطأ: {ex.Message}" });
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
