using System.ComponentModel.DataAnnotations;

namespace HR_system.DTOs.Advance
{
    /// <summary>
    /// Used when creating a new advance
    /// </summary>
    public class CreateAdvanceDto
    {
        [Required(ErrorMessage = "Employee is required")]
        [Display(Name = "Employee")]
        public int Employee_id { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 9999999.99, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }
    }
}
