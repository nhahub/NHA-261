using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using STOCKUPMVC.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


namespace STOCKUPMVC.Models.ViewModels
{
    public class ProductFormViewModel
    {
        public ProductFormViewModel()
        {
            Product = new Product();
        }
        [ValidateNever]
        public Product Product { get; set; }
        [ValidateNever]
        public SelectList Categories { get; set; }
        [ValidateNever]
        public IFormFile ImageFile { get; set; }
    }
}