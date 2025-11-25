using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using STOCKUPMVC.Data.Repositories;
using STOCKUPMVC.Models;
using STOCKUPMVC.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace STOCKUPMVC.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class PurchaseOrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public PurchaseOrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: PurchaseOrder/List
        public async Task<IActionResult> List()
        {
            var orders = await _unitOfWork.PurchaseOrders
                .GetAllQueryable()
                .Include(p => p.Supplier)
                .Include(p => p.Warehouse)
                .Include(p => p.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();

            var viewModel = new PurchaseOrderListViewModel
            {
                PurchaseOrders = orders.Select(p => new PurchaseOrderListItemViewModel
                {
                    POID = p.POID,
                    SupplierName = p.Supplier?.Name,
                    WarehouseName = p.Warehouse?.Name,
                    Status = p.Status,
                    TotalAmount = p.TotalAmount,
                    OrderTime = p.OrderTime
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Suppliers = new SelectList(await _unitOfWork.Suppliers.GetAllAsync(), "SupplierID", "Name");
            ViewBag.Warehouses = new SelectList(await _unitOfWork.Warehouses.GetAllAsync(), "WarehouseID", "Name");
            ViewBag.Categories = new SelectList(await _unitOfWork.Categories.GetAllAsync(), "CategoryID", "Name");

            return View(new PurchaseOrderCreateVMM());
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseOrderCreateVMM model)
        {
            if (model.OrderItems == null || model.OrderItems.Count == 0 || !ModelState.IsValid)
            {
                ViewBag.Suppliers = new SelectList(await _unitOfWork.Suppliers.GetAllAsync(), "SupplierID", "Name");
                ViewBag.Warehouses = new SelectList(await _unitOfWork.Warehouses.GetAllAsync(), "WarehouseID", "Name");
                ViewBag.Categories = new SelectList(await _unitOfWork.Categories.GetAllAsync(), "CategoryID", "Name");
                return View(model);
            }

            var totalAmount = model.OrderItems.Sum(i => i.Quantity * i.UnitPrice);

            var po = new PurchaseOrder
            {
                SupplierID = model.SupplierID,
                WarehouseID = model.WarehouseID,
                OrderTime = DateTime.Now,
                TotalAmount = totalAmount,
                Status = "Pending",
                OrderItems = model.OrderItems.Select(i => new OrderItem
                {
                    ProductID = i.ProductID,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            await _unitOfWork.PurchaseOrders.AddAsync(po);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction("List");
        }

        // AJAX — Get Products by Category
        public async Task<JsonResult> GetProductsByCategory(int categoryId)
        {
            var products = (await _unitOfWork.Products.FindAsync(p => p.CategoryID == categoryId))
                            .Select(p => new
                            {
                                productID = p.ProductID,
                                name = p.Name,
                                price = p.Price
                            });

            return Json(products);
        }

        // GET: Delete Confirmation
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var po = await _unitOfWork.PurchaseOrders
                        .GetAllQueryable()
                        .Include(p => p.Supplier)
                        .Include(p => p.Warehouse)
                        .FirstOrDefaultAsync(p => p.POID == id);

            if (po == null) return NotFound();

            return View(po);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var po = await _unitOfWork.PurchaseOrders
                        .GetAllQueryable()
                        .Include(p => p.OrderItems)
                        .FirstOrDefaultAsync(p => p.POID == id);

            if (po == null) return NotFound();

            foreach (var item in po.OrderItems.ToList())
            {
                _unitOfWork.OrderItems.Delete(item);
            }

            _unitOfWork.PurchaseOrders.Delete(po);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction("List");
        }

        // GET: Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var po = await _unitOfWork.PurchaseOrders
                        .GetAllQueryable()
                        .Include(p => p.OrderItems)
                        .FirstOrDefaultAsync(p => p.POID == id);

            if (po == null) return NotFound();

            // ❌ Cancelled orders cannot be edited
            if (po.Status == "Cancelled")
            {
                TempData["Error"] = "Cancelled orders cannot be modified.";
                return RedirectToAction("List");
            }

            var model = new PurchaseOrderEditViewModel
            {
                POID = po.POID,
                SupplierID = po.SupplierID,
                WarehouseID = po.WarehouseID,
                Status = po.Status,
                TotalAmount = po.TotalAmount,
                OrderItems = po.OrderItems.Select(oi => new PurchaseOrderItemViewModel
                {
                    ProductID = oi.ProductID,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };

            ViewBag.Suppliers = new SelectList(await _unitOfWork.Suppliers.GetAllAsync(), "SupplierID", "Name", model.SupplierID);
            ViewBag.Warehouses = new SelectList(await _unitOfWork.Warehouses.GetAllAsync(), "WarehouseID", "Name", model.WarehouseID);
            ViewBag.Categories = new SelectList(await _unitOfWork.Categories.GetAllAsync(), "CategoryID", "Name");
            ViewBag.Statuses = new SelectList(
                new List<string> { "Pending", "Approved", "Cancelled", "Completed" },
                model.Status
            );

            return View(model);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PurchaseOrderEditViewModel model)
        {
            // Add debug logging
            Console.WriteLine($"Editing PO {id}, New Status: {model.Status}");

            if (model.OrderItems == null || model.OrderItems.Count == 0 || !ModelState.IsValid)
            {
                Console.WriteLine("Model State Invalid or No Order Items");
                // ... rest of your code
            }

            var po = await _unitOfWork.PurchaseOrders
                        .GetAllQueryable()
                        .Include(p => p.OrderItems)
                        .FirstOrDefaultAsync(p => p.POID == id);

            if (po == null)
            {
                Console.WriteLine("Purchase Order not found");
                return NotFound();
            }

            // Debug: Check current status
            Console.WriteLine($"Current Status: {po.Status}, New Status: {model.Status}");

            // ❌ Cancelled orders cannot be edited at all
            if (po.Status == "Cancelled")
            {
                TempData["Error"] = "Cancelled orders cannot be modified.";
                return RedirectToAction("List");
            }

            // ❌ Only Admin can mark Completed
            if (model.Status == "Completed" && !User.IsInRole("Admin"))
            {
                TempData["Error"] = "Only Admin can mark a Purchase Order as Completed.";
                return RedirectToAction("List");
            }

            // Determine if inventory update is needed
            bool markCompleted =
                po.Status != "Completed" &&
                po.Status != "Cancelled" &&
                model.Status == "Completed";

            var newTotalAmount = model.OrderItems.Sum(i => i.Quantity * i.UnitPrice);

            // Update PO - MAKE SURE THIS IS EXECUTING
            po.SupplierID = model.SupplierID;
            po.WarehouseID = model.WarehouseID;
            po.TotalAmount = newTotalAmount;
            po.Status = model.Status; // ← THIS IS THE KEY LINE

            Console.WriteLine($"About to save - PO Status will be: {po.Status}");

            // Replace items
            po.OrderItems.Clear();
            po.OrderItems = model.OrderItems.Select(i => new OrderItem
            {
                ProductID = i.ProductID,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                POID = po.POID
            }).ToList();

            try
            {
                var result = await _unitOfWork.CompleteAsync();
                Console.WriteLine($"Save completed. Result: {result}");

                // ➜ Update Inventory only when transitioning to Completed
                if (markCompleted)
                {
                    Console.WriteLine("Updating inventory for completed PO");
                    // ... your inventory update code
                    await _unitOfWork.CompleteAsync();
                }

                TempData["Success"] = "Purchase Order updated successfully!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving: {ex.Message}");
                TempData["Error"] = "Error updating purchase order.";
            }

            return RedirectToAction("List");
        }

        // GET: Details
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var po = await _unitOfWork.PurchaseOrders
                        .GetAllQueryable()
                        .Include(p => p.Supplier)
                        .Include(p => p.Warehouse)
                        .Include(p => p.OrderItems)
                        .ThenInclude(oi => oi.Product)
                        .FirstOrDefaultAsync(p => p.POID == id);

            if (po == null) return NotFound();

            var model = new PurchaseOrderDetailsViewModel
            {
                POID = po.POID,
                SupplierName = po.Supplier?.Name,
                WarehouseName = po.Warehouse?.Name,
                Status = po.Status,
                TotalAmount = po.TotalAmount,
                OrderTime = po.OrderTime,
                OrderItems = po.OrderItems.Select(oi => new PurchaseOrderDItemViewModel
                {
                    ProductID = oi.ProductID,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.Quantity * oi.UnitPrice
                }).ToList()
            };

            return View(model);
        }

    }
}
