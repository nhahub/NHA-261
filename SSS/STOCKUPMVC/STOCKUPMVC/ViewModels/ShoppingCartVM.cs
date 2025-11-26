using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace STOCKUPMVC.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public List<CartItemVM> Items { get; set; } = new List<CartItemVM>();
        public decimal Total => Items.Sum(item => item.Subtotal);
        public int TotalItems => Items.Sum(item => item.Quantity);
    }

    public class CartItemVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => Price * Quantity;
        public string ImageUrl { get; set; }
    }

    public class CheckoutVM
    {
        [Required(ErrorMessage = "Name is required")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        public string Notes { get; set; }
    }
}