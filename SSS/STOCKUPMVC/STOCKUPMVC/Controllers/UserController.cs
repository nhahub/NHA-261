using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using STOCKUPMVC.Data;
using STOCKUPMVC.Models;
using STOCKUPMVC.ViewModels;

namespace STOCKUPMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly AppDbContext _context;

        public UserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // ---------------- LIST USERS ----------------
        public async Task<IActionResult> List()
        {
            ViewBag.Layout = "_AdminLayout";

            var users = await _userManager.Users
                .Include(u => u.Warehouse)
                .ToListAsync();

            var model = new List<UserListItemViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var primaryRole = roles.FirstOrDefault() ?? "-";

                model.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = primaryRole,
                    WarehouseName = user.Warehouse?.Name ?? "-"
                });
            }

            return View(model);
        }

        // ---------------- CREATE STAFF (GET) ----------------
        [HttpGet]
        public IActionResult CreateStaff()
        {
            ViewBag.Layout = "_AdminLayout";

            var vm = new CreateStaffViewModel
            {
                Warehouses = _context.Warehouses
                    .Select(w => new SelectListItem
                    {
                        Value = w.WarehouseID.ToString(),
                        Text = w.Name
                    })
                    .ToList()
            };

            return View(vm);
        }

        // ---------------- CREATE STAFF (POST) ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStaff(CreateStaffViewModel model)
        {
            ViewBag.Layout = "_AdminLayout";

            if (!ModelState.IsValid)
            {
                model.Warehouses = _context.Warehouses
                    .Select(w => new SelectListItem
                    {
                        Value = w.WarehouseID.ToString(),
                        Text = w.Name
                    })
                    .ToList();

                return View(model);
            }

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email is already in use.");
                model.Warehouses = _context.Warehouses
                    .Select(w => new SelectListItem
                    {
                        Value = w.WarehouseID.ToString(),
                        Text = w.Name
                    })
                    .ToList();

                return View(model);
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email,
                WarehouseID = model.WarehouseID
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                model.Warehouses = _context.Warehouses
                    .Select(w => new SelectListItem
                    {
                        Value = w.WarehouseID.ToString(),
                        Text = w.Name
                    })
                    .ToList();

                return View(model);
            }

            // Ensure Staff role exists
            if (!await _roleManager.RoleExistsAsync("Staff"))
            {
                await _roleManager.CreateAsync(new IdentityRole<int>("Staff"));
            }

            await _userManager.AddToRoleAsync(user, "Staff");

            return RedirectToAction(nameof(List));
        }

        // ---------------- DELETE STAFF ----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.Users
                .Include(u => u.Warehouse)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            // Admin can delete only Staff
            if (!roles.Contains("Staff"))
            {
                TempData["Error"] = "You can delete only Staff users.";
                return RedirectToAction(nameof(List));
            }

            // Prevent self-delete
            if (User.Identity?.Name == user.UserName)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(List));
            }

            var result = await _userManager.DeleteAsync(user);
            TempData[result.Succeeded ? "Success" : "Error"] =
                result.Succeeded ? "Staff user deleted." : "Failed to delete user.";

            return RedirectToAction(nameof(List));
        }
    }
}
