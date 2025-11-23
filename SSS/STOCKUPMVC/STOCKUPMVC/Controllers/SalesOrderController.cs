using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using STOCKUPMVC.Data.Repositories;
using STOCKUPMVC.Models;
using STOCKUPMVC.ViewModels;
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
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null) return NotFound();

            var warehouses = await _unitOfWork.Warehouses.GetAllAsync();
            var statuses = new[] { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };

            var vm = new SalesOrderEditViewModel
            {
                OrderID = order.OrderID,
                WarehouseID = order.WarehouseID,
                Status = order.Status,
                Items = order.OrderItems.ToList(),
                WarehouseList = new SelectList(warehouses, "WarehouseID", "Name", order.WarehouseID),
                StatusList = new SelectList(statuses, order.Status)
            };

            return View(vm);
        }

        // POST: SalesOrder/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SalesOrderEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var warehouses = await _unitOfWork.Warehouses.GetAllAsync();
                var statuses = new[] { "Pending", "Confirmed", "Shipped", "Delivered", "Cancelled" };
                vm.WarehouseList = new SelectList(warehouses, "WarehouseID", "Name", vm.WarehouseID);
                vm.StatusList = new SelectList(statuses, vm.Status);
                return View(vm);
            }

            var order = await _unitOfWork.SalesOrders.GetByIdAsync(vm.OrderID);
            if (order == null) return NotFound();

            order.WarehouseID = vm.WarehouseID;
            order.Status = vm.Status;

            // Optional: update quantities if Items changed
            // foreach(var item in vm.Items) { ... }

            _unitOfWork.SalesOrders.Update(order);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(List));
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

            // Soft delete: mark as Cancelled
            order.Status = "Cancelled";
            _unitOfWork.SalesOrders.Update(order);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(List));
        }
    }
}
