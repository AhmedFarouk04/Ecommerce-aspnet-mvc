using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Identity;
using ECommerce.Web.Helpers;
using ECommerce.Web.Services;
using ECommerce.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Security.Claims;

namespace ECommerce.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly LoginSecurityService _loginSecurity;
        private readonly AdminActivityLogger _activityLogger;
        private readonly ImageProcessingService _imageService;
        private readonly EmailService _emailService;

        private readonly ICartService _cartService;
        private readonly SessionCartService _sessionCart;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            LoginSecurityService loginSecurity,
            AdminActivityLogger activityLogger,
            ImageProcessingService imageService,
            EmailService emailService,
            ICartService cartService,
            SessionCartService sessionCart)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _loginSecurity = loginSecurity;
            _activityLogger = activityLogger;
            _imageService = imageService;
            _emailService = emailService;
            _cartService = cartService;
            _sessionCart = sessionCart;
        }


        private string GetClientIp()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(forwarded))
                    return forwarded.Split(',')[0].Trim();
            }
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }


        public IActionResult Register() => View();

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckEmail(string email)
        {
            var exists = await _userManager.FindByEmailAsync(email);
            if (exists != null)
                return Json($"Email '{email}' is already registered.");

            return Json(true);
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> CheckUserName(string userName)
        {
            var exists = await _userManager.FindByNameAsync(userName);
            if (exists != null)
                return Json($"Username '{userName}' is already taken.");

            return Json(true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null)
            {
                if (existingEmail.EmailConfirmed)
                {
                    ModelState.AddModelError(nameof(model.Email), "Email is already registered.");
                    return View(model);
                }

                ModelState.AddModelError(nameof(model.Email), "Email is already registered. Please login or reset your password.");
                return View(model);
            }

            var existingUserName = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserName != null)
            {
                ModelState.AddModelError(nameof(model.UserName), "Username is already taken.");
                return View(model);
            }

            HttpContext.Session.SetString("Pending_UserName", model.UserName);
            HttpContext.Session.SetString("Pending_Email", model.Email);
            HttpContext.Session.SetString("Pending_Password", model.Password);
            HttpContext.Session.SetString("Verification_Purpose", "register");

            HttpContext.Session.Remove("Pending_TempImage");

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var tempFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "temp-users");
                Directory.CreateDirectory(tempFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(tempFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);

                HttpContext.Session.SetString("Pending_TempImage", fileName);
            }

            GenerateOtpAndSend(model.Email);

            return RedirectToAction("VerifyEmail");
        }

       

        private void GenerateOtpAndSend(string email)
        {
            var otp = new Random().Next(100000, 999999).ToString();

            HttpContext.Session.SetString("Pending_OTP", otp);
            HttpContext.Session.SetString("Pending_OTP_Expiration", DateTime.UtcNow.AddMinutes(5).ToString("O"));

            _emailService.SendEmailAsync(
                email,
                "Verification Code",
                $"<h2>Your verification code:</h2><h1>{otp}</h1><p>Valid for 5 minutes.</p>"
            );
        }

        [HttpGet]
        public IActionResult VerifyEmail()
        {
            var email = HttpContext.Session.GetString("Pending_Email");
            if (email == null)
                return RedirectToAction("Register");

            ViewBag.Email = email;
            return View(new VerifyEmailViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var code = HttpContext.Session.GetString("Pending_OTP");
            var expStr = HttpContext.Session.GetString("Pending_OTP_Expiration");

            if (code == null || expStr == null)
            {
                ModelState.AddModelError("", "Code expired. Please try again.");
                return View(model);
            }

            if (DateTime.UtcNow > DateTime.Parse(expStr))
            {
                ModelState.AddModelError("", "Code expired.");
                return View(model);
            }

            if (model.Code != code)
            {
                ModelState.AddModelError("", "Incorrect verification code.");
                return View(model);
            }

            var purpose = HttpContext.Session.GetString("Verification_Purpose");

            if (purpose == "register")
                return await CompleteRegister();

            if (purpose == "email-change")
                return await CompleteEmailUpdate();

            return BadRequest("Invalid verification flow.");
        }

        [HttpPost]
        public IActionResult ResendCode()
        {
            var email = HttpContext.Session.GetString("Pending_Email");
            if (email == null)
                return RedirectToAction("Register");

            GenerateOtpAndSend(email);

            TempData["Info"] = "New code sent!";
            return RedirectToAction("VerifyEmail");
        }

        

        private async Task<IActionResult> CompleteRegister()
        {
            var email = HttpContext.Session.GetString("Pending_Email");
            var username = HttpContext.Session.GetString("Pending_UserName");
            var password = HttpContext.Session.GetString("Pending_Password");
            var tempImg = HttpContext.Session.GetString("Pending_TempImage");

            var user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true
            };

            if (!string.IsNullOrEmpty(tempImg))
            {
                var tempFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "temp-users");
                var path = Path.Combine(tempFolder, tempImg);

                if (System.IO.File.Exists(path))
                {
                    using var fs = new FileStream(path, FileMode.Open);
                    IFormFile formFile = new FormFile(fs, 0, fs.Length, tempImg, tempImg);

                    (user.ImageUrl, user.ThumbnailUrl) =
                        await _imageService.ProcessImageAsync(formFile, "users");

                    fs.Close();
                    System.IO.File.Delete(path);
                }
            }
            else
            {
                user.ImageUrl = "default.png";
                user.ThumbnailUrl = "default.png";
            }

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);

                return View("VerifyEmail", new VerifyEmailViewModel());
            }

            await _userManager.AddToRoleAsync(user, "Customer");

            HttpContext.Session.Clear();
            return RedirectToAction("ConfirmEmail");
        }

       

        private async Task<IActionResult> CompleteEmailUpdate()
        {
            var userId = HttpContext.Session.GetString("Pending_UserId");
            var newEmail = HttpContext.Session.GetString("Pending_Email");

            var user = await _userManager.FindByIdAsync(userId);

            var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);

            var result = await _userManager.ChangeEmailAsync(user, newEmail, token);

            if (!result.Succeeded)
            {
                TempData["Error"] = "Could not update email.";
                return RedirectToAction("Profile");
            }

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);

            HttpContext.Session.Clear();

            TempData["Success"] = "Email updated successfully!";
            return RedirectToAction("Profile");
        }

        


       


        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string ip = GetClientIp();

            if (await _loginSecurity.IsIpBlockedAsync(ip))
            {
                ModelState.AddModelError("", "Too many attempts. Try later.");
                return View(model);
            }

            ApplicationUser user = model.LoginInput.Contains("@")
                ? await _userManager.FindByEmailAsync(model.LoginInput)
                : await _userManager.FindByNameAsync(model.LoginInput);

            if (user == null)
            {
                await _loginSecurity.RegisterFailedAttemptAsync(ip);
                ModelState.AddModelError("", "Invalid login.");
                return View(model);
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                HttpContext.Session.SetString("Pending_Email", user.Email);
                HttpContext.Session.SetString("Verification_Purpose", "register");

                GenerateOtpAndSend(user.Email);
                return RedirectToAction("VerifyEmail");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, true);

            if (!result.Succeeded)
            {
                await _loginSecurity.RegisterFailedAttemptAsync(ip);
                ModelState.AddModelError("", "Invalid login.");
                return View(model);
            }

            await _loginSecurity.ClearIpStateAsync(ip);

           
            var guestCart = _sessionCart.GetCart();

           
            await _signInManager.SignInWithClaimsAsync(
                user,
                model.RememberMe,
                new List<Claim>
                {
            new Claim("ImageUrl", user.ThumbnailUrl ?? "default.png")
                });

            
            if (guestCart.Any())
            {
                var guestItems = guestCart.Select(x => new CartItemDto
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    Price = x.Price
                }).ToList();

                _cartService.MergeGuestCart(user.Id, guestItems);

              
                _sessionCart.ClearCart();
            }

            return RedirectToAction("Index", "Home");
        }


        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            return View(new UserProfileViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                ImageUrl = user.ImageUrl,
                ThumbnailUrl = user.ThumbnailUrl,
                Roles = await _userManager.GetRolesAsync(user),

                OriginalUserName = user.UserName,
                OriginalEmail = user.Email,
                OriginalImage = user.ImageUrl
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            bool noChanges =
                model.UserName == model.OriginalUserName &&
                model.Email == model.OriginalEmail &&
                model.ImageFile == null;

            if (noChanges)
            {
                ModelState.AddModelError("", "No changes detected.");
                return View(model);
            }

            var existingUserName = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserName != null && existingUserName.Id != user.Id)
            {
                ModelState.AddModelError("UserName", "This username is already taken.");
                return View(model);
            }

            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null && existingEmail.Id != user.Id)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            if (model.Email != model.OriginalEmail)
            {
                var otp = new Random().Next(100000, 999999).ToString();

                HttpContext.Session.SetString("EmailChange_OTP", otp);
                HttpContext.Session.SetString("EmailChange_NewEmail", model.Email);
                HttpContext.Session.SetString("EmailChange_UserId", user.Id);

                await _emailService.SendEmailAsync(
                    model.Email,
                    "Confirm your new email",
                    $"<h1>{otp}</h1>"
                );

                TempData["Info"] = "Verification code sent to your new email.";
                return RedirectToAction("VerifyEmailChange");
            }

            user.UserName = model.UserName;

            if (model.ImageFile != null)
            {
                _imageService.DeleteImage(user.ImageUrl, "users");
                (user.ImageUrl, user.ThumbnailUrl) =
                    await _imageService.ProcessImageAsync(model.ImageFile, "users");
            }

            await _userManager.UpdateAsync(user);

            await _signInManager.SignInWithClaimsAsync(user, false,
                new List<Claim>
                {
            new Claim("ImageUrl", user.ThumbnailUrl ?? "default.png"),
            new Claim("UserName", user.UserName)
                });

            ViewBag.Message = "Profile updated successfully!";
            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyEmailChange()
        {
            var newEmail = HttpContext.Session.GetString("EmailChange_NewEmail");
            if (newEmail == null)
                return RedirectToAction("Profile");

            ViewBag.NewEmail = newEmail;
            return View(new VerifyEmailViewModel());
        }




        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (model.NewPassword == model.CurrentPassword)
            {
                ModelState.AddModelError("", "New password cannot be the same as the current password.");
                return View(model);
            }


            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);
                return View(model);
            }

            ViewBag.Message = "Password changed successfully!";
            return View(new ChangePasswordViewModel());
        }



        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordViewModel model,
            [FromServices] EmailService emailService)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return RedirectToAction("ForgotPasswordConfirmation");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var link = Url.Action("ResetPassword", "Account", new { email = model.Email, token }, Request.Scheme);

            await emailService.SendEmailAsync(
                model.Email,
                "Reset Password",
                $"Reset your password:<br><a href='{link}'>Click here</a>"
            );

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        public IActionResult ForgotPasswordConfirmation() => View();

        public IActionResult ResetPassword(string email, string token)
            => View(new ResetPasswordViewModel { Email = email, Token = token });

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return RedirectToAction("ResetPasswordConfirmation");

            var reset = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (!reset.Succeeded)
            {
                foreach (var e in reset.Errors)
                    ModelState.AddModelError("", e.Description);
                return View(model);
            }

            return RedirectToAction("ResetPasswordConfirmation");
        }

        public IActionResult ResetPasswordConfirmation() => View();

    }
}
