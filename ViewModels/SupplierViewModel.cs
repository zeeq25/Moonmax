namespace Moonmax.ViewModels
{
    public class SupplierViewModel
    {
        public int SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public string ProductLine { get; set; }
        public string PaymentTerms { get; set; }
        

        // Computed property for display
        public string PaymentTermsDisplay
        {
            get
            {
                return PaymentTerms switch
                {
                    "COD" => "Cash On Delivery",
                    "PDC-15" => "PDC(15 Days)",
                    "PDC-30" => "PDC(30 Days)",
                    "PDC-60" => "PDC(60 Days)",
                    _ => PaymentTerms
                };
            }
        }
        public string Status { get; set; }




    }


}
