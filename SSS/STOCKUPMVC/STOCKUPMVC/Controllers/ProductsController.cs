using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using STOCKUPMVC.Data.Repositories;
using STOCKUPMVC.Models;
using STOCKUPMVC.Models.ViewModels;

namespace STOCKUPMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private const int PageSize = 10;

        public ProductsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString, int? categoryId, int page = 1)
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

            // categories for filter
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var categoriesSelect = new SelectList(categories, "CategoryID", "Name", categoryId);

            var vm = new ProductListViewModel
            {
                Products = products,
                Categories = categoriesSelect,
                SearchString = searchString,
                CategoryId = categoryId,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize)
            };

            return View(vm);
        }

        // GET: Create
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create()
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var vm = new ProductFormViewModel
            {
                Categories = new SelectList(categories, "CategoryID", "Name")
            };

            return View(vm);
        }

        // POST: Create
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _unitOfWork.Categories.GetAllAsync();
                vm.Categories = new SelectList(categories, "CategoryID", "Name", vm.Product.CategoryID);
                return View(vm);
            }

            await _unitOfWork.Products.AddAsync(vm.Product);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null) return NotFound();

            var categories = await _unitOfWork.Categories.GetAllAsync();
            var vm = new ProductFormViewModel
            {
                Product = product,
                Categories = new SelectList(categories, "CategoryID", "Name", product.CategoryID)
            };

            return View(vm);
        }

        // POST: Edit
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _unitOfWork.Categories.GetAllAsync();
                vm.Categories = new SelectList(categories, "CategoryID", "Name", vm.Product.CategoryID);
                return View(vm);
            }

            _unitOfWork.Products.Update(vm.Product);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Details
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _unitOfWork.Products
                .GetAllQueryable()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null) return NotFound();

            var vm = new ProductDetailsViewModel { Product = product };
            return View(vm);
        }

        // GET: Delete
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _unitOfWork.Products
                .GetAllQueryable()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null) return NotFound();

            var vm = new ProductDetailsViewModel { Product = product };
            return View(vm);
        }

        // POST: DeleteConfirmed
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null) return NotFound();

            _unitOfWork.Products.Delete(product);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
