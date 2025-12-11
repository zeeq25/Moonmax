using System;
using System.Collections.Generic;

namespace Moonmax.ViewModels
{
    public class JobOrderReportVM
    {
        // Job Order List
        public List<JobOrderRecordVM> JobOrderList { get; set; } = new List<JobOrderRecordVM>();

        // KPIs
        public int TotalJobs { get; set; }
        public int CompletedJobs { get; set; }
        public int PendingJobs { get; set; }
        public decimal JobRevenue { get; set; }

        // Filters
        public List<string> FiltersStatuses { get; set; } = new List<string>();
        public List<string> FiltersServiceTypes { get; set; } = new List<string>();

        // Charts
        public List<string> ChartStatuses { get; set; } = new List<string>();
        public List<int> ChartCounts { get; set; } = new List<int>();
        public List<string> ChartServiceTypes { get; set; } = new List<string>();
        public List<int> ChartServiceCounts { get; set; } = new List<int>();
        public List<string> ChartDates { get; set; } = new List<string>();
        public List<int> ChartDateCounts { get; set; } = new List<int>();
    }
}
