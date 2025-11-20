using STOCKUPMVC.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace STOCKUPMVC.Models
{
    public class Product
    {
        public Product()
        {
            Inventories = new List<Inventory>();
            OrderItems = new List<OrderItem>();
            SupplierProducts = new List<SupplierProduct>();
            StockMovements = new List<StockMovement>();
        }

        [Key]
        public int ProductID { get; set; }

        [Required]
        public string Name { get; set; }

        public string SKU { get; set; }
        public string Description { get; set; }

        [ForeignKey("Category")]
        public int CategoryID { get; set; }

        public decimal Price { get; set; }

        // Navigation properties
        public Category Category { get; set; }
        public ICollection<Inventory> Inventories { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<SupplierProduct> SupplierProducts { get; set; }
        public ICollection<StockMovement> StockMovements { get; set; }
    }
}
