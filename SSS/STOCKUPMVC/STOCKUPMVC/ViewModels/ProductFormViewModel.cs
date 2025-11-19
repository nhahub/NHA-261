using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using STOCKUPMVC.Models;

namespace STOCKUPMVC.Models.ViewModels
{
    public class ProductFormViewModel
    {
        public ProductFormViewModel()
        {
            Product = new Product();
        }

        public Product Product { get; set; }

        public SelectList Categories { get; set; }
    }
}
