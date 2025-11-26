using Microsoft.AspNetCore.Mvc.Rendering;
using STOCKUPMVC.Models;
using System;
using System.Collections.Generic;

namespace STOCKUPMVC.ViewModels
{
    // List view model for filtering and showing orders
    public class SalesOrderListViewModel
    {
        public IEnumerable<SalesOrder> Orders { get; set; }

        public int? SelectedCustomerId { get; set; }
        public int? SelectedWarehouseId { get; set; }
        public string SelectedStatus { get; set; }

        public SelectList Customers { get; set; }
        public SelectList Warehouses { get; set; }
        public SelectList Statuses { get; set; }
    }

}
