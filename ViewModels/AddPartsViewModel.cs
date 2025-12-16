using Microsoft.AspNetCore.Mvc.Rendering;

namespace Moonmax.ViewModels
{
    public class AddPartsViewModel
    {
        public int JobID { get; set; }
        public string JobClient { get; set; } = "";
        public string ServiceType { get; set; } = "";
        public string JobStatus { get; set; } = "Pending"; // ⬅️ THIS WAS MISSING!

        // NEW: Category selection
        public string? SelectedCategory { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        // For dropdown selection
        public int InventoryID { get; set; }
        public IEnumerable<SelectListItem> InventoryItems { get; set; } = new List<SelectListItem>();
        public int Quantity { get; set; }
        // List of parts already added to this job
        public List<JobPartListingVM> Parts { get; set; } = new List<JobPartListingVM>();
    }
}
