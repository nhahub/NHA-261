using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace STOCKUPMVC.Models
{
    public class PurchaseOrder
    {
        [Key]
        public int POID { get; set; }

        [ForeignKey("Supplier")]
        public int SupplierID { get; set; }

        [ForeignKey("Warehouse")]
        public int WarehouseID { get; set; }

        public DateTime OrderTime { get; set; }
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";

        public Supplier Supplier { get; set; }
        public Warehouse Warehouse { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}