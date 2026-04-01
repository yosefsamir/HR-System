using HR_system.DTOs.Security;
using HR_system.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR_system.Controllers
{
    public class SecurityController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SecurityController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // ── List Users ───────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "الأمان وإدارة المستخدمين";

            var users = await _userManager.Users
                .OrderBy(u => u.CreatedOn)
                .Select(u => new UserListItemDto
                {
                    Id = u.Id.ToString(),
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    UserName = u.UserName!,
                    IsActive = u.IsActive,
                    LastLogin = u.LastLogin,
                    CreatedOn = u.CreatedOn
                })
                .ToListAsync();

            return View(users);
        }

        // ── Create User ──────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("، ", errors) });
            }

            // Check duplicate username
            var existing = await _userManager.FindByNameAsync(dto.UserName);
            if (existing != null)
                return Json(new { success = false, message = "اسم المستخدم مستخدم بالفعل" });

            var user = new ApplicationUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                UserName = dto.UserName,
                Email = dto.UserName + "@hrsystem.local",
                IsActive = true,
                CreatedOn = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (result.Succeeded)
                return Json(new
                {
                    success = true,
                    message = "تم إنشاء المستخدم بنجاح",
                    user = new
                    {
                        id = user.Id.ToString(),
                        firstName = user.FirstName,
                        lastName = user.LastName ?? "",
                        userName = user.UserName
                    }
                });

            var errs = result.Errors.Select(e => e.Description);
            return Json(new { success = false, message = string.Join("، ", errs) });
        }

        // ── Edit User ────────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody] EditUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("، ", errors) });
            }

            if (!Guid.TryParse(dto.Id, out var userId))
                return Json(new { success = false, message = "معرّف المستخدم غير صالح" });

            var user = await _userManager.FindByIdAsync(dto.Id);
            if (user == null)
                return Json(new { success = false, message = "المستخدم غير موجود" });

            // Prevent disabling the last active user
            if (!dto.IsActive)
            {
                var activeCount = _userManager.Users.Count(u => u.IsActive && u.Id != user.Id);
                if (activeCount == 0)
                    return Json(new { success = false, message = "لا يمكن تعطيل المستخدم الوحيد النشط" });
            }

            // Check duplicate username (another user)
            var duplicate = await _userManager.FindByNameAsync(dto.UserName);
            if (duplicate != null && duplicate.Id != user.Id)
                return Json(new { success = false, message = "اسم المستخدم مستخدم بالفعل" });

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.UserName = dto.UserName;
            user.NormalizedUserName = dto.UserName.ToUpperInvariant();
            user.IsActive = dto.IsActive;
            user.ModifiedOn = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                return Json(new { success = true, message = "تم تحديث المستخدم بنجاح" });

            var errs = result.Errors.Select(e => e.Description);
            return Json(new { success = false, message = string.Join("، ", errs) });
        }

        // ── Change Password ──────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join("، ", errors) });
            }

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                return Json(new { success = false, message = "المستخدم غير موجود" });

            // Remove old password then set new one
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (result.Succeeded)
                return Json(new { success = true, message = "تم تغيير كلمة المرور بنجاح" });

            var errs = result.Errors.Select(e => e.Description);
            return Json(new { success = false, message = string.Join("، ", errs) });
        }

        // ── Delete User ──────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromBody] DeleteUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.Id);
            if (user == null)
                return Json(new { success = false, message = "المستخدم غير موجود" });

            // Prevent deleting the last user
            var totalUsers = _userManager.Users.Count();
            if (totalUsers <= 1)
                return Json(new { success = false, message = "لا يمكن حذف المستخدم الوحيد في النظام" });

            // Prevent deleting yourself
            var currentUserId = _userManager.GetUserId(User);
            if (user.Id.ToString() == currentUserId)
                return Json(new { success = false, message = "لا يمكنك حذف حسابك الخاص" });

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
                return Json(new { success = true, message = "تم حذف المستخدم بنجاح" });

            var errs = result.Errors.Select(e => e.Description);
            return Json(new { success = false, message = string.Join("، ", errs) });
        }

        // ── Toggle Active ────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive([FromBody] ToggleActiveDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.Id);
            if (user == null)
                return Json(new { success = false, message = "المستخدم غير موجود" });

            if (user.IsActive)
            {
                var activeCount = _userManager.Users.Count(u => u.IsActive && u.Id != user.Id);
                if (activeCount == 0)
                    return Json(new { success = false, message = "لا يمكن تعطيل المستخدم الوحيد النشط" });
                user.IsActive = false;
            }
            else
            {
                user.IsActive = true;
            }

            user.ModifiedOn = DateTime.Now;
            await _userManager.UpdateAsync(user);

            return Json(new
            {
                success = true,
                isActive = user.IsActive,
                message = user.IsActive ? "تم تفعيل المستخدم" : "تم تعطيل المستخدم"
            });
        }
    }

    // Small helper DTOs used only in this controller
    public class DeleteUserDto { public string Id { get; set; } = null!; }
    public class ToggleActiveDto { public string Id { get; set; } = null!; }
}
