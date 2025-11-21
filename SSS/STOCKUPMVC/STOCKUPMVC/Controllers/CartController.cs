using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using STOCKUPMVC.Models;
using STOCKUPMVC.Models.ViewModels;
using STOCKUPMVC.Data.Repositories;

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

            // Create customer using your existing Customer model
            var customer = new Customer
            {
                Name = model.CustomerName,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address
            };

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.CompleteAsync();

            // Create sales order using your existing SalesOrder model
            var salesOrder = new SalesOrder
            {
                CustomerID = customer.CustomerID,
                WarehouseID = 1, // Default warehouse
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

        // GET: /Cart/OrderConfirmation
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var order = await _unitOfWork.SalesOrders
                .GetAllQueryable()
                .Include(so => so.Customer)
                .Include(so => so.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(so => so.OrderID == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
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
