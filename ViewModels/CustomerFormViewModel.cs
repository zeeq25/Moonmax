using System.ComponentModel.DataAnnotations;

namespace Moonmax.ViewModels
{
    public class CustomerFormViewModel
    {
        public int ClientID { get; set; } // nullable for Add

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string Phone { get; set; }

        [Required]
        public string PaymentType { get; set; }
    }
}
