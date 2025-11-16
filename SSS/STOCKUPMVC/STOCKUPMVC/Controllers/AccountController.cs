using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using STOCKUPMVC.Models;
using STOCKUPMVC.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;


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
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Login", model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    user, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                    return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password");
            return View("Login", model);
        }


        // ---------------- REGISTER ----------------
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated == false)
                return RedirectToAction("Login");

            PopulateRoles();   // نجهز الـ dropdown
            return View("Register");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                PopulateRoles();
                return View("Register", model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // لو مفيش Role مختار خليه Viewer
                var role = string.IsNullOrWhiteSpace(model.Role) ? "Viewer" : model.Role;

                await _userManager.AddToRoleAsync(user, role);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            PopulateRoles();
            return View("Register", model);
        }

        // ---------------- LOGOUT ----------------
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        // --------------- HELPER -----------------
        private void PopulateRoles()
        {
            ViewBag.Roles = new List<SelectListItem>
    {
        new SelectListItem { Value = "Staff",  Text = "Staff"  },
        new SelectListItem { Value = "Viewer", Text = "Viewer" }
        // متعمّد ما أضفتش Admin هنا عشان مينفعش أي حد يعمل Admin بسهولة
    };
        }
    }
}
