namespace Moonmax.ViewModels
{
    public class PurchaseOrderDetailsVM
    {
        public int PurchaseOrderID { get; set; }
        public string SupplierName { get; set; }
        public string Status { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? ExpectedDelivery { get; set; }
        public decimal TotalAmount { get; set; }
        public List<POItemVM> Items { get; set; } = new();
    }

    public class POItemVM
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
