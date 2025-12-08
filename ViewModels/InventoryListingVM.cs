namespace Moonmax.ViewModels
{
    public class InventoryListingVM
    {
        public int InventoryID { get; set; }
        public string Category { get; set; }
        public string PartName { get; set; }
        public decimal UnitCost { get; set; }
        public int QuantityInStock { get; set; }
        public string SupplierName { get; set; }


        public int ReorderLevel { get; set; }


    }

}

