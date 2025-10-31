using System.ComponentModel.DataAnnotations.Schema;

namespace STOCKUPMVC.Models
{
    public class SupplierProduct
    {
        [ForeignKey("Supplier")]
        public int SupplierID { get; set; }

        [ForeignKey("Product")]
        public int ProductID { get; set; }

        public Supplier Supplier { get; set; }
        public Product Product { get; set; }
    }
}
