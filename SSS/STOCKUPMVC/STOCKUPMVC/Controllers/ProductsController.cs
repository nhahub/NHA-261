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

        // GET: Products/WorkerIndex
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> WorkerIndex(string searchString, int? categoryId, int page = 1)
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
                // REPOPULATE CATEGORIES WHEN MODEL IS INVALID
                var categories = await _unitOfWork.Categories.GetAllAsync();
                vm.Categories = new SelectList(categories, "CategoryID", "Name", vm.Product.CategoryID);
                return View(vm);
            }

            // Handle image upload
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                vm.Product.ImageUrl = await SaveImageAsync(vm.ImageFile);
            }
            else
            {
                vm.Product.ImageUrl = "/images/placeholder.png";
            }

            await _unitOfWork.Products.AddAsync(vm.Product);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(WorkerIndex));
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

            // Handle image upload
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                // Get existing product to check current image
                var existingProduct = await _unitOfWork.Products.GetByIdAsync(vm.Product.ProductID);
                if (existingProduct != null && !string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    // Delete old image if it's not the placeholder
                    if (existingProduct.ImageUrl != "/images/placeholder.png")
                    {
                        DeleteImage(existingProduct.ImageUrl);
                    }
                }

                // Save new image
                vm.Product.ImageUrl = await SaveImageAsync(vm.ImageFile);
            }
            else
            {
                // Keep existing image URL - get it from database
                var existingProduct = await _unitOfWork.Products.GetByIdAsync(vm.Product.ProductID);
                if (existingProduct != null)
                {
                    vm.Product.ImageUrl = existingProduct.ImageUrl;
                }
            }

            _unitOfWork.Products.Update(vm.Product);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(WorkerIndex));
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null) return NotFound();

            // Delete associated image file if it exists and is not placeholder
            if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl != "/images/placeholder.png")
            {
                DeleteImage(product.ImageUrl);
            }

            _unitOfWork.Products.Delete(product);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(WorkerIndex));
        }

        // Helper methods for image handling
        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return "/images/placeholder.png";

            try
            {
                // Create unique filename
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var imagesFolder = Path.Combine("wwwroot", "images", "products");
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), imagesFolder, fileName);

                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                return $"/images/products/{fileName}";
            }
            catch (Exception ex)
            {
                // Log error and return placeholder
                Console.WriteLine($"Error saving image: {ex.Message}");
                return "/images/placeholder.png";
            }
        }

        private void DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || imageUrl == "/images/placeholder.png")
                return;

            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw - we don't want image deletion failure to break the main operation
                Console.WriteLine($"Error deleting image: {ex.Message}");
            }
        }
    }
}