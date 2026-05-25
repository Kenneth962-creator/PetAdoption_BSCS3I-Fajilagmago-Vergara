using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetAdoptionManagementSystem.Data;
using PetAdoptionManagementSystem.Models.Domain;
using PetAdoptionManagementSystem.Models.ViewModels;
using PetAdoptionManagementSystem.Services;
using System.Security.Cryptography;
using System.Text;

namespace PetAdoptionManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly OtpService _otpService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IEmailService emailService,
            OtpService otpService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailService = emailService;
            _otpService = otpService;
        }

        private string HashString(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private string GenerateOtp()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        private string GenerateRecoveryCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    EmailConfirmed = false,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Member");
                    user.EmailConfirmed = true; // Auto-confirm for now
                    await _userManager.UpdateAsync(user);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["Success"] = "Registration successful.";
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }

                var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                if (!isPasswordValid)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }

                if (!user.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "You must confirm your email before logging in.");
                    return View(model);
                }

                // Check for lockout
                if (await _userManager.IsLockedOutAsync(user))
                {
                    ModelState.AddModelError(string.Empty, "Account is locked out. Please try again later.");
                    return View(model);
                }

                // Instead of logging the user in, redirect them to the same page but trigger the modal
                var otp = _otpService.GenerateOtp();
                HttpContext.Session.SetString("OtpCode", otp);
                HttpContext.Session.SetString("OtpExpiry", DateTime.UtcNow.AddSeconds(60).ToString("O"));
                HttpContext.Session.SetString("OtpUserId", user.Id);
                HttpContext.Session.SetString("OtpRememberMe", model.RememberMe.ToString());
                
                TempData["GeneratedOtp"] = otp;
                TempData["ShowOtpModal"] = "true";
                return View(model);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpViewModel model)
        {
            var storedOtp = HttpContext.Session.GetString("OtpCode");
            var expiryString = HttpContext.Session.GetString("OtpExpiry");
            var userId = HttpContext.Session.GetString("OtpUserId");

            if (storedOtp == null || expiryString == null || userId == null)
            {
                return Json(new { success = false, message = "OTP session missing or expired." });
            }

            var expiry = DateTime.Parse(expiryString);
            if (DateTime.UtcNow > expiry)
            {
                return Json(new { success = false, expired = true, message = "Code expired." });
            }

            if (model.OtpCode == storedOtp)
            {
                // Invalidate OTP after success
                HttpContext.Session.Remove("OtpCode");
                HttpContext.Session.Remove("OtpExpiry");
                
                HttpContext.Session.SetString("IsOtpVerified", "true");
                
                var user = await _userManager.FindByIdAsync(userId);
                var rememberMeStr = HttpContext.Session.GetString("OtpRememberMe");
                bool.TryParse(rememberMeStr, out bool rememberMe);
                
                await _signInManager.SignInAsync(user, isPersistent: rememberMe);
                
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                string redirectUrl = isAdmin ? Url.Action("Index", "Dashboard", new { area = "Admin" }) : Url.Action("Index", "Profile");
                
                return Json(new { success = true, redirectUrl });
            }

            return Json(new { success = false, message = "Invalid OTP code." });
        }

        [HttpPost]
        public IActionResult ResendOtp()
        {
            var otp = _otpService.GenerateOtp();
            HttpContext.Session.SetString("OtpCode", otp);
            HttpContext.Session.SetString("OtpExpiry", DateTime.UtcNow.AddSeconds(60).ToString("O"));
            
            return Json(new { success = true, newOtp = otp });
        }

        [HttpGet]
        [Authorize]
        public IActionResult ShowRecoveryCodes()
        {
            var codesStr = TempData["RecoveryCodes"] as string;
            if (string.IsNullOrEmpty(codesStr)) return RedirectToAction("Index", "Home");

            ViewBag.RecoveryCodes = codesStr.Split(',');
            return View();
        }

        [HttpGet]
        public IActionResult UseRecoveryCode(string email)
        {
            return View(new UseRecoveryCodeViewModel { Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UseRecoveryCode(UseRecoveryCodeViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid attempt.");
                return View(model);
            }

            var hash = HashString(model.RecoveryCode);
            var recoveryRecord = await _context.RecoveryCodes.FirstOrDefaultAsync(r => r.UserId == user.Id && r.CodeHash == hash && !r.IsUsed);

            if (recoveryRecord == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid or already used recovery code.");
                return View(model);
            }

            // Valid!
            recoveryRecord.IsUsed = true;
            recoveryRecord.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);
            return RedirectUserByRole(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult OtpRequired(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        private IActionResult RedirectUserByRole(ApplicationUser user)
        {
            var isAdmin = _userManager.IsInRoleAsync(user, "Admin").Result;
            if (isAdmin)
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            return RedirectToAction("Index", "Home"); // Should be Catalog later
        }
    }
}

