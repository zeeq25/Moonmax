using System;
using System.ComponentModel.DataAnnotations;

namespace Moonmax.ViewModels
{
    public class PaymentFormViewModel
    {
        [Required(ErrorMessage = "Invoice ID is required")]
        public int InvoiceID { get; set; }

        [Required(ErrorMessage = "Payment method is required")]
        public string PaymentMethod { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal AmountPaid { get; set; }

        [Required(ErrorMessage = "Payment date is required")]
        public DateTime PaymentDate { get; set; }

        // PDC-specific fields (optional)
        public string? CheckNumber { get; set; }
        public string? BankName { get; set; }
        public DateTime? CheckDate { get; set; }

        // ✅ FIXED: Made these nullable so they're not required
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }
}