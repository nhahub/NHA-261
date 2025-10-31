using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace STOCKUPMVC.Models
{
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
