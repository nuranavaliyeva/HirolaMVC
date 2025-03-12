using HirolaMVC.Models;
using HirolaMVC.Services.Interfaces;
using HirolaMVC.Utilities.Enums;
using HirolaMVC.Utilities.Exceptions;
using HirolaMVC.ViewModels;
using HirolaMVC.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Stripe;

namespace HirolaMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager,IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM userVM)
        {

            if (!ModelState.IsValid)
            {
                return View();
            }
            AppUser user = new AppUser
            {
                Name = userVM.Name,
                Surname = userVM.Surname,
                Email = userVM.Email,
                UserName = userVM.UserName,

            };
            IdentityResult result = await _userManager.CreateAsync(user, userVM.Password);
            if (!result.Succeeded)
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }
            await _userManager.AddToRoleAsync(user, UserRole.Member.ToString());
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { token, Email = user.Email }, Request.Scheme);

            await _emailService.SendMailAsync(user.Email, "Email Confirmation", confirmationLink);
            await _userManager.ConfirmEmailAsync(user,token);
            //await _signInManager.SignInAsync(user, false);
            
            return RedirectToAction(nameof(SuccessfullyRegistered), "Account");
        }
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            AppUser user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound();
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return BadRequest();
            }
            await _signInManager.SignInAsync(user, isPersistent: false);
            return View();
        }
        public IActionResult SuccessfullyRegistered()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]

        public async Task<IActionResult> Login(LoginVM userVM, string? returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            AppUser user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == userVM.UsernameOrEmail || u.Email == userVM.UsernameOrEmail);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Username, Email or Password is incorrect");
                return View();

            }
            var result = await _signInManager.PasswordSignInAsync(user, userVM.Password, userVM.IsPersistent, true);
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Your account is locked bro, please try later.");
                return View();
            }
            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Please confirm your email.");
                return View();
            }
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Username, Email or Password is incorrect");
                return View();
            }



            if (returnUrl is null)
            {
                return RedirectToAction("Index", "Home");
            }

            return Redirect(returnUrl);


           
        }
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
        public async Task<IActionResult> CreateRoles()
        {
            foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
            {
                {
                    if (!await _roleManager.RoleExistsAsync(role.ToString()))
                        await _roleManager.CreateAsync(new IdentityRole { Name = role.ToString() });
                }
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM forgotPasswordVM)
        {
            if (!ModelState.IsValid) return View(forgotPasswordVM);
            var user = await _userManager.FindByEmailAsync(forgotPasswordVM.Email);
            if (user == null) return View(forgotPasswordVM);
            string token = await _userManager.GeneratePasswordResetTokenAsync(user);

            string link = Url.Action("ResetPassword","Account", new {userId=user.Id, token=token},HttpContext.Request.Scheme);
            await _emailService.SendMailAsync( forgotPasswordVM.Email,"ResetPassword", link,false);
            return RedirectToAction(nameof(Login));
        }
        public async Task<IActionResult> ResetPassword(string userId,string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token)) return BadRequest();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM resetPasswordVM, string userId,string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token)) return BadRequest();
            if(!ModelState.IsValid) return View(resetPasswordVM);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            var identityuser = await _userManager.ResetPasswordAsync(user, token, resetPasswordVM.ConfirmPassword);
            return RedirectToAction(nameof(Login));
        }
        public IActionResult ChangePassword()
        {
            return View();  
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM changePasswordVM)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            AppUser user = await _userManager.GetUserAsync(User);
            IdentityResult result = await _userManager.ChangePasswordAsync(user, changePasswordVM.CurrentPassword, changePasswordVM.NewPassword);
            if(!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Description);
                    return  View();
                }
            }
            await _userManager.UpdateAsync(user);
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }
    }
}

