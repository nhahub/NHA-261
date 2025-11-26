using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using STOCKUPMVC.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace STOCKUPMVC.Models.ViewModels
{
    public class InventoryEditViewModel
    {
        [Required]
        [Display(Name = "Product")]
        public int ProductID { get; set; }

        [Required]
        [Display(Name = "Warehouse")]
        public int WarehouseID { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be 0 or more")]
        public int Quantity { get; set; }
        [ValidateNever]
        public SelectList Products { get; set; }
        [ValidateNever]
        public SelectList Warehouses { get; set; }
    }
}