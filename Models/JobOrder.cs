using System;
using System.ComponentModel.DataAnnotations;

namespace Moonmax.Models
{
    public class JobOrder
    {
        [Key]
        public int JobID { get; set; }

        public int? ClientID { get; set; } // nullable for Walk-In
        public Client? Client { get; set; }

        public string? ContactNumber { get; set; } 

        [Required]
        public int TechnicianID { get; set; }
        public Technicians? Technician { get; set; }

        [Required]
        public string ServiceType { get; set; }
        
        public DateTime CreatedAt { get; set; }

        public DateTime? DueDate { get; set; }

        [Required]
        public decimal Cost { get; set; }

        [Required]
        public string Status { get; set; }

        public ICollection<JobPart> JobParts { get; set; }

    }


}
