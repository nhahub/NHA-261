using System.Collections.Generic;

namespace STOCKUPMVC.ViewModels
{
    public class UserListItemViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string WarehouseName { get; set; }
    }
}