using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace STOCKUPMVC.ViewModels
{
    public class PurchaseOrderCreateVM
    {
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public int SupplierID { get; set; }
        public List<OrderItemVM> Items { get; set; } = new();


        public IEnumerable<SelectListItem> Suppliers { get; set; }
        public IEnumerable<CategoryWithProductsVM> Categories { get; set; }
    }


    public class OrderItemVM
    {
        public int CategoryID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }


    public class CategoryWithProductsVM
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public List<ProductVM> Products { get; set; }
    }


    public class ProductVM
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
