namespace Moonmax.ViewModels
{
    public class InvoiceViewModel
    {
        public int InvoiceID { get; set; }
        public string InvoiceNumber { get; set; }
        public string ClientName { get; set; }


        public string ContactNumber { get; set; } // ⬅️ ADD HERE


        public int JobID { get; set; }
        public decimal Amount { get; set; }
        public string PaymentType { get; set; }

        public DateTime DateIssued { get; set; }
        public DateTime? DueDate { get; set; }

        public string Status { get; set; }
        
    }
}

