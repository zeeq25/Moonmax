using Microsoft.AspNetCore.Mvc.Rendering;

namespace Moonmax.ViewModels
{
    public class AddPartsViewModel
    {
        public int JobID { get; set; }
        public string JobClient { get; set; } = "";
        public string ServiceType { get; set; } = "";

        // For dropdown selection
        public int InventoryID { get; set; }
        public IEnumerable<SelectListItem> InventoryItems { get; set; } = new List<SelectListItem>();

        public int Quantity { get; set; }

        // List of parts already added to this job
        public List<JobPartListingVM> Parts { get; set; } = new List<JobPartListingVM>();
    }
}
