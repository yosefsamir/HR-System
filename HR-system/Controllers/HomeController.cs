using Microsoft.AspNetCore.Mvc;
using HR_system.Services.Interfaces;

namespace HR_system.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IShiftService _shiftService;

        public HomeController(
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            IShiftService shiftService)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
            _shiftService = shiftService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "الرئيسية";
            
            var employees = await _employeeService.GetAllAsync();
            var departments = await _departmentService.GetAllAsync();
            var shifts = await _shiftService.GetAllAsync();

            ViewBag.EmployeeCount = employees.Count();
            ViewBag.DepartmentCount = departments.Count();
            ViewBag.ShiftCount = shifts.Count();

            return View();
        }
    }
}
