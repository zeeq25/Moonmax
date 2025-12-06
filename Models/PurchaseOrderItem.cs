// Models/PurchaseOrderItem.cs
using System.ComponentModel.DataAnnotations;

namespace Moonmax.Models
{
    public class PurchaseOrderItem
    {
        [Key]
        public int PurchaseOrderItemID { get; set; }

        [Required]
        public int PurchaseOrderID { get; set; }

        public PurchaseOrders PurchaseOrder { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public string Category { get; set; }    
    }
}
