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
            if (!ModelState.IsValid || model.OrderItems.Count == 0)
            {
                ViewBag.Suppliers = new SelectList(await _unitOfWork.Suppliers.GetAllAsync(), "SupplierID", "Name");
                ViewBag.Warehouses = new SelectList(await _unitOfWork.Warehouses.GetAllAsync(), "WarehouseID", "Name");
                ViewBag.Categories = new SelectList(await _unitOfWork.Categories.GetAllAsync(), "CategoryID", "Name");

                return View(model);
            }

            var po = new PurchaseOrder
            {
                SupplierID = model.SupplierID,
                WarehouseID = model.WarehouseID,
                OrderTime = DateTime.Now,
                TotalAmount = model.TotalAmount,
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

            // Delete all related order items first
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

            var model = new PurchaseOrderEditViewModel
            {
                SupplierID = po.SupplierID,
                WarehouseID = po.WarehouseID,
                Status = po.Status,
                OrderItems = po.OrderItems.Select(oi => new PurchaseOrderItemViewModel
                {
                    ProductID = oi.ProductID,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    // Optional: you can include CategoryID if you track it in OrderItem
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PurchaseOrderEditViewModel model)
        {
            if (!ModelState.IsValid || model.OrderItems.Count == 0)
            {
                ViewBag.Suppliers = new SelectList(await _unitOfWork.Suppliers.GetAllAsync(), "SupplierID", "Name", model.SupplierID);
                ViewBag.Warehouses = new SelectList(await _unitOfWork.Warehouses.GetAllAsync(), "WarehouseID", "Name", model.WarehouseID);
                ViewBag.Categories = new SelectList(await _unitOfWork.Categories.GetAllAsync(), "CategoryID", "Name");
                ViewBag.Statuses = new SelectList(new List<string> { "Pending", "Approved", "Cancelled", "Completed" }, model.Status);
                return View(model);
            }

            var po = await _unitOfWork.PurchaseOrders
                        .GetAllQueryable()
                        .Include(p => p.OrderItems)
                        .FirstOrDefaultAsync(p => p.POID == id);

            if (po == null) return NotFound();

            // Check if status is changing to Completed
            bool markCompleted = po.Status != "Completed" && model.Status == "Completed";

            // Update main PO
            po.SupplierID = model.SupplierID;
            po.WarehouseID = model.WarehouseID;
            po.TotalAmount = model.TotalAmount;
            po.Status = model.Status;

            // Clear old items
            po.OrderItems.Clear();

            // Add updated items
            po.OrderItems = model.OrderItems.Select(i => new OrderItem
            {
                ProductID = i.ProductID,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                POID = po.POID
            }).ToList();

            await _unitOfWork.CompleteAsync();

            // ✅ Update Warehouse Stock if Completed
            if (markCompleted)
            {
                foreach (var item in po.OrderItems)
                {
                    // Check if Inventory exists for this Product + Warehouse
                    var inventory = (await _unitOfWork.Inventories
                        .FindAsync(inv => inv.ProductID == item.ProductID && inv.WarehouseID == po.WarehouseID))
                        .FirstOrDefault();

                    if (inventory != null)
                    {
                        inventory.Quantity += item.Quantity; // increase stock
                        _unitOfWork.Inventories.Update(inventory);
                    }
                    else
                    {
                        // Create new Inventory record
                        await _unitOfWork.Inventories.AddAsync(new Inventory
                        {
                            ProductID = item.ProductID,
                            WarehouseID = po.WarehouseID,
                            Quantity = item.Quantity
                        });
                    }
                }

                await _unitOfWork.CompleteAsync();
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
