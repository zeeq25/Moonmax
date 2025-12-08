namespace Moonmax.ViewModels
{
    public class JobOrderDetailsViewModel
    {
        public int JobID { get; set; }
        public string Client { get; set; }
        public string ServiceType { get; set; }
        public string Technician { get; set; }
        public DateTime Created { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal Cost { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }

        // Collection of parts used for this job
        public List<JobOrderPartViewModel> Parts { get; set; } = new List<JobOrderPartViewModel>();
    }

    public class JobOrderPartViewModel
    {
        public string PartName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }

        // Optional computed property
        //public decimal TotalCost => Quantity * UnitCost;
    }

}
