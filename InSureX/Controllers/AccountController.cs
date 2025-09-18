using InSureX.Models;
using InSureX.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InSureX.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuditService _auditService;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            AuditService auditService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _auditService = auditService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt");
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, password, false, false);

            if (result.Succeeded)
            {
                // Last Login Tracking
                user.LastLoginTime = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Audit log for login
                await _auditService.LogAsync("Login", "ApplicationUser", null, user.Id, User);

                // Role-based redirection
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
                    return RedirectToAction("AdminDashboard", "Admin");
                if (roles.Any(r => r.Equals("CPD", StringComparison.OrdinalIgnoreCase)))
                    return RedirectToAction("CPDDashboard", "CPD");
                if (roles.Any(r => r.Equals("Employee", StringComparison.OrdinalIgnoreCase)))
                    return RedirectToAction("EmployeeDashboard", "Employee");

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid login attempt");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                await _auditService.LogAsync("Logout", "ApplicationUser", null, user.Id, User);
            }

            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
