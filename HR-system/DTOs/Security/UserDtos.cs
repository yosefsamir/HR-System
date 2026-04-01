using System.ComponentModel.DataAnnotations;

namespace HR_system.DTOs.Security
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        [Display(Name = "الاسم الأول")]
        [StringLength(50, ErrorMessage = "الاسم الأول لا يتجاوز 50 حرف")]
        public string FirstName { get; set; } = null!;

        [Display(Name = "الاسم الأخير")]
        [StringLength(50, ErrorMessage = "الاسم الأخير لا يتجاوز 50 حرف")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [Display(Name = "اسم المستخدم")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "اسم المستخدم بين 3 و 50 حرف")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور 6 أحرف على الأقل")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور")]
        [Compare("Password", ErrorMessage = "كلمتا المرور غير متطابقتين")]
        public string ConfirmPassword { get; set; } = null!;
    }

    public class EditUserDto
    {
        public string Id { get; set; } = null!;

        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        [Display(Name = "الاسم الأول")]
        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [Display(Name = "الاسم الأخير")]
        [StringLength(50)]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [Display(Name = "اسم المستخدم")]
        [StringLength(50, MinimumLength = 3)]
        public string UserName { get; set; } = null!;

        public bool IsActive { get; set; }
    }

    public class ChangePasswordDto
    {
        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور الجديدة")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور 6 أحرف على الأقل")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور")]
        [Compare("NewPassword", ErrorMessage = "كلمتا المرور غير متطابقتين")]
        public string ConfirmPassword { get; set; } = null!;
    }

    public class UserListItemDto
    {
        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string? LastName { get; set; }
        public string UserName { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
