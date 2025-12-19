using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Moonmax.ViewModels
{
    public class CreateJobOrderViewModel
    {   
       
         
        public int? ClientID { get; set; } // Nullable for Walk-In
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Technician is required.")]
        public int TechnicianID { get; set; }

        [Required(ErrorMessage = "Service type is required.")]
        public string ServiceType { get; set; }

        [Required(ErrorMessage = "Due date is required.")]
        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "Job cost is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Job cost must be greater than 0")]
        public decimal Cost { get; set; }

        
        //public string Status { get; set; }

        public List<SelectListItem> Technicians { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Clients { get; set; } = new List<SelectListItem>();

        // Add this for status dropdown
        public List<SelectListItem> StatusList { get; set; } = new();

        // Add this for service type dropdown
        public List<SelectListItem> ServiceTypes { get; set; } = new();
    }
}
