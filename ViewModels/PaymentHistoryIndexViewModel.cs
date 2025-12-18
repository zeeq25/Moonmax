using System.Collections.Generic;

namespace Moonmax.ViewModels
{
    public class PaymentHistoryIndexViewModel
    {
        public List<PaymentHistoryViewModel> Payments { get; set; } = new List<PaymentHistoryViewModel>();

        // KPIs
        public decimal TotalPaymentsReceived { get; set; }
        public decimal CashPayments { get; set; }
        public decimal CheckPayments { get; set; }
        public int TotalTransactions { get; set; }
    }
}