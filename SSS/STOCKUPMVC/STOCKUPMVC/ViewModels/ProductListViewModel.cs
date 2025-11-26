using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using STOCKUPMVC.Models;

namespace STOCKUPMVC.Models.ViewModels
{
    public class ProductListViewModel
    {
        public IEnumerable<Product> Products { get; set; }

        // For filter dropdown
        public SelectList Categories { get; set; }

        // Filters
        public string? SearchString { get; set; }
        public int? CategoryId { get; set; }

        // Pagination
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
