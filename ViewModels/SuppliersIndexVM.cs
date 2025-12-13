namespace Moonmax.ViewModels
{
    public class SuppliersIndexVM
    {
        public List<SupplierViewModel> Suppliers { get; set; } = new();
        public int ActiveSuppliers { get; set; }
        public int PendingPOs { get; set; }
        public int DeliveredPOs { get; set; }
        public decimal TotalPOValue { get; set; }
    }
}
