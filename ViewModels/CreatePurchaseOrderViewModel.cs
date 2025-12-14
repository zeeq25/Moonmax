// ViewModels/CreatePurchaseOrderViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Moonmax.Models;

namespace Moonmax.ViewModels
{
    public class CreatePurchaseOrderViewModel
    {
        [Required(ErrorMessage ="Supplier is required.")]
        public int SupplierID { get; set; }

        [Required(ErrorMessage = "Parts/Items required.")]
        public List<PurchaseOrderItem> Items { get; set; } = new();

        [Required]
        [DataType(DataType.Date)]
        public DateTime ExpectedDelivery { get; set; }

        public string Status { get; set; } = "Pending";

        // For dropdowns
        public IEnumerable<Supplier> Suppliers { get; set; } = new List<Supplier>();

        // Categiries for dropdowns
        public IEnumerable<string> Categories { get; set; } = new List<string>
    {
        "Hoses",
        "Fittings",
        "Lubricants",
        "Tools",
        "Supplies",
        "Others"
    };


    }

    public class PurchaseOrderItem
    {
        [Required]
        public string ProductName { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public string Category { get; set; }    

        public decimal Total => Quantity * UnitPrice;
    }
}
