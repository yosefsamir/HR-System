using System.ComponentModel.DataAnnotations;

namespace HR_system.DTOs.Department
{
    /// <summary>
    /// Used when updating a department
    /// </summary>
    public class UpdateDepartmentDto
    {
        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        [Display(Name = "Department Name")]
        public string Department_name { get; set; } = string.Empty;
    }
}
