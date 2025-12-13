namespace Moonmax.ViewModels
{
    public class SalesIndexViewModel
    {
        public List<InvoiceViewModel> Invoices { get; set; }
        public List<JobOrderListingVM> InProgressJobs { get; set; } // jobs ready for invoicing
        


        // KPI properties
        public decimal TotalSales { get; set; }
        public decimal PaidInvoices { get; set; }
        public decimal PendingPDCs { get; set; }
        public int ActiveCustomers { get; set; }
    }

}
