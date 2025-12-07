namespace Moonmax.ViewModels
{
    public class SalesIndexViewModel
    {
        public List<InvoiceViewModel> Invoices { get; set; }
        public List<JobOrderListingVM> InProgressJobs { get; set; } // jobs ready for invoicing
    }

}
