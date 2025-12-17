namespace Moonmax.ViewModels
{
    public class JobPartListingVM
    {
        public int PartID { get; set; }       // PK in JobParts table
        public int InventoryID { get; set; }     // FK to Inventory
        public string PartName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }

        // Make TotalCost assignable if controller sets it
        public decimal TotalCost { get; set; }


    }
}
