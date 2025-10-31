using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace STOCKUPMVC.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FullName { get; set; }
        public int? WarehouseID { get; set; }

        // Navigation
        public Warehouse Warehouse { get; set; }
        public ICollection<StockMovement> CreatedStockMovements { get; set; }
    }
}
