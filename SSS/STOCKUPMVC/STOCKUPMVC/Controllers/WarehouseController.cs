using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using STOCKUPMVC.Data;
using STOCKUPMVC.Models;

namespace STOCKUPMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class WarehouseController : Controller
    {
        private readonly AppDbContext _context;

        public WarehouseController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Warehouse
        public async Task<IActionResult> Index()
        {
            var warehouses = await _context.Warehouses.ToListAsync();
            return View(warehouses);
        }

        // GET: Warehouse/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Warehouse/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Warehouse warehouse)
        {
            if (!ModelState.IsValid)
                return View(warehouse);

            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Warehouse/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null) return NotFound();
            return View(warehouse);
        }

        // POST: Warehouse/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Warehouse warehouse)
        {
            if (!ModelState.IsValid)
                return View(warehouse);

            _context.Warehouses.Update(warehouse);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Warehouse/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var warehouse = await _context.Warehouses
                .Include(w => w.Inventories)
                .Include(w => w.PurchaseOrders)
                .Include(w => w.SalesOrders)
                .Include(w => w.FromMovements)
                .Include(w => w.ToMovements)
                .FirstOrDefaultAsync(w => w.WarehouseID == id);

            if (warehouse == null)
                return NotFound();

            // Check if warehouse contains linked data
            if (warehouse.Inventories.Any() ||
                warehouse.SalesOrders.Any() ||
                warehouse.PurchaseOrders.Any() ||
                warehouse.FromMovements.Any() ||
                warehouse.ToMovements.Any())
            {
                TempData["Error"] = "Cannot delete this warehouse because it contains products, orders, or stock movements.";
                return RedirectToAction("Index");
            }

            return View(warehouse);
        }

        // POST: Warehouse/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var warehouse = await _context.Warehouses
                .Include(w => w.Inventories)
                .Include(w => w.SalesOrders)
                .Include(w => w.PurchaseOrders)
                .Include(w => w.FromMovements)
                .Include(w => w.ToMovements)
                .FirstOrDefaultAsync(w => w.WarehouseID == id);

            if (warehouse == null)
                return NotFound();

            // Block deletion if warehouse contains relationships
            if (warehouse.Inventories.Any() ||
                warehouse.SalesOrders.Any() ||
                warehouse.PurchaseOrders.Any() ||
                warehouse.FromMovements.Any() ||
                warehouse.ToMovements.Any())
            {
                TempData["Error"] = "Cannot delete warehouse that contains products or order history.";
                return RedirectToAction("Index");
            }

            _context.Warehouses.Remove(warehouse);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Warehouse deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
