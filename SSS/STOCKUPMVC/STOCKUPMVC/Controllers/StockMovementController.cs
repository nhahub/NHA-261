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
    public class StockMovementController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public StockMovementController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Index
        public async Task<IActionResult> Index()
        {
            var movements = await _unitOfWork.StockMovements
                .GetAllQueryable()
                .Include(sm => sm.Product)
                .Include(sm => sm.FromWarehouse)
                .Include(sm => sm.ToWarehouse)
                .ToListAsync();

            var viewModel = movements.Select(sm => new StockMovementListItemVMM
            {
                StockMovementID = sm.StockMovementID,
                ProductName = sm.Product?.Name,
                FromWarehouseName = sm.FromWarehouse?.Name,
                ToWarehouseName = sm.ToWarehouse?.Name,
                Quantity = sm.Quantity,
                MovementTime = sm.MovementTime
            }).ToList();

            return View(viewModel);
        }

        // GET: Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Products = new SelectList(await _unitOfWork.Products.GetAllAsync(), "ProductID", "Name");
            ViewBag.Warehouses = new SelectList(await _unitOfWork.Warehouses.GetAllAsync(), "WarehouseID", "Name");

            return View(new StockMovementCreateVMM());
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StockMovementCreateVMM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Products = new SelectList(await _unitOfWork.Products.GetAllAsync(), "ProductID", "Name", model.ProductID);
                ViewBag.Warehouses = new SelectList(await _unitOfWork.Warehouses.GetAllAsync(), "WarehouseID", "Name");
                return View(model);
            }

            // Check FromWarehouse inventory
            var inventoryFrom = await _unitOfWork.Inventories
                .GetAllQueryable()
                .FirstOrDefaultAsync(i => i.ProductID == model.ProductID && i.WarehouseID == model.FromWarehouseID);

            if (inventoryFrom == null || inventoryFrom.Quantity < model.Quantity)
            {
                ModelState.AddModelError("", "Not enough quantity in the From Warehouse!");
                ViewBag.Products = new SelectList(await _unitOfWork.Products.GetAllAsync(), "ProductID", "Name", model.ProductID);
                ViewBag.Warehouses = new SelectList(await _unitOfWork.Warehouses.GetAllAsync(), "WarehouseID", "Name");
                return View(model);
            }

            // Decrease from FromWarehouse
            inventoryFrom.Quantity -= model.Quantity;
            _unitOfWork.Inventories.Update(inventoryFrom);

            // Increase in ToWarehouse
            var inventoryTo = await _unitOfWork.Inventories
                .GetAllQueryable()
                .FirstOrDefaultAsync(i => i.ProductID == model.ProductID && i.WarehouseID == model.ToWarehouseID);

            if (inventoryTo == null)
            {
                inventoryTo = new Inventory
                {
                    ProductID = model.ProductID,
                    WarehouseID = model.ToWarehouseID,
                    Quantity = model.Quantity
                };
                await _unitOfWork.Inventories.AddAsync(inventoryTo);
            }
            else
            {
                inventoryTo.Quantity += model.Quantity;
                _unitOfWork.Inventories.Update(inventoryTo);
            }

            // Add StockMovement record
            var movement = new StockMovement
            {
                ProductID = model.ProductID,
                FromWarehouseID = model.FromWarehouseID,
                ToWarehouseID = model.ToWarehouseID,
                Quantity = model.Quantity
            };
            await _unitOfWork.StockMovements.AddAsync(movement);

            await _unitOfWork.CompleteAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
