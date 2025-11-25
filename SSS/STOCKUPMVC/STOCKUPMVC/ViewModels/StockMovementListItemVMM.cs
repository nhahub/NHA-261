namespace STOCKUPMVC.ViewModels
{
    public class StockMovementListItemVMM
    {
        public int StockMovementID { get; set; }
        public string ProductName { get; set; }
        public string FromWarehouseName { get; set; }
        public string ToWarehouseName { get; set; }
        public int Quantity { get; set; }
        public DateTime MovementTime { get; set; }
    }
}
