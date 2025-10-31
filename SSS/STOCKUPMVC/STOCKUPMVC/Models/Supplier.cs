using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace STOCKUPMVC.Models
{
    public class Supplier
    {
        [Key]
        public int SupplierID { get; set; }

        [Required]
        public string Name { get; set; }

        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        public ICollection<PurchaseOrder> PurchaseOrders { get; set; }
        public ICollection<SupplierProduct> SupplierProducts { get; set; }
    }
}
