using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace STOCKUPMVC.ViewModels
{
    public class PurchaseOrderListItemViewModel
    {
        public int POID { get; set; }
        public string SupplierName { get; set; }
        public string WarehouseName { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderTime { get; set; }
    }

    public class PurchaseOrderListViewModel
    {
        public IEnumerable<PurchaseOrderListItemViewModel> PurchaseOrders { get; set; }

        // Filter properties
        public int? SelectedSupplierId { get; set; }
        public int? SelectedWarehouseId { get; set; }
        public string SelectedStatus { get; set; }

        // Dropdowns
        public IEnumerable<SelectListItem> Suppliers { get; set; }
        public IEnumerable<SelectListItem> Warehouses { get; set; }
    }
}