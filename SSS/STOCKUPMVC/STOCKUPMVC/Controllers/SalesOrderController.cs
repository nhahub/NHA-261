using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using STOCKUPMVC.Data.Repositories;
using STOCKUPMVC.Models;
using STOCKUPMVC.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace STOCKUPMVC.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class SalesOrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public SalesOrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: SalesOrder/List
        public async Task<IActionResult> List(int? customerId, int? warehouseId, string status)
        {
            var ordersQuery = _unitOfWork.SalesOrders.GetAllQueryable()
                .Include(o => o.Customer)
                .Include(o => o.Warehouse)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (customerId.HasValue)
                ordersQuery = ordersQuery.Where(o => o.CustomerID == customerId.Value);

            if (warehouseId.HasValue)
                ordersQuery = ordersQuery.Where(o => o.WarehouseID == warehouseId.Value);

            if (!string.IsNullOrEmpty(status))
                ordersQuery = ordersQuery.Where(o => o.Status == status);

            var orders = await ordersQuery.ToListAsync();

            var customers = await _unitOfWork.Customers.GetAllAsync();
            var warehouses = await _unitOfWork.Warehouses.GetAllAsync();

            var statuses = new[] { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };

            var vm = new SalesOrderListViewModel
            {
                Orders = orders,
                SelectedCustomerId = customerId,
                SelectedWarehouseId = warehouseId,
                SelectedStatus = status,
                Customers = new SelectList(customers, "CustomerID", "Name", customerId),
                Warehouses = new SelectList(warehouses, "WarehouseID", "Name", warehouseId),
                Statuses = new SelectList(statuses, status)
            };

            return View(vm);
        }

        // GET: SalesOrder/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _unitOfWork.SalesOrders.GetAllQueryable()
                .Include(o => o.Customer)
                .Include(o => o.Warehouse)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null) return NotFound();

            var warehouses = await _unitOfWork.Warehouses.GetAllAsync();
            var statuses = new[] { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };

            var vm = new SalesOrderDetailsViewModel
            {
                OrderID = order.OrderID,
                Customer = order.Customer,
                Warehouse = order.Warehouse,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.ToList(),
                WarehouseList = new SelectList(warehouses, "WarehouseID", "Name", order.WarehouseID),
                StatusList = new SelectList(statuses, order.Status)
            };

            return View(vm);
        }

        // GET: SalesOrder/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _unitOfWork.SalesOrders.GetAllQueryable()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Customer)
                .Include(o => o.Warehouse)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null) return NotFound();

            var warehouses = await _unitOfWork.Warehouses.GetAllAsync();
            var statuses = new[] { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };

            var vm = new SalesOrderEditViewModel
            {
                OrderID = order.OrderID,
                WarehouseID = order.WarehouseID,
                Status = order.Status,
                Items = order.OrderItems?.ToList() ?? new List<OrderItem>(),
                WarehouseList = new SelectList(warehouses, "WarehouseID", "Name", order.WarehouseID),
                StatusList = new SelectList(statuses, order.Status)
            };

            return View(vm);
        }

        // POST: SalesOrder/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SalesOrderEditViewModel vm)
        {
            Console.WriteLine($"Editing Sales Order {id}, New Status: {vm.Status}");

            if (id != vm.OrderID)
            {
                Console.WriteLine("ID mismatch");
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Model State Invalid");
                await RepopulateViewModel(vm);
                return View(vm);
            }

            try
            {
                // Get existing order with tracking
                var existingOrder = await _unitOfWork.SalesOrders.GetAllQueryable()
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderID == id);

                if (existingOrder == null)
                {
                    Console.WriteLine("Sales Order not found");
                    return NotFound();
                }

                // Store old status for inventory logic
                var oldStatus = existingOrder.Status;
                var newStatus = vm.Status;

                Console.WriteLine($"Current Status: {oldStatus}, New Status: {newStatus}");

                // Validate status transition
                if (!IsValidStatusTransition(oldStatus, newStatus))
                {
                    Console.WriteLine($"Invalid status transition: {oldStatus} -> {newStatus}");
                    ModelState.AddModelError("Status", $"Invalid status transition from {oldStatus} to {newStatus}.");
                    await RepopulateViewModel(vm);
                    return View(vm);
                }

                // Check inventory before confirming order
                if (oldStatus != "Confirmed" && newStatus == "Confirmed")
                {
                    Console.WriteLine("Checking inventory for confirmation");
                    var inventoryCheck = await CheckInventoryAvailability(existingOrder.OrderItems.ToList(), vm.WarehouseID);
                    if (!inventoryCheck.IsAvailable)
                    {
                        Console.WriteLine($"Inventory check failed: {inventoryCheck.ErrorMessage}");
                        ModelState.AddModelError("", inventoryCheck.ErrorMessage);
                        await RepopulateViewModel(vm);
                        return View(vm);
                    }
                }

                // Update order properties
                existingOrder.WarehouseID = vm.WarehouseID;
                existingOrder.Status = newStatus; // ← THIS IS THE KEY LINE
                existingOrder.TotalAmount = existingOrder.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);

                Console.WriteLine($"About to save - Sales Order Status will be: {existingOrder.Status}");

                // Handle inventory updates based on status changes
                await HandleInventoryUpdates(existingOrder, oldStatus, newStatus);

                _unitOfWork.SalesOrders.Update(existingOrder);
                var result = await _unitOfWork.CompleteAsync();

                Console.WriteLine($"Save completed. Result: {result}");

                if (result > 0)
                {
                    TempData["SuccessMessage"] = $"Order #{id} updated successfully! Status changed to {newStatus}";
                }
                else
                {
                    TempData["ErrorMessage"] = "No changes were made to the order.";
                }

                return RedirectToAction(nameof(List));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating order: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while updating the order. Please try again.");
                await RepopulateViewModel(vm);
                return View(vm);
            }
        }
        // Helper method to validate status transitions
        private bool IsValidStatusTransition(string oldStatus, string newStatus)
        {
            var allowedTransitions = new Dictionary<string, List<string>>
            {
                { "Pending", new List<string> { "Confirmed", "Cancelled" } },
                { "Confirmed", new List<string> { "Shipped", "Cancelled" } },
                { "Shipped", new List<string> { "Delivered" } },
                { "Delivered", new List<string> { } }, // No transitions from Delivered
                { "Cancelled", new List<string> { } }  // No transitions from Cancelled
            };

            return allowedTransitions.ContainsKey(oldStatus) &&
                   allowedTransitions[oldStatus].Contains(newStatus);
        }

        // Helper method to check inventory availability
        private async Task<(bool IsAvailable, string ErrorMessage)> CheckInventoryAvailability(List<OrderItem> orderItems, int warehouseId)
        {
            foreach (var item in orderItems)
            {
                var inventory = (await _unitOfWork.Inventories
                    .FindAsync(inv => inv.ProductID == item.ProductID && inv.WarehouseID == warehouseId))
                    .FirstOrDefault();

                var availableQuantity = inventory?.Quantity ?? 0;

                if (availableQuantity < item.Quantity)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductID);
                    return (false, $"Not enough inventory for {product?.Name}. Available: {availableQuantity}, Requested: {item.Quantity}");
                }
            }

            return (true, string.Empty);
        }

        // Helper method to handle inventory updates based on status changes
        private async Task HandleInventoryUpdates(SalesOrder order, string oldStatus, string newStatus)
        {
            // Pending → Confirmed: Decrease inventory (reserve stock)
            if (oldStatus != "Confirmed" && newStatus == "Confirmed")
            {
                await UpdateInventory(order.OrderItems.ToList(), order.WarehouseID, false); // Decrease
            }
            // Confirmed → Cancelled: Restore inventory
            else if (oldStatus == "Confirmed" && newStatus == "Cancelled")
            {
                await UpdateInventory(order.OrderItems.ToList(), order.WarehouseID, true); // Increase
            }
            // Shipped/Delivered: No inventory changes (already deducted at Confirmed)
            // Other transitions: No inventory changes
        }

        // Helper method to update inventory (increase or decrease)
        private async Task UpdateInventory(List<OrderItem> orderItems, int warehouseId, bool increase)
        {
            foreach (var item in orderItems)
            {
                // Find existing inventory record
                var inventory = (await _unitOfWork.Inventories
                    .FindAsync(inv => inv.ProductID == item.ProductID && inv.WarehouseID == warehouseId))
                    .FirstOrDefault();

                if (inventory != null)
                {
                    // Update existing inventory
                    if (increase)
                    {
                        inventory.Quantity += item.Quantity; // Restore stock
                    }
                    else
                    {
                        inventory.Quantity -= item.Quantity; // Reserve stock
                    }
                    _unitOfWork.Inventories.Update(inventory);
                }
                else if (!increase)
                {
                    // Create new inventory record with negative quantity (shouldn't happen if we checked availability)
                    await _unitOfWork.Inventories.AddAsync(new Inventory
                    {
                        ProductID = item.ProductID,
                        WarehouseID = warehouseId,
                        Quantity = -item.Quantity
                    });
                }
                // Note: If increasing but inventory doesn't exist, it means the product was removed - we skip
            }
        }

        // Helper method to repopulate view model data
        private async Task RepopulateViewModel(SalesOrderEditViewModel vm)
        {
            var warehouses = await _unitOfWork.Warehouses.GetAllAsync();
            var statuses = new[] { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };

            vm.WarehouseList = new SelectList(warehouses, "WarehouseID", "Name", vm.WarehouseID);
            vm.StatusList = new SelectList(statuses, vm.Status);

            if (vm.Items == null || !vm.Items.Any())
            {
                var orderWithItems = await _unitOfWork.SalesOrders.GetAllQueryable()
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.OrderID == vm.OrderID);
                vm.Items = orderWithItems?.OrderItems.ToList() ?? new List<OrderItem>();
            }
        }

        private async Task<bool> OrderExists(int id)
        {
            return await _unitOfWork.SalesOrders.GetByIdAsync(id) != null;
        }

        // GET: SalesOrder/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _unitOfWork.SalesOrders.GetAllQueryable()
                .Include(o => o.Customer)
                .Include(o => o.Warehouse)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // POST: SalesOrder/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _unitOfWork.SalesOrders.GetByIdAsync(id);
            if (order == null) return NotFound();

            // Only allow cancellation if order is not already delivered
            if (order.Status == "Delivered")
            {
                TempData["ErrorMessage"] = "Cannot cancel a delivered order.";
                return RedirectToAction(nameof(List));
            }

            // If order was confirmed, restore inventory before cancelling
            if (order.Status == "Confirmed")
            {
                var orderWithItems = await _unitOfWork.SalesOrders.GetAllQueryable()
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.OrderID == id);

                if (orderWithItems != null)
                {
                    await UpdateInventory(orderWithItems.OrderItems.ToList(), order.WarehouseID, true);
                }
            }

            order.Status = "Cancelled";
            _unitOfWork.SalesOrders.Update(order);
            await _unitOfWork.CompleteAsync();

            TempData["SuccessMessage"] = $"Order #{id} has been cancelled.";
            return RedirectToAction(nameof(List));
        }
    }
}