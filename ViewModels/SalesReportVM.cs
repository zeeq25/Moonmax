using System;
using System.Collections.Generic;

namespace Moonmax.ViewModels
{
    public class SalesReportVM
    {
        public List<InvoiceReportVM> InvoiceReport { get; set; }
        public List<string> Months { get; set; }
        public List<decimal> MonthlyRevenue { get; set; }
        public List<string> ServiceTypes { get; set; }
        public List<decimal> ServiceRevenue { get; set; }

        public decimal TotalRevenue { get; set; }
        public int InvoiceCount { get; set; }
        public decimal AvgInvoiceValue { get; set; }
        public string TopServiceRevenue { get; set; }
    }

}
