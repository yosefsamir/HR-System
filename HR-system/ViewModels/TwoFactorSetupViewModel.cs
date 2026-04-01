using System.ComponentModel.DataAnnotations;

namespace HR_system.ViewModels
{
    public class TwoFactorSetupViewModel
    {
        public bool IsEnabled { get; set; }
        public string UserName { get; set; } = "";

        public string SharedKey { get; set; } = "";
        public string? QrCodeImageDataUrl { get; set; }

        public List<string>? RecoveryCodes { get; set; }

        [Required(ErrorMessage = "رمز التحقق مطلوب")]
        [Display(Name = "رمز التحقق")]
        public string Code { get; set; } = "";
    }
}

