using System.Collections.Generic;

namespace Moonmax.ViewModels
{
    public class DashboardViewModel
    {
        // USERS
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }

        // JOB ORDERS 
        public int ActiveRepairs { get; set; }
        public int CompletedJobsMonth { get; set; }

        // INVENTORY
        public int TotalInventoryStock { get; set; }

        // SALES
        public decimal MonthlySales { get; set; }

        // ACTIVITY FEED
        public List<string> RecentActivity { get; set; } = new();

        // TODAY'S SUMMARY
        public int NewWorkOrders { get; set; }
        public int CompletedJobsToday { get; set; }
        public int InvoicesSent { get; set; }
        public decimal PendingPayments { get; set; }

        // CHARTS
        public List<string> WorkOrderServices { get; set; } = new(); // Service names
        public List<int> WorkOrderCounts { get; set; } = new();       // Counts per service

        public List<string> WeeklyLabels { get; set; } = new();       // Days of week
        public List<int> WeeklyOutput { get; set; } = new();          // Jobs completed

        public List<string> InventoryItems { get; set; } = new();     // Inventory names
        public List<int> InventoryValues { get; set; } = new();       // Inventory quantity/value

        public List<LowStockItem> LowStockItems { get; set; }

    }

    public class DashboardActivityItem
    {
        public string Title { get; set; }
        public string Meta { get; set; }
        public string TimeAgo { get; set; }
        public string Icon { get; set; }
        public string IconColor { get; set; }
    }

    public class LowStockItem
    {
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public int CriticalLevel { get; set; }
        public int ReorderLevel { get; set; }
    }
}
