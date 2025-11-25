using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace STOCKUPMVC.ViewModels
{
    public class PurchaseOrderItemViewModel
    {
        public int CategoryID { get; set; }
        public int ProductID { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal TotalPrice => Quantity * UnitPrice;
    }

    public class PurchaseOrderEditViewModel
    {
        [Required]
        public int SupplierID { get; set; }
        public int POID { get; set; }
        [Required]
        public string Status { get; set; }

        [Required]
        public int WarehouseID { get; set; }

        //public DateTime OrderTime { get; set; } = DateTime.Now;

        public List<PurchaseOrderItemViewModel> OrderItems { get; set; } = new();

        public decimal TotalAmount
        {
            get
            {
                decimal total = 0;
                foreach (var item in OrderItems)
                    total += item.TotalPrice;
                return total;
            }
        }
    }
}
