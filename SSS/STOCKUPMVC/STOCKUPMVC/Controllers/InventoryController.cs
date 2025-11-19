using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using STOCKUPMVC.Data.Repositories;
using STOCKUPMVC.Models;
using STOCKUPMVC.Models.ViewModels;
using System.Linq.Expressions;

namespace STOCKUPMVC.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class InventoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public InventoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Inventory/List
        // GET: Inventory/List
        public async Task<IActionResult> List(int? productId, int? warehouseId)
        {
            // Get filtered inventories
            var inventoriesQuery = _unitOfWork.Inventories
                .GetAllQueryable()
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .AsQueryable();

            if (productId.HasValue)
                inventoriesQuery = inventoriesQuery.Where(i => i.ProductID == productId.Value);

            if (warehouseId.HasValue)
                inventoriesQuery = inventoriesQuery.Where(i => i.WarehouseID == warehouseId.Value);

            var inventories = await inventoriesQuery.ToListAsync();

            // Prepare dropdowns
            var products = await _unitOfWork.Products.GetAllAsync();
            var warehouses = await _unitOfWork.Warehouses.GetAllAsync();

            var viewModel = new InventoryListViewModel
            {
                SelectedProductId = productId,
                SelectedWarehouseId = warehouseId,
                Inventories = inventories,
                Products = new SelectList(products, "ProductID", "Name", productId),
                Warehouses = new SelectList(warehouses, "WarehouseID", "Name", warehouseId)
            };

            return View(viewModel);
        }



        // GET: Inventory/Create
        public async Task<IActionResult> Create()
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            var warehouses = await _unitOfWork.Warehouses.GetAllAsync();

            var viewModel = new InventoryEditViewModel
            {
                Products = new SelectList(products, "ProductID", "Name"),
                Warehouses = new SelectList(warehouses, "WarehouseID", "Name")
            };

            return View(viewModel);
        }


        // POST: Inventory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // repopulate dropdowns
                var products = await _unitOfWork.Products.GetAllAsync();
                var warehouses = await _unitOfWork.Warehouses.GetAllAsync();
                vm.Products = new SelectList(products, "ProductID", "Name", vm.ProductID);
                vm.Warehouses = new SelectList(warehouses, "WarehouseID", "Name", vm.WarehouseID);
                return View(vm);
            }

            var inventory = new Inventory
            {
                ProductID = vm.ProductID,
                WarehouseID = vm.WarehouseID,
                Quantity = vm.Quantity
            };

            await _unitOfWork.Inventories.AddAsync(inventory);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(List));
        }


        // GET: Inventory/Edit/5
        public async Task<IActionResult> Edit(int productId, int warehouseId)
        {
            var inventory = await _unitOfWork.Inventories
                .GetAllQueryable()
                .FirstOrDefaultAsync(i => i.ProductID == productId && i.WarehouseID == warehouseId);

            if (inventory == null) return NotFound();

            var products = await _unitOfWork.Products.GetAllAsync();
            var warehouses = await _unitOfWork.Warehouses.GetAllAsync();

            var vm = new InventoryEditViewModel
            {
                ProductID = inventory.ProductID,
                WarehouseID = inventory.WarehouseID,
                Quantity = inventory.Quantity,
                Products = new SelectList(products, "ProductID", "Name", inventory.ProductID),
                Warehouses = new SelectList(warehouses, "WarehouseID", "Name", inventory.WarehouseID)
            };

            return View(vm);
        }


        // POST: Inventory/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InventoryEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var products = await _unitOfWork.Products.GetAllAsync();
                var warehouses = await _unitOfWork.Warehouses.GetAllAsync();
                vm.Products = new SelectList(products, "ProductID", "Name", vm.ProductID);
                vm.Warehouses = new SelectList(warehouses, "WarehouseID", "Name", vm.WarehouseID);
                return View(vm);
            }

            var inventory = await _unitOfWork.Inventories
                .GetAllQueryable()
                .FirstOrDefaultAsync(i => i.ProductID == vm.ProductID && i.WarehouseID == vm.WarehouseID);

            if (inventory == null) return NotFound();

            inventory.Quantity = vm.Quantity;

            _unitOfWork.Inventories.Update(inventory);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(List));
        }


        // GET: Inventory/Delete/5
        public async Task<IActionResult> Delete(int productId, int warehouseId)
        {
            var inventory = await _unitOfWork.Inventories
                .GetAllQueryable()
                .Include(i => i.Product)
                .Include(i => i.Warehouse)
                .FirstOrDefaultAsync(i => i.ProductID == productId && i.WarehouseID == warehouseId);

            if (inventory == null) return NotFound();

            var vm = new InventoryEditViewModel
            {
                ProductID = inventory.ProductID,
                WarehouseID = inventory.WarehouseID,
                Quantity = inventory.Quantity
            };

            return View(vm);
        }


        // POST: Inventory/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int productId, int warehouseId)
        {
            var inventory = await _unitOfWork.Inventories
                .GetAllQueryable()
                .FirstOrDefaultAsync(i => i.ProductID == productId && i.WarehouseID == warehouseId);

            if (inventory == null) return NotFound();

            _unitOfWork.Inventories.Delete(inventory);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(List));
        }


        // Helper to populate Product and Warehouse dropdowns
        private async Task PopulateDropDowns(int? selectedProduct = null, int? selectedWarehouse = null)
        {
            var products = await _unitOfWork.Products.GetAllAsync();
            var warehouses = await _unitOfWork.Warehouses.GetAllAsync();

            ViewBag.Products = new SelectList(products, "ProductID", "Name", selectedProduct);
            ViewBag.Warehouses = new SelectList(warehouses, "WarehouseID", "Name", selectedWarehouse);
        }
    }
}
