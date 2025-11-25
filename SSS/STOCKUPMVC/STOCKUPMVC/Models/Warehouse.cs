using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace STOCKUPMVC.Models
{
    public class Warehouse
    {
        [Key]
        public int WarehouseID { get; set; }

        [Required]
        public string Name { get; set; }

        public string Location { get; set; }

        [ValidateNever]
        public ICollection<Inventory> Inventories { get; set; }

        [ValidateNever]
        public ICollection<StockMovement> FromMovements { get; set; }

        [ValidateNever]
        public ICollection<StockMovement> ToMovements { get; set; }

        [ValidateNever]
        public ICollection<SalesOrder> SalesOrders { get; set; }

        [ValidateNever]
        public ICollection<PurchaseOrder> PurchaseOrders { get; set; }
    }
}
