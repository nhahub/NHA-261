using Microsoft.AspNetCore.Mvc.Rendering;
using STOCKUPMVC.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace STOCKUPMVC.ViewModels
{
    // Edit order (status & warehouse) view model
    public class SalesOrderEditViewModel
    {
        public int OrderID { get; set; }

        [Required]
        [Display(Name = "Warehouse")]
        public int WarehouseID { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; }

        public SelectList WarehouseList { get; set; }
        public SelectList StatusList { get; set; }

        public List<OrderItem> Items { get; set; } // optional: admin can adjust quantities if needed
    }

}
