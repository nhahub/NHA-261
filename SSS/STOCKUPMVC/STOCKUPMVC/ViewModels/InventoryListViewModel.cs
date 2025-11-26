using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace STOCKUPMVC.Models.ViewModels
{
    public class InventoryListViewModel
    {
        public int? SelectedProductId { get; set; }
        public int? SelectedWarehouseId { get; set; }

        public IEnumerable<Inventory> Inventories { get; set; }

        public SelectList Products { get; set; }
        public SelectList Warehouses { get; set; }
    }
}
