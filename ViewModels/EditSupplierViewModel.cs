using System.ComponentModel.DataAnnotations;

namespace Moonmax.ViewModels
{
    public class EditSupplierViewModel
    {
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string ProductLine { get; set; }
        public string PaymentTerms { get; set; }
        public string Status { get; set; }
    }

}
