using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moonmax.Models
{

    [Table("StockMovements")]
    public class StockMovement
    {
        [Key]
        public int MovementID { get; set; }

        // What inventory item was affected
        [Required]
        public int InventoryID { get; set; }
        [ForeignKey("InventoryID")]
        public Inventory Inventory { get; set; }

        // Movement type: "IN" or "OUT"
        [Required]
        [MaxLength(10)]
        public string MovementType { get; set; }  // IN / OUT

        // How many items moved
        [Required]
        public int Quantity { get; set; }

        // Stock Before - Stock After
        public int PreviousQuantity { get; set; }
        public int NewQuantity { get; set; }

        // Optional: if from Procurement (PurchaseOrder)
        public int? PurchaseOrderID { get; set; }
        [ForeignKey("PurchaseOrderID")]
        public PurchaseOrders PurchaseOrder { get; set; }

        // Optional: if from Job Order consumption
        public int? JobOrderID { get; set; }
        [ForeignKey("JobOrderID")]
        public JobOrder JobOrder { get; set; }

        // Who performed the action
        public int? UserID { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }

        // Timestamp
        public DateTime MovementDate { get; set; } = DateTime.Now;
    }
}
