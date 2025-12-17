using System.ComponentModel.DataAnnotations;

namespace Moonmax.ViewModels
{
    public class CustomerFormViewModel
    {
        public int ClientID { get; set; } // nullable for Add

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        
        public string? Email { get; set; }
        public string? Phone { get; set; }

        [Required]
        public string PaymentType { get; set; }

        // ✅ NEW
        [Required]
        public string ClientType { get; set; } = "MainClient";
    }
}
