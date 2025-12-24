using HR_system.DTOs.Deduction;
using HR_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HR_system.Controllers
{
    public class DeductionsController : Controller
    {
        private readonly IDeductionService _deductionService;
        private readonly IEmployeeService _employeeService;

        public DeductionsController(IDeductionService deductionService, IEmployeeService employeeService)
        {
            _deductionService = deductionService;
            _employeeService = employeeService;
        }

        // GET: Deductions
        public IActionResult Index()
        {
            return View();
        }

        #region AJAX API Endpoints

        // GET: Deductions/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll(int? employeeId, DateTime? startDate, DateTime? endDate, bool isSearch = false)
        {
            try
            {
                // Only return records if explicitly searching
                if (!isSearch)
                {
                    return Json(new { success = true, data = new List<DeductionDto>(), total = 0.0m });
                }

                IEnumerable<DeductionDto> deductions;

                if (startDate.HasValue && endDate.HasValue)
                {
                    deductions = await _deductionService.GetByDateRangeAsync(startDate.Value, endDate.Value);
                }
                else
                {
                    deductions = await _deductionService.GetAllAsync();
                }

                if (employeeId.HasValue)
                {
                    deductions = deductions.Where(d => d.Employee_id == employeeId.Value);
                }

                var total = deductions.Sum(d => d.Amount);
                return Json(new { success = true, data = deductions, total = total });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Deductions/GetById/5
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var deduction = await _deductionService.GetByIdAsync(id);
                if (deduction == null)
                {
                    return Json(new { success = false, message = "السجل غير موجود" });
                }
                return Json(new { success = true, data = deduction });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Deductions/CreateAjax
        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] CreateDeductionDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var result = await _deductionService.CreateAsync(dto);
                return Json(new { success = true, message = "تم إضافة الخصم بنجاح", data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // PUT: Deductions/UpdateAjax/5
        [HttpPut]
        public async Task<IActionResult> UpdateAjax(int id, [FromBody] UpdateDeductionDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var result = await _deductionService.UpdateAsync(id, dto);
                if (result == null)
                {
                    return Json(new { success = false, message = "السجل غير موجود" });
                }
                return Json(new { success = true, message = "تم تحديث الخصم بنجاح", data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // DELETE: Deductions/DeleteAjax/5
        [HttpDelete]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            try
            {
                var result = await _deductionService.DeleteAsync(id);
                if (!result)
                {
                    return Json(new { success = false, message = "السجل غير موجود" });
                }
                return Json(new { success = true, message = "تم حذف الخصم بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Deductions/GetEmployees
        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            try
            {
                var employees = await _employeeService.GetAllAsync();
                var activeEmployees = employees.Where(e => e.Status == "Active")
                    .Select(e => new { id = e.Id, name = e.Emp_name, code = e.Code });
                return Json(new { success = true, data = activeEmployees });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion
    }
}
