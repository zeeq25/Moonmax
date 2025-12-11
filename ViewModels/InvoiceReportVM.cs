namespace Moonmax.ViewModels
{
    public class InvoiceReportVM
    {
        public string InvoiceNumber { get; set; }
        public string ClientName { get; set; }
        public string ServiceType { get; set; }
        public decimal Amount { get; set; }
        public string PaymentType { get; set; }
        public DateTime DateIssued { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }


        // NEW
        public DateTime? PaymentReceivedDate { get; set; }
    }
}
