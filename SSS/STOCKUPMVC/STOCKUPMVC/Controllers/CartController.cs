using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using STOCKUPMVC.Models;
using STOCKUPMVC.Models.ViewModels;
using STOCKUPMVC.Data.Repositories;
using System.Transactions;

namespace STOCKUPMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private const string CartSessionKey = "ShoppingCart";

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: /Cart
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // POST: /Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var cart = GetCart();
            var existingItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItemVM
                {
                    ProductId = product.ProductID,
                    ProductName = product.Name,
                    SKU = product.SKU,
                    Price = product.Price,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl ?? "/images/placeholder.png"
                });
            }

            SaveCart(cart);
            TempData["Success"] = $"{product.Name} added to cart!";
            return RedirectToAction("Index", "Products");
        }

        // POST: /Cart/RemoveFromCart
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                cart.Items.Remove(item);
                SaveCart(cart);
                TempData["Success"] = $"{item.ProductName} removed from cart!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                return RemoveFromCart(productId);
            }

            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.Quantity = quantity;
                SaveCart(cart);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Cart/ClearCart
        [HttpPost]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
            TempData["Success"] = "Cart cleared!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Cart/Checkout
        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (!cart.Items.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CartTotal = cart.Total;
            var checkoutVM = new CheckoutVM();
            return View(checkoutVM);
        }

        // POST: /Cart/Checkout
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutVM model)
        {
            var cart = GetCart();
            if (!cart.Items.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CartTotal = cart.Total;
                return View(model);
            }

            try
            {
                // Verify all products still exist and are valid
                foreach (var item in cart.Items)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    if (product == null)
                    {
                        TempData["Error"] = $"Product '{item.ProductName}' is no longer available.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                // Get or create default warehouse
                var warehouses = await _unitOfWork.Warehouses.GetAllAsync();
                int warehouseId;
                
                if (!warehouses.Any())
                {
                    // Create default warehouse if none exists
                    var defaultWarehouse = new Warehouse 
                    { 
                        Name = "Main Warehouse", 
                        Location = "Default Location" 
                    };
                    await _unitOfWork.Warehouses.AddAsync(defaultWarehouse);
                    await _unitOfWork.CompleteAsync();
                    warehouseId = defaultWarehouse.WarehouseID;
                }
                else
                {
                    warehouseId = warehouses.First().WarehouseID;
                }

                // Create customer
                var customer = new Customer
                {
                    Name = model.CustomerName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Address = model.Address
                };

                await _unitOfWork.Customers.AddAsync(customer);
                await _unitOfWork.CompleteAsync();

                // Create sales order
                var salesOrder = new SalesOrder
                {
                    CustomerID = customer.CustomerID,
                    WarehouseID = warehouseId,
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    TotalAmount = cart.Total,
                    OrderItems = cart.Items.Select(item => new OrderItem
                    {
                        ProductID = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    }).ToList()
                };

                await _unitOfWork.SalesOrders.AddAsync(salesOrder);
                await _unitOfWork.CompleteAsync();

                // Clear cart
                HttpContext.Session.Remove(CartSessionKey);

                TempData["Success"] = $"Order #{salesOrder.OrderID} placed successfully!";
                return RedirectToAction("OrderConfirmation", new { orderId = salesOrder.OrderID });
            }
            catch (DbUpdateException dbEx)
            {
                // Log the specific database error
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                System.Diagnostics.Debug.WriteLine($"DATABASE ERROR: {innerMessage}");
                
                TempData["Error"] = "A database error occurred while processing your order. Please try again.";
                ViewBag.CartTotal = cart.Total;
                return View(model);
            }
            catch (Exception ex)
            {
                // Log general errors
                System.Diagnostics.Debug.WriteLine($"CHECKOUT ERROR: {ex.Message}");
                TempData["Error"] = "An unexpected error occurred while processing your order. Please try again.";
                ViewBag.CartTotal = cart.Total;
                return View(model);
            }
        }

        // GET: /Cart/OrderConfirmation
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            try
            {
                var order = await _unitOfWork.SalesOrders
                    .GetAllQueryable()
                    .Include(so => so.Customer)
                    .Include(so => so.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(so => so.OrderID == orderId);

                if (order == null)
                {
                    TempData["Error"] = "Order not found.";
                    return RedirectToAction("Index", "Home");
                }

                return View(order);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ORDER CONFIRMATION ERROR: {ex.Message}");
                TempData["Error"] = "Error loading order confirmation.";
                return RedirectToAction("Index", "Home");
            }
        }

        // Helper methods for session management
        private ShoppingCartVM GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            return cartJson == null ? new ShoppingCartVM() : JsonConvert.DeserializeObject<ShoppingCartVM>(cartJson);
        }

        private void SaveCart(ShoppingCartVM cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonConvert.SerializeObject(cart));
        }
    }
}