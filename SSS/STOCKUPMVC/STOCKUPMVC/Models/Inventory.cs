using System.ComponentModel.DataAnnotations.Schema;

namespace STOCKUPMVC.Models
{
    public class Inventory
    {
        [ForeignKey("Product")]
        public int ProductID { get; set; }

        [ForeignKey("Warehouse")]
        public int WarehouseID { get; set; }

        public int Quantity { get; set; }

        public Product Product { get; set; }
        public Warehouse Warehouse { get; set; }
    }
}
