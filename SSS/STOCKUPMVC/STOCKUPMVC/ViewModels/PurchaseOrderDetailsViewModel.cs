using System;
using System.Collections.Generic;

namespace STOCKUPMVC.ViewModels
{ 
public class PurchaseOrderDetailsViewModel
{
    public int POID { get; set; }
    public string SupplierName { get; set; }
    public string WarehouseName { get; set; }
    public string Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderTime { get; set; }

    public List<PurchaseOrderDItemViewModel> OrderItems { get; set; } = new();
}

public class PurchaseOrderDItemViewModel
{
    public int ProductID { get; set; }
    public string ProductName { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
}