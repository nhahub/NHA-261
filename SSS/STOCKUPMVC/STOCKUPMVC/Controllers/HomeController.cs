using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using STOCKUPMVC.Data.Repositories;
using STOCKUPMVC.Models;
using STOCKUPMVC.Models.ViewModels;
using STOCKUPMVC.ViewModels;
using System;
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

        // ===============================
        //           INDEX REDIRECT
        // ===============================
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                    return RedirectToAction(nameof(AdminDashboard));

                if (roles.Contains("Staff"))
                    return RedirectToAction(nameof(WorkerDashboard));
            }

            // Public user = product list
            var productVm = await GetProductListViewModel(null, null, 1);
            return View("UserView", productVm);
        }

        // ===============================
        //         ADMIN DASHBOARD
        // ===============================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var vm = new DashboardViewModel
            {
                ProductCount = await _unitOfWork.Products.CountAsync(),
                WarehouseCount = await _unitOfWork.Warehouses.CountAsync(),
                PendingSalesOrderCount = await _unitOfWork.SalesOrders.CountAsync(s => s.Status == "Pending"),
                PendingPurchaseOrderCount = await _unitOfWork.PurchaseOrders.CountAsync(p => p.Status == "Pending"),
                RecentSalesOrders = await _unitOfWork.SalesOrders
                    .GetAllQueryable()
                    .Include(so => so.Customer)
                    .OrderByDescending(s => s.OrderDate)
                    .Take(5)
                    .ToListAsync(),
                RecentPurchaseOrders = await _unitOfWork.PurchaseOrders
                    .GetAllQueryable()
                    .Include(po => po.Supplier)
                    .OrderByDescending(p => p.OrderTime)
                    .Take(5)
                    .ToListAsync()
            };

            // IMPORTANT: explicitly use Dashboard.cshtml
            return View("Dashboard", vm);
        }

        // ===============================
        //         WORKER DASHBOARD
        // ===============================
        [Authorize(Roles = "Staff")]
        public IActionResult WorkerDashboard()
        {
            // IMPORTANT: use your WorkerView.cshtml as Worker Dashboard
            return View("WorkerView");
        }

        // ===============================
        //     PRODUCT LIST FOR ADMIN
        // ===============================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminView(string searchString, int? categoryId, int page = 1)
        {
            var vm = await GetProductListViewModel(searchString, categoryId, page);
            return View(vm);
        }

        // ===============================
        //  PRODUCT LIST FOR STAFF (VIEW ONLY)
        // ===============================
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> WorkerView(string searchString, int? categoryId, int page = 1)
        {
            var vm = await GetProductListViewModel(searchString, categoryId, page);
            return View(vm);
        }

        // ===============================
        //  PRODUCT LIST FOR PUBLIC
        // ===============================
        [AllowAnonymous]
        public async Task<IActionResult> UserView(string searchString, int? categoryId, int page = 1)
        {
            var vm = await GetProductListViewModel(searchString, categoryId, page);
            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // ===============================
        //     REUSABLE PRODUCT LIST
        // ===============================
        private async Task<ProductListViewModel> GetProductListViewModel(string searchString, int? categoryId, int page)
        {
            var query = _unitOfWork.Products
                .GetAllQueryable()
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(p =>
                    p.Name.Contains(searchString) ||
                    (p.SKU != null && p.SKU.Contains(searchString)));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryID == categoryId.Value);
            }

            var totalItems = await query.CountAsync();
            var products = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

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
