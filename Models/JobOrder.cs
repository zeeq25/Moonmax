using System;
using System.ComponentModel.DataAnnotations;

namespace Moonmax.Models
{
    public class JobOrder
    {
        [Key]
        public int JobID { get; set; }

        [Required]
        public int ClientID { get; set; } 
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
      
        public string Status { get; set; }

        public ICollection<JobPart> JobParts { get; set; }

    }


}
