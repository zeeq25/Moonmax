namespace Moonmax.ViewModels
{
    public class CustomerViewModel
    {
        public int ClientID { get; set; }
        public string Name { get; set; }          // Customer name
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PaymentType { get; set; }   // Cash, PDC-15 DAYS, etc.
        public int TotalTransactions { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal Outstanding { get; set; }
    }
}
