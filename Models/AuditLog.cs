using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moonmax.Models
{
    public class AuditLog
    {
        public int AuditID { get; set; }
        public int UserID { get; set; }            // User performing the action
        public string Action { get; set; }         // "Stock In", "Stock Out", "Created Job Order"
        public int? TargetID { get; set; }         // ID of affected record (PO, Invoice, JobOrder)
        public string Module { get; set; }         // "Inventory", "JobOrder", etc.
        public string Description { get; set; }    // Optional details
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("UserID")]
        public User? User { get; set; }
    }
}
