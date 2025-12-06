using System.ComponentModel.DataAnnotations;

namespace Moonmax.ViewModels
{
    public class CreateSupplierViewModel
    {
        [Required(ErrorMessage = "Supplier name is required")]
        [MaxLength(100)]
        public string SupplierName { get; set; }

        [Required(ErrorMessage = "Contact number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Product line is required")]
        public string ProductLine { get; set; }

        [Required(ErrorMessage = "Payment terms is required")]
        public string PaymentTerms { get; set; }

        [Required]
        public string Status { get; set; }
    }
}
