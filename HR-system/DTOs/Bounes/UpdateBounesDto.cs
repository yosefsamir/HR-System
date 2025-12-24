using System.ComponentModel.DataAnnotations;

namespace HR_system.DTOs.Bounes
{
    /// <summary>
    /// Used when updating a bonus
    /// </summary>
    public class UpdateBounesDto
    {
        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 9999999.99, ErrorMessage = "Amount must be greater than 0")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        [Display(Name = "Reason")]
        public string? Reason { get; set; }
    }
}
