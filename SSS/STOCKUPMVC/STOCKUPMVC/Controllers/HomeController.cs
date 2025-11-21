using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using STOCKUPMVC.Data.Repositories;
using STOCKUPMVC.Models;
using System.Linq;
using System.Threading.Tasks;

namespace STOCKUPMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(user);

                // Admin/Staff users go to AdminView
                if (roles.Contains("Admin") || roles.Contains("Staff"))
                    return View("AdminView");
            }

            // All others (public or viewer) see UserView
            return View("UserView");
        }

        // ADD THESE MISSING ACTIONS:

        [Authorize(Roles = "Admin")]
        public IActionResult AdminView()
        {
            return View();
        }

        [Authorize(Roles = "Staff,Admin")]
        public IActionResult WorkerView()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult UserView()
        {
            return View();
        }

        // Add Privacy action if needed
        public IActionResult Privacy()
        {
            return View();
        }
    }
}