using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using STOCKUPMVC.Data.Repositories;
using STOCKUPMVC.Models;

namespace STOCKUPMVC.Controllers
{
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private const int PageSize = 10;

        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString, int? categoryId, int page = 1)
        {
            var productsQuery = _unitOfWork.Products
                .GetAllQueryable()
                .Include(p => p.Category)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(searchString) ||
                    (p.SKU != null && p.SKU.Contains(searchString)));
            }

            // CATEGORY FILTER
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                productsQuery = productsQuery.Where(p => p.CategoryID == categoryId.Value);
            }

            // PAGINATION
            int totalItems = await productsQuery.CountAsync();
            var products = await productsQuery
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // SEND CATEGORY LIST
            ViewBag.Categories = await _unitOfWork.Categories.GetAllAsync();

            ViewBag.SearchString = searchString;
            ViewBag.CategoryId = categoryId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            return View(products);
        }

        // GET: Create
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _unitOfWork.Categories.GetAllAsync();
            return View();
        }

        // POST: Create
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _unitOfWork.Categories.GetAllAsync();
                return View(product);
            }

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Edit
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = await _unitOfWork.Categories.GetAllAsync();
            return View(product);
        }

        // POST: Edit
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        public async Task<IActionResult> Edit(Product product)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _unitOfWork.Categories.GetAllAsync();
                return View(product);
            }

            _unitOfWork.Products.Update(product);
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

            return View(product);
        }

        // GET: Delete
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Delete
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost, ActionName("Delete")]
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
