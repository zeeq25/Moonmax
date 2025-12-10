namespace Moonmax.ViewModels
{
    public class ReportsViewModel
    {
        // KPI Values
        public decimal TotalRevenue { get; set; }
        public decimal AvgJobValue { get; set; }
        public int TotalJobs { get; set; }
        public int TotalInventoryItems { get; set; }

        // Monthly Sales Trend (last 6 months)
        public List<string> SalesMonths { get; set; }
        public List<decimal> SalesValues { get; set; }

        // Service Type Distribution
        public List<string> ServiceLabels { get; set; }
        public List<int> ServiceCounts { get; set; }

        // Top Clients
        public List<string> TopClients { get; set; }
        public List<decimal> TopClientsRevenue { get; set; }

        // Inventory Turnover
        public List<string> Categories { get; set; }
        public List<int> CategoryTurnover { get; set; }
    }
}
