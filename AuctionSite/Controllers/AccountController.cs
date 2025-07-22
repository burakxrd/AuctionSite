using AuctionSite.Models;
using AuctionSite.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Localization;
using AuctionSite.Resources;
using System.Globalization;


namespace AuctionSite.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private readonly IStringLocalizer<SharedResources> _sharedLocalizer;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IEmailSender emailSender,
            IStringLocalizer<SharedResources> sharedLocalizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailSender = emailSender;
            _sharedLocalizer = sharedLocalizer;
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email!);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("PasswordResetLinkSentCheckEmail", currentCulture) ?? "";
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

                await _emailSender.SendEmailAsync(model.Email!,
                    SharedResources.ResourceManager.GetString("ResetYourPasswordSubject", currentCulture) ?? "",
                    string.Format(SharedResources.ResourceManager.GetString("ResetPasswordEmailBody", currentCulture) ?? "", HtmlEncoder.Default.Encode(callbackUrl!)));

                TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("PasswordResetLinkSentCheckEmail", currentCulture) ?? "";
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string? code = null)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            if (code == null)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("PasswordResetCodeNotSpecified", currentCulture) ?? "";
                return RedirectToAction(nameof(Login));
            }
            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email!);
            if (user == null)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("PasswordResetFailed", currentCulture) ?? "";
                return View();
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code!, model.Password!);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("PasswordResetSuccess", currentCulture) ?? "";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }
        [HttpGet]
        [Authorize]
        [ResponseCache(NoStore = true, Duration = 0, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Manage()
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("LoginRequiredForAccountManagement", currentCulture) ?? "";
                return RedirectToAction("Login", "Account");
            }

            var model = new ManageViewModel
            {
                Username = currentUser.UserName,
                Email = currentUser.Email,
                PhoneNumber = currentUser.PhoneNumber,
                IsEmailConfirmed = currentUser.EmailConfirmed,
            };

            ViewBag.VirtualBalance = currentUser.VirtualBalance;

            return View(model);
        }
        [HttpGet]
        [AllowAnonymous]

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Username, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password!);

                if (result.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, protocol: HttpContext.Request.Scheme);

                    await _emailSender.SendEmailAsync(model.Email!,
                        SharedResources.ResourceManager.GetString("ConfirmYourEmailSubject", currentCulture) ?? "",
                        string.Format(SharedResources.ResourceManager.GetString("ConfirmEmailBody", currentCulture) ?? "", HtmlEncoder.Default.Encode(callbackUrl!)));

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("AccountCreatedConfirmEmail", currentCulture) ?? "";
                        return RedirectToAction(nameof(Login));
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("SuccessfullyRegisteredAndLoggedIn", currentCulture) ?? "";
                        return RedirectToAction(nameof(HomeController.Index), "Home");
                    }
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    ModelState.AddModelError(string.Empty, SharedResources.ResourceManager.GetString("InvalidLoginAttempt", currentCulture) ?? "");
                    if (user != null)
                    {
                        await _userManager.AccessFailedAsync(user);
                        if (await _userManager.IsLockedOutAsync(user))
                        {
                            TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("AccountLockedOut", currentCulture) ?? "";
                            return RedirectToPage("./Lockout");
                        }
                    }
                    return View(model);
                }

                if (await _userManager.IsLockedOutAsync(user))
                {
                    TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("AccountLockedOut", currentCulture) ?? "";
                    return RedirectToPage("./Lockout");
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                };

                var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Secret Key 'Jwt:Key' configuration is missing.");
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var expireDaysString = _configuration["Jwt:ExpireDays"] ?? throw new InvalidOperationException("JWT Expire Days 'Jwt:ExpireDays' configuration is not a valid number.");
                if (!double.TryParse(expireDaysString, out double expireDays))
                {
                    throw new InvalidOperationException("JWT Expire Days 'Jwt:ExpireDays' configuration is not a valid number.");
                }
                var expires = DateTime.UtcNow.AddDays(expireDays);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: expires,
                    signingCredentials: credentials);

                var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

                Response.Cookies.Append("JwtToken", jwtToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Expires = expires,
                    SameSite = SameSiteMode.Lax
                });

                await _userManager.ResetAccessFailedCountAsync(user);

                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("LoginSuccess", currentCulture) ?? "";
                return LocalRedirect(returnUrl ?? "~/");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            Response.Cookies.Delete("JwtToken");
            await _signInManager.SignOutAsync();

            TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("LogoutSuccess", currentCulture) ?? "";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("PasswordChangeSuccess", currentCulture) ?? "";
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        [Authorize]
        [ResponseCache(NoStore = true, Duration = 0, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> ChangeEmail()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }
            var model = new ChangeEmailViewModel { NewEmail = currentUser.Email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return NotFound(); }

            if (model.NewEmail == user.Email)
            {
                TempData["InfoMessage"] = SharedResources.ResourceManager.GetString("NewEmailSameAsCurrent", currentCulture) ?? "";
                return View(model);
            }

            var existingUserWithNewEmail = await _userManager.FindByEmailAsync(model.NewEmail!);
            if (existingUserWithNewEmail != null && existingUserWithNewEmail.Id != user.Id)
            {
                ModelState.AddModelError(string.Empty, SharedResources.ResourceManager.GetString("EmailAlreadyUsed", currentCulture) ?? "");
                return View(model);
            }

            user.PendingNewEmail = model.NewEmail;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            var tokenForOldEmail = await _userManager.GenerateUserTokenAsync(user, Microsoft.AspNetCore.Identity.TokenOptions.DefaultProvider, "VerifyOldEmailForChange");
            var callbackUrl = Url.Action("ConfirmOldEmailChangeLink", "Account", new { userId = user.Id, token = tokenForOldEmail }, protocol: HttpContext.Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email!,
                SharedResources.ResourceManager.GetString("EmailChangeConfirmationSubjectStep1", currentCulture) ?? "",
                string.Format(SharedResources.ResourceManager.GetString("EmailChangeConfirmationBodyStep1", currentCulture) ?? "", user.UserName, HtmlEncoder.Default.Encode(callbackUrl!)));

            TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("ConfirmationLinkSentToCurrentEmail", currentCulture) ?? "";
            return RedirectToAction(nameof(CheckOldEmailInstructions));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmOldEmailChangeLink(string userId, string token)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            if (userId == null || token == null)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("InvalidLinkOrMissingInfoOldEmail", currentCulture) ?? "";
                return RedirectToAction(nameof(Manage));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.PendingNewEmail))
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("UserNotFoundOrEmailChangeNotInitiated", currentCulture) ?? "";
                return RedirectToAction(nameof(Manage));
            }

            var isTokenValid = await _userManager.VerifyUserTokenAsync(user, Microsoft.AspNetCore.Identity.TokenOptions.DefaultProvider, "VerifyOldEmailForChange", token);

            if (isTokenValid)
            {
                var tokenForNewEmail = await _userManager.GenerateChangeEmailTokenAsync(user, user.PendingNewEmail);
                var callbackUrlForNewEmail = Url.Action("ConfirmNewEmailForChange", "Account", new { userId = user.Id, email = user.PendingNewEmail, token = tokenForNewEmail }, protocol: HttpContext.Request.Scheme);

                await _emailSender.SendEmailAsync(user.PendingNewEmail!,
                    SharedResources.ResourceManager.GetString("EmailChangeConfirmationSubjectStep2", currentCulture) ?? "",
                    string.Format(SharedResources.ResourceManager.GetString("EmailChangeConfirmationBodyStep2", currentCulture) ?? "", user.UserName, HtmlEncoder.Default.Encode(callbackUrlForNewEmail!)));

                TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("ConfirmationLinkSentToNewEmail", currentCulture) ?? "";
                return RedirectToAction(nameof(CheckNewEmailInstructions));
            }
            else
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("InvalidOrExpiredLinkOldEmail", currentCulture) ?? "";
                return RedirectToAction(nameof(Manage));
            }
        }

        [HttpGet]
        [Authorize]
        public IActionResult CheckOldEmailInstructions()
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            TempData["SuccessMessage"] = TempData["SuccessMessage"] ?? (SharedResources.ResourceManager.GetString("ConfirmationLinkSentToCurrentEmailDefault", currentCulture) ?? "");
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult CheckNewEmailInstructions()
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            TempData["SuccessMessage"] = TempData["SuccessMessage"] ?? (SharedResources.ResourceManager.GetString("ConfirmationLinkSentToNewEmailDefault", currentCulture) ?? "");
            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult EmailConfirmationResult(bool isSuccess)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            ViewBag.IsSuccess = isSuccess;
            if (isSuccess)
            {
                ViewBag.Message = SharedResources.ResourceManager.GetString("EmailConfirmedAndUpdatedSuccess", currentCulture) ?? "";
            }
            else
            {
                ViewBag.Message = SharedResources.ResourceManager.GetString("EmailConfirmationFailedLinkInvalid", currentCulture) ?? "";
            }
            return View();
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ChangeUsername()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ChangeUsernameViewModel { NewUsername = user.UserName };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangeUsername(ChangeUsernameViewModel model)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            if (!ModelState.IsValid)
            {
                var errors = ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                );
                return Json(new { success = false, errors = errors });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = SharedResources.ResourceManager.GetString("UserNotFound", currentCulture) ?? "" });
            }

            if (model.NewUsername == user.UserName)
            {
                return Json(new { success = false, message = SharedResources.ResourceManager.GetString("NewUsernameSameAsCurrent", currentCulture) ?? "" });
            }

            var userWithSameUsername = await _userManager.FindByNameAsync(model.NewUsername ?? "");
            if (userWithSameUsername != null && userWithSameUsername.Id != user.Id)
            {
                return Json(new { success = false, message = SharedResources.ResourceManager.GetString("UsernameAlreadyUsed", currentCulture) ?? "" });
            }

            var setResult = await _userManager.SetUserNameAsync(user, model.NewUsername);
            if (!setResult.Succeeded)
            {
                foreach (var error in setResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("UsernameChangeSuccess", currentCulture) ?? "";
            return RedirectToAction(nameof(Manage));
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmNewEmailForChange(string userId, string email, string token)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            if (userId == null || email == null || token == null)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("InvalidLinkOrMissingInfoNewEmail", currentCulture) ?? "";
                return RedirectToAction(nameof(Manage));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("UserNotFound", currentCulture) ?? "";
                return RedirectToAction(nameof(Manage));
            }

            if (user.PendingNewEmail == null || !string.Equals(user.PendingNewEmail, email, StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("EmailChangeInfoMismatch", currentCulture) ?? "";
                return RedirectToAction(nameof(Manage));
            }

            var result = await _userManager.ChangeEmailAsync(user, email, token);
            if (result.Succeeded)
            {
                user.PendingNewEmail = null;
                await _userManager.UpdateAsync(user);
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("EmailConfirmedAndUpdatedSuccess", currentCulture) ?? "";
                return RedirectToAction(nameof(Manage));
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                TempData["ErrorMessage"] = string.Format(SharedResources.ResourceManager.GetString("NewEmailConfirmationFailed", currentCulture) ?? "", string.Join(", ", result.Errors.Select(e => e.Description)));
                return RedirectToAction(nameof(Manage));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ResendEmailConfirmation()
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("UserNotFound", currentCulture) ?? "";
                return RedirectToAction(nameof(Manage));
            }

            if (user.EmailConfirmed)
            {
                TempData["InfoMessage"] = SharedResources.ResourceManager.GetString("EmailAlreadyConfirmed", currentCulture) ?? "";
                return RedirectToAction(nameof(Manage));
            }
            if (string.IsNullOrEmpty(user.Email))
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("EmailNotDefined", currentCulture) ?? "";
                return RedirectToAction(nameof(Manage));
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, protocol: HttpContext.Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email!,
                SharedResources.ResourceManager.GetString("ConfirmYourEmailSubject", currentCulture) ?? "",
                string.Format(SharedResources.ResourceManager.GetString("ConfirmEmailBody", currentCulture) ?? "", HtmlEncoder.Default.Encode(callbackUrl!)));

            TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("EmailConfirmationLinkResent", currentCulture) ?? "";
            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            if (userId == null || token == null)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("InvalidLinkEmailConfirmation", currentCulture) ?? "";
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = SharedResources.ResourceManager.GetString("UserNotFound", currentCulture) ?? "";
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("EmailConfirmedSuccess", currentCulture) ?? "";
                return RedirectToAction(nameof(Manage));
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                TempData["ErrorMessage"] = string.Format(SharedResources.ResourceManager.GetString("EmailConfirmationFailed", currentCulture) ?? "", string.Join(", ", result.Errors.Select(e => e.Description)));
                return RedirectToAction(nameof(Manage));
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SetPhoneNumber()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return NotFound(); }

            var model = new SetPhoneNumberViewModel { PhoneNumber = user.PhoneNumber };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> SetPhoneNumber(SetPhoneNumberViewModel model)
        {
            var currentCulture = CultureInfo.CurrentUICulture;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) { return NotFound(); }

            var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
            if (!setPhoneResult.Succeeded)
            {
                foreach (var error in setPhoneResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
            await _signInManager.RefreshSignInAsync(user);
            TempData["SuccessMessage"] = SharedResources.ResourceManager.GetString("PhoneNumberUpdateSuccess", CultureInfo.CurrentUICulture) ?? "";
            return RedirectToAction(nameof(Manage));
        }
        public IActionResult Lockout() { return View(); }
    }
}
