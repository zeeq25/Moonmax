namespace Moonmax.ViewModels
{
    public class JobOrderRecordVM
    {
        public int JobID { get; set; }

        // Auto-generated order number
        public string JobOrderNumber { get; set; }

        public string ClientName { get; set; }
        public string TechnicianName { get; set; }
        public string ServiceType { get; set; }
        public decimal Cost { get; set; }
        public string Status { get; set; }
        public string? ContactNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }

        // Added parts/items
        public List<string> Parts { get; set; } = new List<string>();
    }

}
