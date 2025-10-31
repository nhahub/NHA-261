using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace STOCKUPMVC.Models
{
    public class StockMovement
    {
        [Key]
        public int MovementID { get; set; }

        [ForeignKey("Product")]
        public int ProductID { get; set; }

        [ForeignKey("FromWarehouse")]
        public int? FromWarehouseID { get; set; }

        [ForeignKey("ToWarehouse")]
        public int? ToWarehouseID { get; set; }

        [ForeignKey("CreatedBy")]
        public int CreatedById { get; set; }

        public int Quantity { get; set; }
        public string MovementType { get; set; } // e.g. "IN", "OUT", "TRANSFER"
        public DateTime CreatedAt { get; set; }

        public Product Product { get; set; }
        public Warehouse FromWarehouse { get; set; }
        public Warehouse ToWarehouse { get; set; }
        public ApplicationUser CreatedBy { get; set; }
    }
}
