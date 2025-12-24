using HR_system.DTOs.Advance;
using HR_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HR_system.Controllers
{
    public class AdvancesController : Controller
    {
        private readonly IAdvanceService _advanceService;
        private readonly IEmployeeService _employeeService;

        public AdvancesController(IAdvanceService advanceService, IEmployeeService employeeService)
        {
            _advanceService = advanceService;
            _employeeService = employeeService;
        }

        // GET: Advances
        public IActionResult Index()
        {
            return View();
        }

        #region AJAX API Endpoints

        // GET: Advances/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll(int? employeeId, DateTime? startDate, DateTime? endDate, bool isSearch = false)
        {
            try
            {
                // Only return records if explicitly searching
                if (!isSearch)
                {
                    return Json(new { success = true, data = new List<AdvanceDto>(), total = 0.0m });
                }

                IEnumerable<AdvanceDto> advances;

                if (startDate.HasValue && endDate.HasValue)
                {
                    advances = await _advanceService.GetByDateRangeAsync(startDate.Value, endDate.Value);
                }
                else
                {
                    advances = await _advanceService.GetAllAsync();
                }

                if (employeeId.HasValue)
                {
                    advances = advances.Where(a => a.Employee_id == employeeId.Value);
                }

                var total = advances.Sum(a => a.Amount);
                return Json(new { success = true, data = advances, total = total });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Advances/GetById/5
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var advance = await _advanceService.GetByIdAsync(id);
                if (advance == null)
                {
                    return Json(new { success = false, message = "السجل غير موجود" });
                }
                return Json(new { success = true, data = advance });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Advances/CreateAjax
        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] CreateAdvanceDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var result = await _advanceService.CreateAsync(dto);
                return Json(new { success = true, message = "تم إضافة السلفة بنجاح", data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // PUT: Advances/UpdateAjax/5
        [HttpPut]
        public async Task<IActionResult> UpdateAjax(int id, [FromBody] UpdateAdvanceDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var result = await _advanceService.UpdateAsync(id, dto);
                if (result == null)
                {
                    return Json(new { success = false, message = "السجل غير موجود" });
                }
                return Json(new { success = true, message = "تم تحديث السلفة بنجاح", data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // DELETE: Advances/DeleteAjax/5
        [HttpDelete]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            try
            {
                var result = await _advanceService.DeleteAsync(id);
                if (!result)
                {
                    return Json(new { success = false, message = "السجل غير موجود" });
                }
                return Json(new { success = true, message = "تم حذف السلفة بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Advances/GetEmployees
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
