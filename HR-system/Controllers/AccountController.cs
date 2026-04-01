using HR_system.DTOs.Account;
using HR_system.DTOs.Security;
using HR_system.Middleware;
using HR_system.Models;
using HR_system.Security;
using HR_system.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Text.Encodings.Web;

namespace HR_system.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Setup()
        {
            if (_userManager.Users.Any())
                return RedirectToAction(nameof(Login));

            ViewData["Title"] = "إعداد الحساب الأول";
            return View(new CreateUserDto());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Setup(CreateUserDto dto)
        {
            if (_userManager.Users.Any())
                return RedirectToAction(nameof(Login));

            if (!ModelState.IsValid)
                return View(dto);

            var existing = await _userManager.FindByNameAsync(dto.UserName);
            if (existing != null)
            {
                ModelState.AddModelError("", "اسم المستخدم مستخدم بالفعل");
                return View(dto);
            }

            if (!await _roleManager.RoleExistsAsync(RoleNames.Admin))
            {
                var createRole = await _roleManager.CreateAsync(new ApplicationRole
                {
                    Name = RoleNames.Admin,
                    NormalizedName = RoleNames.Admin.ToUpperInvariant(),
                    IsActive = true,
                    CreatedOn = DateTime.Now
                });
                if (!createRole.Succeeded)
                {
                    ModelState.AddModelError("", string.Join("، ", createRole.Errors.Select(e => e.Description)));
                    return View(dto);
                }
            }

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
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError("", err.Description);
                return View(dto);
            }

            var addToRole = await _userManager.AddToRoleAsync(user, RoleNames.Admin);
            if (!addToRole.Succeeded)
            {
                foreach (var err in addToRole.Errors)
                    ModelState.AddModelError("", err.Description);
                return View(dto);
            }

            ConditionalAuthMiddleware.MarkUsersExist();
            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            // If already signed in, redirect to home
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            // If no users exist yet, redirect to the security/user-management page
            var anyUser = _userManager.Users.Any();
            if (!anyUser)
                return RedirectToAction(nameof(Setup));

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null && !user.IsActive)
            {
                ModelState.AddModelError("", "هذا الحساب غير نشط. تواصل مع مدير النظام.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.UserName, model.Password, isPersistent: false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Update LastLogin
                if (user != null)
                {
                    user.LastLogin = DateTime.Now;
                    await _userManager.UpdateAsync(user);
                }

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(nameof(LoginWith2fa), new { returnUrl });
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "الحساب مقفل مؤقتاً بسبب محاولات دخول متعددة. حاول لاحقاً.");
                return View(model);
            }

            ModelState.AddModelError("", "اسم المستخدم أو كلمة المرور غير صحيحة");
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWith2fa(string? returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
                return RedirectToAction(nameof(Login), new { returnUrl });

            ViewData["ReturnUrl"] = returnUrl;
            return View(new TwoFactorLoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginWith2fa(TwoFactorLoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
                return RedirectToAction(nameof(Login), new { returnUrl = model.ReturnUrl });

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "هذا الحساب غير نشط. تواصل مع مدير النظام.");
                return View(model);
            }

            var code = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(code, isPersistent: false, rememberClient: false);

            if (result.Succeeded)
            {
                user.LastLogin = DateTime.Now;
                await _userManager.UpdateAsync(user);

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "الحساب مقفل مؤقتاً بسبب محاولات دخول متعددة. حاول لاحقاً.");
                return View(model);
            }

            ModelState.AddModelError("", "رمز التحقق غير صحيح");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> TwoFactor()
        {
            ViewData["Title"] = "المصادقة الثنائية";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var vm = await BuildTwoFactorSetupViewModelAsync(user);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableTwoFactor(TwoFactorSetupViewModel model)
        {
            ViewData["Title"] = "المصادقة الثنائية";

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            if (!ModelState.IsValid)
                return View(nameof(TwoFactor), await BuildTwoFactorSetupViewModelAsync(user));

            var code = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, code);
            if (!isValid)
            {
                ModelState.AddModelError("", "رمز التحقق غير صحيح");
                return View(nameof(TwoFactor), await BuildTwoFactorSetupViewModelAsync(user));
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            var vm = await BuildTwoFactorSetupViewModelAsync(user);
            vm.RecoveryCodes = (recoveryCodes ?? Array.Empty<string>()).ToList();
            return View(nameof(TwoFactor), vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableTwoFactor(TwoFactorSetupViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            if (string.IsNullOrWhiteSpace(model.Code))
            {
                ModelState.AddModelError("", "أدخل رمز التحقق لتعطيل المصادقة الثنائية");
                return View(nameof(TwoFactor), await BuildTwoFactorSetupViewModelAsync(user));
            }

            var ok = await ValidateTwoFactorCodeOrRecoveryAsync(user, model.Code);
            if (!ok)
            {
                ModelState.AddModelError("", "رمز التحقق غير صحيح");
                return View(nameof(TwoFactor), await BuildTwoFactorSetupViewModelAsync(user));
            }

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            return RedirectToAction(nameof(TwoFactor));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetAuthenticatorKey(TwoFactorSetupViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            if (string.IsNullOrWhiteSpace(model.Code))
            {
                ModelState.AddModelError("", "أدخل رمز التحقق لإعادة ضبط المفتاح");
                return View(nameof(TwoFactor), await BuildTwoFactorSetupViewModelAsync(user));
            }

            var ok = await ValidateTwoFactorCodeOrRecoveryAsync(user, model.Code);
            if (!ok)
            {
                ModelState.AddModelError("", "رمز التحقق غير صحيح");
                return View(nameof(TwoFactor), await BuildTwoFactorSetupViewModelAsync(user));
            }

            await _userManager.ResetAuthenticatorKeyAsync(user);
            await _userManager.SetTwoFactorEnabledAsync(user, false);
            return RedirectToAction(nameof(TwoFactor));
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task<TwoFactorSetupViewModel> BuildTwoFactorSetupViewModelAsync(ApplicationUser user)
        {
            var isEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

            var key = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrWhiteSpace(key))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                key = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var issuer = "HR System";
            var accountName = user.UserName ?? user.Email ?? user.Id.ToString();
            var otpauthUri = BuildOtpAuthUri(issuer, accountName, key!);

            return new TwoFactorSetupViewModel
            {
                IsEnabled = isEnabled,
                UserName = user.UserName ?? "",
                SharedKey = FormatKey(key!),
                QrCodeImageDataUrl = GenerateQrCodeDataUrl(otpauthUri)
            };
        }

        private static string BuildOtpAuthUri(string issuer, string accountName, string secret)
        {
            return string.Format(
                "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
                UrlEncoder.Default.Encode(issuer),
                UrlEncoder.Default.Encode(accountName),
                UrlEncoder.Default.Encode(secret));
        }

        private static string FormatKey(string unformattedKey)
        {
            var result = new List<string>();
            for (var i = 0; i + 4 <= unformattedKey.Length; i += 4)
                result.Add(unformattedKey.Substring(i, 4));

            var remaining = unformattedKey.Length % 4;
            if (remaining != 0)
                result.Add(unformattedKey[^remaining..]);

            return string.Join(" ", result).ToLowerInvariant();
        }

        private static string? GenerateQrCodeDataUrl(string content)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(data);
            var bytes = qrCode.GetGraphic(8);
            return $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
        }

        private async Task<bool> ValidateTwoFactorCodeOrRecoveryAsync(ApplicationUser user, string input)
        {
            var code = input.Replace(" ", string.Empty).Replace("-", string.Empty);
            if (string.IsNullOrWhiteSpace(code))
                return false;

            var ok = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, code);
            if (ok)
                return true;

            // Allow using a recovery code as an alternative (consumes the code).
            var redeem = await _userManager.RedeemTwoFactorRecoveryCodeAsync(user, code);
            return redeem.Succeeded;
        }
    }
}
