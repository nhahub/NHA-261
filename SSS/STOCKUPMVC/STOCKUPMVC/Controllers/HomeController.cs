using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using STOCKUPMVC.Data.Repositories;
using STOCKUPMVC.Models;
using STOCKUPMVC.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace STOCKUPMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private const int PageSize = 10;

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
                    return RedirectToAction("AdminView");
            }

            // All others (public or viewer) see UserView
            return RedirectToAction("UserView");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminView(string searchString, int? categoryId, int page = 1)
        {
            // Load products for admin view
            var vm = await GetProductListViewModel(searchString, categoryId, page);
            return View(vm);
        }

        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> WorkerView(string searchString, int? categoryId, int page = 1)
        {
            // Load products for worker view
            var vm = await GetProductListViewModel(searchString, categoryId, page);
            return View(vm);
        }

        [AllowAnonymous]
        public async Task<IActionResult> UserView(string searchString, int? categoryId, int page = 1)
        {
            // Load products for customer view
            var vm = await GetProductListViewModel(searchString, categoryId, page);
            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Helper method to load products (same logic as ProductsController)
        private async Task<ProductListViewModel> GetProductListViewModel(string searchString, int? categoryId, int page)
        {
            var query = _unitOfWork.Products
                .GetAllQueryable()
                .Include(p => p.Category)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(p =>
                    p.Name.Contains(searchString) ||
                    (p.SKU != null && p.SKU.Contains(searchString)));
            }

            // Category filter
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryID == categoryId.Value);
            }

            // Pagination
            var totalItems = await query.CountAsync();
            var products = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Categories for filter
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var categoriesSelect = new SelectList(categories, "CategoryID", "Name", categoryId);

            return new ProductListViewModel
            {
                Products = products,
                Categories = categoriesSelect,
                SearchString = searchString,
                CategoryId = categoryId,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize)
            };
        }
    }
}