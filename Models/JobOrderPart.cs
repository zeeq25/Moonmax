using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moonmax.Models
{
    public class JobOrderPart
    {
        [Key]
        public int JobOrderPartID { get; set; }

        
        public int JobID { get; set; }
        public JobOrder JobOrder { get; set; }

        
        public int InventoryID { get; set; }
        public Inventory? Inventory { get; set; }

        [Required]
        [MaxLength(100)]
        public string PartName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } // e.g., Electrical, Mechanical, etc.

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Cost { get; set; } // Cost per unit

        [NotMapped]
        public decimal LineCost => Quantity * Cost; // Total cost for this part
    }
}
