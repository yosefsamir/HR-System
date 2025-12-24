using System.ComponentModel.DataAnnotations;

namespace HR_system.DTOs.Employee
{
    /// <summary>
    /// Used when updating an employee
    /// </summary>
    public class UpdateEmployeeDto
    {
        [Required(ErrorMessage = "Employee name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        [Display(Name = "Employee Name")]
        public string Emp_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Employee code is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Code must be between 1 and 50 characters")]
        [Display(Name = "Employee Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "الراتب مطلوب")]
        [Range(0.01, 9999999.99, ErrorMessage = "الراتب يجب أن يكون أكبر من صفر")]
        [Display(Name = "Salary")]
        public decimal Salary { get; set; }

        [StringLength(10)]
        [Display(Name = "Gender")]
        public string? Gender { get; set; }

        [Range(18, 100, ErrorMessage = "العمر يجب أن يكون بين 18 و 100")]
        [Display(Name = "Age")]
        public int? Age { get; set; }

        [Display(Name = "Department")]
        public int? Department_id { get; set; }

        [Required(ErrorMessage = "الوردية مطلوبة")]
        [Display(Name = "Shift")]
        public int Shift_id { get; set; }

        [StringLength(50)]
        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Range(0.1, 10, ErrorMessage = "Overtime multiplier must be between 0.1 and 10")]
        [Display(Name = "Overtime Rate Multiplier")]
        public decimal Rate_overtime_multiplier { get; set; }

        [Range(0.1, 10, ErrorMessage = "Late time multiplier must be between 0.1 and 10")]
        [Display(Name = "Late Time Rate Multiplier")]
        public decimal Rate_latetime_multiplier { get; set; }
    }
}
