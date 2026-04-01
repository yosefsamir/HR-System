using System.ComponentModel.DataAnnotations;

namespace HR_system.ViewModels
{
    public class TwoFactorLoginViewModel
    {
        public string? ReturnUrl { get; set; }

        [Required(ErrorMessage = "رمز التحقق مطلوب")]
        [Display(Name = "رمز التحقق")]
        public string Code { get; set; } = "";
    }
}
