using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moonmax.Models
{
    [Table("Suppliers")]
    public class Supplier
    {
        [Key]
        public int SupplierID { get; set; }

        [Required, MaxLength(100)]
        public string SupplierName { get; set; }
        
        
        [MaxLength(100)]        
        public string ContactNumber { get; set; }

        [EmailAddress, MaxLength(150)]
        public string Email { get; set; }

        [MaxLength(100)]
        public string ProductLine { get; set; }

        [MaxLength(50)]
        public string PaymentTerms { get; set; } = "COD";// e.g., PDC, COD

        [Required]
        public string Status { get; set; } = "Active"; // Active / Inactive

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public ICollection<Inventory> Inventories { get; set; }
    }
}
