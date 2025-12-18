using System;

namespace Moonmax.ViewModels
{
    public class PaymentHistoryViewModel
    {
        public int PaymentID { get; set; }

        public int InvoiceID { get; set; }
        public string InvoiceNumber { get; set; }
        public string ClientName { get; set; }  // Added for view
        public int? JobID { get; set; }         // Added for view
        public string PaymentMethod { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }

        public string ReferenceNumber { get; set; }

        // PDC Fields
        public string CheckNumber { get; set; }
        public string BankName { get; set; }
        public DateTime? CheckDate { get; set; }  // Added for view
        public string PDCStatus { get; set; }

        // Tracking Users
        public string ProcessedBy { get; set; }
        public string DepositedBy { get; set; }
        public string ClearedBy { get; set; }

        public string Notes { get; set; }
    }
}
