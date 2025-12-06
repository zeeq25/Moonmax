using System.Collections.Generic;

namespace Moonmax.ViewModels
{
    public class DashboardViewModel
    {
        // USERS
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }

        // JOB ORDERS (placeholders for now)
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

    }

    public class DashboardActivityItem
    {
        public string Title { get; set; }
        public string Meta { get; set; }
        public string TimeAgo { get; set; }
        public string Icon { get; set; }
        public string IconColor { get; set; }
    }
}
