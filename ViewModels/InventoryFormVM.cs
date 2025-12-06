using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Moonmax.ViewModels
{
    public class InventoryFormVM
    {
        public int InventoryID { get; set; }  // For edit

        [Required]
        [Display(Name = "Category")]
        public string Category { get; set; }
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();

        [Required]
        [Display(Name = "Part Name")]
        public string PartName { get; set; }

        [Required]
        [Range(typeof(decimal), "0", "999999")]
        [Display(Name = "Unit Cost")]
        public decimal UnitCost { get; set; }

        [Required]
        [Range(0, 999999)]
        [Display(Name = "Quantity In Stock")]
        public int QuantityInStock { get; set; }

        [Required(ErrorMessage = "Please select a supplier.")]
        [Display(Name = "Supplier")]
        public int SupplierID { get; set; }
        public List<SelectListItem> Suppliers { get; set; } = new List<SelectListItem>();
    }
}
