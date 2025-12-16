using System;
using System.Collections.Generic;

namespace Moonmax.ViewModels
{
    public class InventoryReportVM
    {
        public List<InventoryItemVM> Inventories { get; set; } = new List<InventoryItemVM>();
        public decimal TotalStockValue { get; set; }
        public int LowStockCount { get; set; }
        public int TotalStockAdded { get; set; }
        public int TotalStockUsed { get; set; }

        // For chart
        public List<string> UsageDates { get; set; } = new List<string>();
        public List<int> UsageQuantities { get; set; } = new List<int>();

        public class InventoryItemVM
        {
            public int InventoryID { get; set; }
            public string Category { get; set; }
            public string PartName { get; set; }
            public decimal UnitCost { get; set; }
            public int QuantityInStock { get; set; }
            public int ReorderLevel { get; set; }
            public string SupplierName { get; set; }
            // public DateTime CreatedAt { get; set; }

            public DateTime? UpdatedAt {get; set;}
        }
    }
}
