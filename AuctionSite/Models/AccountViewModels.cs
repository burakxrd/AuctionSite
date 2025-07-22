using System.ComponentModel.DataAnnotations;
using AuctionSite.Resources; // SharedResources'a erişim için eklendi

namespace AuctionSite.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessageResourceName = nameof(SharedResources.EmailRequired), ErrorMessageResourceType = typeof(SharedResources))]
        [EmailAddress(ErrorMessageResourceName = nameof(SharedResources.EmailInvalid), ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = nameof(SharedResources.Email))]
        public string? Email { get; set; }

        [Required(ErrorMessageResourceName = nameof(SharedResources.UsernameRequired), ErrorMessageResourceType = typeof(SharedResources))]
        [StringLength(256, ErrorMessageResourceName = nameof(SharedResources.StringLengthValidation), ErrorMessageResourceType = typeof(SharedResources), MinimumLength = 3)]
        [Display(Name = nameof(SharedResources.Username))]
        public string? Username { get; set; }

        [Required(ErrorMessageResourceName = nameof(SharedResources.PasswordRequired), ErrorMessageResourceType = typeof(SharedResources))]
        [StringLength(100, ErrorMessageResourceName = nameof(SharedResources.StringLengthValidation), ErrorMessageResourceType = typeof(SharedResources), MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = nameof(SharedResources.Password))]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = nameof(SharedResources.ConfirmPassword))]
        [Compare("Password", ErrorMessageResourceName = nameof(SharedResources.PasswordMismatch), ErrorMessageResourceType = typeof(SharedResources))]
        public string? ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessageResourceName = nameof(SharedResources.EmailRequired), ErrorMessageResourceType = typeof(SharedResources))]
        [EmailAddress(ErrorMessageResourceName = nameof(SharedResources.EmailInvalid), ErrorMessageResourceType = typeof(SharedResources))]
        public required string Email { get; set; }

        [Required(ErrorMessageResourceName = nameof(SharedResources.PasswordRequired), ErrorMessageResourceType = typeof(SharedResources))]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Display(Name = nameof(SharedResources.RememberMe))]
        public bool RememberMe { get; set; }
    }

    public class ManageViewModel
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool HasAuthenticator { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessageResourceName = nameof(SharedResources.OldPasswordRequired), ErrorMessageResourceType = typeof(SharedResources))]
        [DataType(DataType.Password)]
        [Display(Name = nameof(SharedResources.OldPassword))]
        public required string OldPassword { get; set; }

        [Required(ErrorMessageResourceName = nameof(SharedResources.NewPasswordRequired), ErrorMessageResourceType = typeof(SharedResources))]
        [StringLength(100, ErrorMessageResourceName = nameof(SharedResources.StringLengthValidation), ErrorMessageResourceType = typeof(SharedResources), MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = nameof(SharedResources.NewPassword))]
        public required string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = nameof(SharedResources.ConfirmNewPassword))]
        [Compare("NewPassword", ErrorMessageResourceName = nameof(SharedResources.NewPasswordMismatch), ErrorMessageResourceType = typeof(SharedResources))]
        public required string ConfirmPassword { get; set; }
    }

    public class ChangeEmailViewModel
    {
        [Required(ErrorMessageResourceName = nameof(SharedResources.NewEmailRequired), ErrorMessageResourceType = typeof(SharedResources))]
        [EmailAddress(ErrorMessageResourceName = nameof(SharedResources.EmailInvalid), ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = nameof(SharedResources.NewEmail))]
        public string? NewEmail { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required(ErrorMessageResourceName = nameof(SharedResources.VerificationCodeRequired), ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = nameof(SharedResources.VerificationCode))]
        public string? Code { get; set; }

        public string? EmailPurpose { get; set; }
    }

    public class SetPhoneNumberViewModel
    {
        [Phone(ErrorMessageResourceName = nameof(SharedResources.PhoneNumberInvalid), ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = nameof(SharedResources.PhoneNumber))]
        public string? PhoneNumber { get; set; }
    }

    public class ChangeUsernameViewModel
    {
        [Required(ErrorMessageResourceName = nameof(SharedResources.UsernameRequired), ErrorMessageResourceType = typeof(SharedResources))]
        [StringLength(256, ErrorMessageResourceName = nameof(SharedResources.StringLengthValidation), ErrorMessageResourceType = typeof(SharedResources), MinimumLength = 3)]
        [Display(Name = nameof(SharedResources.NewUsername))]
        public string? NewUsername { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessageResourceName = nameof(SharedResources.EmailRequired), ErrorMessageResourceType = typeof(SharedResources))]
        [EmailAddress(ErrorMessageResourceName = nameof(SharedResources.EmailInvalid), ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = nameof(SharedResources.Email))]
        public string? Email { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessageResourceName = nameof(SharedResources.EmailRequired), ErrorMessageResourceType = typeof(SharedResources))]
        [EmailAddress(ErrorMessageResourceName = nameof(SharedResources.EmailInvalid), ErrorMessageResourceType = typeof(SharedResources))]
        [Display(Name = nameof(SharedResources.Email))]
        public string? Email { get; set; }

        [Required(ErrorMessageResourceName = nameof(SharedResources.PasswordRequired), ErrorMessageResourceType = typeof(SharedResources))]
        [StringLength(100, ErrorMessageResourceName = nameof(SharedResources.StringLengthValidation), ErrorMessageResourceType = typeof(SharedResources), MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = nameof(SharedResources.Password))]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = nameof(SharedResources.ConfirmPassword))]
        [Compare("Password", ErrorMessageResourceName = nameof(SharedResources.PasswordMismatch), ErrorMessageResourceType = typeof(SharedResources))]
        public string? ConfirmPassword { get; set; }

        [Required]
        public string? Code { get; set; }
    }
}
