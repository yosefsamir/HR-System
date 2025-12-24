using HR_system.DTOs.Employee;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HR_system.ViewModels
{
    /// <summary>
    /// ViewModel for Employees Index page - displays list with filters
    /// </summary>
    public class EmployeesIndexViewModel
    {
        public IEnumerable<EmployeeDto> Employees { get; set; } = new List<EmployeeDto>();
        
        // Filter Properties
        public string? SearchTerm { get; set; }
        public int? DepartmentFilter { get; set; }
        public int? ShiftFilter { get; set; }
        public string? StatusFilter { get; set; }
        
        // Dropdowns for filters
        public SelectList? Departments { get; set; }
        public SelectList? Shifts { get; set; }
        public SelectList? Statuses { get; set; }
        
        // Counts
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
    }
}
