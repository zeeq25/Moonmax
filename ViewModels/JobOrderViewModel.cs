using System;

namespace Moonmax.ViewModels
{
    public class JobOrderViewModel
    {
        public int JobID { get; set; }
        public string Client { get; set; }

        public int? ClientID { get; set; }  
        public string ServiceType { get; set; }
        public string Technician { get; set; }
        public int? TechnicianID { get; set; }
        public DateTime Created { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal Cost { get; set; }
        public string Status { get; set; }

        public string DisplayClient
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Client))
                    return "Walk-In";

                return Client;
            }
        }
    }
}
