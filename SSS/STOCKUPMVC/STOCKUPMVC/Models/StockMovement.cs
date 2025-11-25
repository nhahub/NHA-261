using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace STOCKUPMVC.Models
{
    public class StockMovement
    {
        [Key]
        public int StockMovementID { get; set; }

        [ForeignKey("Product")]
        public int ProductID { get; set; }

        [ForeignKey("FromWarehouse")]
        public int FromWarehouseID { get; set; }

        [ForeignKey("ToWarehouse")]
        public int ToWarehouseID { get; set; }

        public int Quantity { get; set; }
        public DateTime MovementTime { get; set; } = DateTime.Now;

        public Product Product { get; set; }
        public Warehouse FromWarehouse { get; set; }
        public Warehouse ToWarehouse { get; set; }
    }
}