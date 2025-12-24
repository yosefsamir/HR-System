using HR_system.DTOs.Employee;
using HR_system.Services.Interfaces;
using HR_system.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HR_system.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IShiftService _shiftService;

        public EmployeesController(
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            IShiftService shiftService)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
            _shiftService = shiftService;
        }

        // GET: Employees
        public async Task<IActionResult> Index(string? search, int? departmentId, int? shiftId, string? status)
        {
            var allEmployees = await _employeeService.GetAllAsync();
            IEnumerable<EmployeeDto> filteredEmployees = allEmployees;

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                filteredEmployees = await _employeeService.SearchAsync(search);
            }
            
            if (departmentId.HasValue)
            {
                filteredEmployees = filteredEmployees.Where(e => e.Department_id == departmentId);
            }
            
            if (shiftId.HasValue)
            {
                filteredEmployees = filteredEmployees.Where(e => e.Shift_id == shiftId);
            }
            
            if (!string.IsNullOrWhiteSpace(status))
            {
                filteredEmployees = filteredEmployees.Where(e => e.Status == status);
            }

            // Load departments and shifts for filter dropdowns
            var departments = await _departmentService.GetAllAsync();
            var shifts = await _shiftService.GetAllAsync();

            var viewModel = new EmployeesIndexViewModel
            {
                Employees = filteredEmployees,
                SearchTerm = search,
                DepartmentFilter = departmentId,
                ShiftFilter = shiftId,
                StatusFilter = status,
                Departments = new SelectList(departments, "Id", "Department_name", departmentId),
                Shifts = new SelectList(shifts, "Id", "Shift_name", shiftId),
                Statuses = new SelectList(new[] { "Active", "Inactive", "On Leave", "Terminated" }, status),
                TotalCount = allEmployees.Count(),
                ActiveCount = allEmployees.Count(e => e.Status == "Active"),
                InactiveCount = allEmployees.Count(e => e.Status != "Active")
            };

            return View(viewModel);
        }

        // GET: Employees/Create
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            return View(new CreateEmployeeDto
            {
                Status = "Active",
                Rate_overtime_multiplier = 1.5m,
                Rate_latetime_multiplier = 1.0m
            });
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEmployeeDto dto)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync(dto.Department_id, dto.Shift_id);
                return View(dto);
            }

            // Check for unique code
            if (!await _employeeService.IsCodeUniqueAsync(dto.Code))
            {
                ModelState.AddModelError("Code", "يوجد موظف بهذا الكود بالفعل");
                await LoadDropdownsAsync(dto.Department_id, dto.Shift_id);
                return View(dto);
            }

            await _employeeService.CreateAsync(dto);
            TempData["Success"] = "تم إضافة الموظف بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            var dto = new UpdateEmployeeDto
            {
                Emp_name = employee.Emp_name,
                Code = employee.Code,
                Salary = employee.Salary,
                Gender = employee.Gender,
                Age = employee.Age,
                Department_id = employee.Department_id,
                Shift_id = employee.Shift_id,
                Status = employee.Status,
                Rate_overtime_multiplier = employee.Rate_overtime_multiplier,
                Rate_latetime_multiplier = employee.Rate_latetime_multiplier
            };

            ViewBag.EmployeeId = id;
            ViewBag.EmployeeName = employee.Emp_name;
            await LoadDropdownsAsync(employee.Department_id, employee.Shift_id);
            return View(dto);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateEmployeeDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.EmployeeId = id;
                await LoadDropdownsAsync(dto.Department_id, dto.Shift_id);
                return View(dto);
            }

            // Check for unique code (excluding current employee)
            if (!await _employeeService.IsCodeUniqueAsync(dto.Code, id))
            {
                ModelState.AddModelError("Code", "يوجد موظف بهذا الكود بالفعل");
                ViewBag.EmployeeId = id;
                await LoadDropdownsAsync(dto.Department_id, dto.Shift_id);
                return View(dto);
            }

            var result = await _employeeService.UpdateAsync(id, dto);
            if (result == null)
            {
                return NotFound();
            }

            TempData["Success"] = "تم تحديث بيانات الموظف بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // POST: Employees/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _employeeService.DeleteAsync(id);
                if (!result)
                {
                    TempData["Error"] = "الموظف غير موجود";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Success"] = "تم حذف الموظف بنجاح";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        #region Helper Methods

        private async Task LoadDropdownsAsync(int? selectedDepartmentId = null, int? selectedShiftId = null)
        {
            var departments = await _departmentService.GetAllAsync();
            var shifts = await _shiftService.GetAllAsync();

            ViewBag.Departments = new SelectList(departments, "Id", "Department_name", selectedDepartmentId);
            ViewBag.Shifts = new SelectList(shifts, "Id", "Shift_name", selectedShiftId);
            ViewBag.Genders = new SelectList(new[] { "ذكر", "أنثى" });
            ViewBag.Statuses = new SelectList(new[] { "Active", "Inactive", "On Leave", "Terminated" });
        }

        #endregion
    }
}
