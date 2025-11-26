using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using STOCKUPMVC.Models;
using STOCKUPMVC.ViewModels;
using System.Threading.Tasks;

namespace STOCKUPMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ---------------- LOGIN ----------------
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // If already logged in, redirect based on role
                return RedirectToAppropriatePage();
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    // Redirect based on user role
                    return RedirectToAppropriatePage();
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password");
            return View(model);
        }

        // ---------------- REGISTER (Public Viewer) ----------------
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
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Default role for public registration is Viewer
                if (!await _userManager.IsInRoleAsync(user, "Viewer"))
                    await _userManager.AddToRoleAsync(user, "Viewer");

                // Sign in the user after registration
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // ---------------- LOGOUT ----------------
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ---------------- HELPER METHOD FOR ROLE-BASED REDIRECTION ----------------
        private IActionResult RedirectToAppropriatePage()
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("AdminView", "Home");
            else if (User.IsInRole("Staff"))
                return RedirectToAction("WorkerView", "Home");
            else
                return RedirectToAction("Index", "Home");
        }
    }
}