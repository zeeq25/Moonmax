namespace Moonmax.ViewModels
{
    public class ReceiptViewModel
    {
        // Identity
        public int PaymentID { get; set; }

        // Invoice / Client
        public string InvoiceNumber { get; set; }
        public string ClientName { get; set; }
        public int JobID { get; set; }

        // Amounts
        public decimal AmountPaid { get; set; }

        // Dates
        public DateTime PaymentDate { get; set; }
        public DateTime DateIssued { get; set; }

        // Payment
        public string PaymentMethod { get; set; }
        public string ReferenceNumber { get; set; }

        // Check details
        public string CheckNumber { get; set; }
        public string BankName { get; set; }
        public DateTime? CheckDate { get; set; }

        // Meta
        public string ProcessedBy { get; set; }
        public string Notes { get; set; }
    }
}
