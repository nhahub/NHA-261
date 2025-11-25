using System.ComponentModel.DataAnnotations;

namespace STOCKUPMVC.ViewModels
{
    public class StockMovementCreateVMM
    {
        [Required]
        public int ProductID { get; set; }

        [Required]
        public int FromWarehouseID { get; set; }

        [Required]
        public int ToWarehouseID { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
