using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moonmax.Models
{   

    [Table("Inventories")]
    public class Inventory
    {
        [Key]
        public int InventoryID { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        public string PartName { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        [Required]
        public int QuantityInStock { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Foreign Key
        public int SupplierID { get; set; }

        // Navigation property
        [ForeignKey("SupplierID")]
        public Supplier Supplier { get; set; }
    }
}
