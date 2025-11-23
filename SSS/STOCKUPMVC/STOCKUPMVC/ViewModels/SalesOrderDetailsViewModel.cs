using Microsoft.AspNetCore.Mvc.Rendering;
using STOCKUPMVC.Models;

namespace STOCKUPMVC.ViewModels
{
    // Details view model
    public class SalesOrderDetailsViewModel
    {
        public int OrderID { get; set; }
        public Customer Customer { get; set; }
        public Warehouse Warehouse { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItem> Items { get; set; }

        public SelectList WarehouseList { get; set; }
        public SelectList StatusList { get; set; }
    }
}
