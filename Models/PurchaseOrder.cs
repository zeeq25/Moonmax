// Models/PurchaseOrder.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moonmax.Models
{
    [Table("PurchaseOrder")]
    public class PurchaseOrders
    {
        [Key]
        public int PurchaseOrderID { get; set; }

        [Required]
        public int SupplierID { get; set; }

        public Supplier Supplier { get; set; }

        public DateTime DateCreated { get; set; }

        [Required]
        public DateTime ExpectedDelivery { get; set; }

        public string Status { get; set; }

        public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }
}

