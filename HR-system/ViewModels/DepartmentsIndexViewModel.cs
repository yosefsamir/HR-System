using HR_system.DTOs.Department;

namespace HR_system.ViewModels
{
    /// <summary>
    /// ViewModel for Departments Index page - displays list and allows inline add/edit
    /// </summary>
    public class DepartmentsIndexViewModel
    {
        public IEnumerable<DepartmentDto> Departments { get; set; } = new List<DepartmentDto>();
        public CreateDepartmentDto NewDepartment { get; set; } = new CreateDepartmentDto();
        public int? EditingDepartmentId { get; set; }
        public UpdateDepartmentDto? EditDepartment { get; set; }
    }
}
