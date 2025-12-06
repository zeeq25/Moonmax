using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moonmax.Models
{
    [Table("JobParts")]
    public class JobPart
    {
        [Key]
        public int PartID { get; set; }

        [Required]
        public int JobID { get; set; }
        [ForeignKey("JobID")]
        public JobOrder JobOrder { get; set; }

        [Required]
        public int InventoryID { get; set; }  // FK to Inventories
        [ForeignKey("InventoryID")]
        public Inventory Inventory { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitCost { get; set; }

        public decimal TotalCost => Quantity * UnitCost;
    }
}
