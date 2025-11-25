using System.Collections.Generic;
using STOCKUPMVC.Models;

namespace STOCKUPMVC.ViewModels
{
    public class DashboardViewModel
    {
        public int ProductCount { get; set; }
        public int WarehouseCount { get; set; }
        public int PendingSalesOrderCount { get; set; }

        // ADD THESE MISSING PROPERTIES:
        public int PendingPurchaseOrderCount { get; set; }
        public List<PurchaseOrder> RecentPurchaseOrders { get; set; }

        // Your friend's existing property:
        public List<SalesOrder> RecentSalesOrders { get; set; }
    }
}