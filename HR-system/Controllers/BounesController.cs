using HR_system.DTOs.Bounes;
using HR_system.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HR_system.Controllers
{
    public class BounesController : Controller
    {
        private readonly IBounesService _bounesService;
        private readonly IEmployeeService _employeeService;

        public BounesController(IBounesService bounesService, IEmployeeService employeeService)
        {
            _bounesService = bounesService;
            _employeeService = employeeService;
        }

        // GET: Bounes
        public IActionResult Index()
        {
            return View();
        }

        #region AJAX API Endpoints

        // GET: Bounes/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll(int? employeeId, DateTime? startDate, DateTime? endDate, bool isSearch = false)
        {
            try
            {
                // Only return records if explicitly searching
                if (!isSearch)
                {
                    return Json(new { success = true, data = new List<BounesDto>(), total = 0.0m });
                }

                IEnumerable<BounesDto> bonuses;

                if (startDate.HasValue && endDate.HasValue)
                {
                    bonuses = await _bounesService.GetByDateRangeAsync(startDate.Value, endDate.Value);
                }
                else
                {
                    bonuses = await _bounesService.GetAllAsync();
                }

                if (employeeId.HasValue)
                {
                    bonuses = bonuses.Where(b => b.Employee_id == employeeId.Value);
                }

                var total = bonuses.Sum(b => b.Amount);
                return Json(new { success = true, data = bonuses, total = total });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Bounes/GetById/5
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var bonus = await _bounesService.GetByIdAsync(id);
                if (bonus == null)
                {
                    return Json(new { success = false, message = "السجل غير موجود" });
                }
                return Json(new { success = true, data = bonus });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Bounes/CreateAjax
        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] CreateBounesDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var result = await _bounesService.CreateAsync(dto);
                return Json(new { success = true, message = "تم إضافة المكافأة بنجاح", data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // PUT: Bounes/UpdateAjax/5
        [HttpPut]
        public async Task<IActionResult> UpdateAjax(int id, [FromBody] UpdateBounesDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                var result = await _bounesService.UpdateAsync(id, dto);
                if (result == null)
                {
                    return Json(new { success = false, message = "السجل غير موجود" });
                }
                return Json(new { success = true, message = "تم تحديث المكافأة بنجاح", data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // DELETE: Bounes/DeleteAjax/5
        [HttpDelete]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            try
            {
                var result = await _bounesService.DeleteAsync(id);
                if (!result)
                {
                    return Json(new { success = false, message = "السجل غير موجود" });
                }
                return Json(new { success = true, message = "تم حذف المكافأة بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Bounes/GetEmployees
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
