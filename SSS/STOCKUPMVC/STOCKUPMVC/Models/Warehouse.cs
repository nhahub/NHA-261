using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace STOCKUPMVC.Models
{
    public class Warehouse
    {
        [Key]
        public int WarehouseID { get; set; }

        [Required]
        public string Name { get; set; }

        public string Location { get; set; }

        public ICollection<Inventory> Inventories { get; set; }
        public ICollection<StockMovement> FromMovements { get; set; }
        public ICollection<StockMovement> ToMovements { get; set; }

        public ICollection<SalesOrder> SalesOrders { get; set; }
        public ICollection<PurchaseOrder> PurchaseOrders { get; set; }
    }
}
