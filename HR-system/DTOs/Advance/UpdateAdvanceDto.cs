using System.ComponentModel.DataAnnotations;

namespace HR_system.DTOs.Advance
{
    /// <summary>
    /// Used when updating an advance
    /// </summary>
    public class UpdateAdvanceDto
    {
        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 9999999.99, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }
    }
}
