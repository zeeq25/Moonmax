using System;
using System.Collections.Generic;

namespace Moonmax.ViewModels
{
    public class JobOrderListingVM
    {
        public int JobID { get; set; }
        public string Client { get; set; }
        public string ServiceType { get; set; }
        public string Technician { get; set; }
        public DateTime Created { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal Cost { get; set; }
        public string Status { get; set; }
        public int TechnicianID { get; set; }
    }

    public class JobOrdersIndexViewModel
    {
        public int PendingJobs { get; set; }
        public int InProgressJobs { get; set; }
        public int CompletedJobs { get; set; }
        public decimal TotalRevenue { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public List<JobOrderListingVM> JobOrders { get; set; } = new List<JobOrderListingVM>();
    }
}
