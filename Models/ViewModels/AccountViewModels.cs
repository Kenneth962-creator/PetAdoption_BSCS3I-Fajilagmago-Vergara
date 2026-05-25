using System.ComponentModel.DataAnnotations;

namespace PetAdoptionManagementSystem.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        [Display(Name = "Address")]
        public string? Address { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class VerifyOtpViewModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits.")]
        [Display(Name = "OTP Code")]
        public string OtpCode { get; set; } = string.Empty;

        public string Purpose { get; set; } = "Registration"; // Registration or TwoFactor
        
        public bool RememberMe { get; set; }
    }

    public class UseRecoveryCodeViewModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Recovery Code")]
        public string RecoveryCode { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class ManageProfileViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        [Display(Name = "Address")]
        public string? Address { get; set; }
    }
}