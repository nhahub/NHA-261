using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using STOCKUPMVC.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace STOCKUPMVC.ViewModels
{
    public class SalesOrderEditViewModel
    {
        public int OrderID { get; set; }

        [Required]
        public int WarehouseID { get; set; }

        [Required]
        public string Status { get; set; }

        [ValidateNever]
        public SelectList WarehouseList { get; set; }

        [ValidateNever]
        public SelectList StatusList { get; set; }

        [ValidateNever]
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}