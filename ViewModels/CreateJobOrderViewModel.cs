using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Moonmax.ViewModels
{
    public class CreateJobOrderViewModel
    {
        public int? ClientID { get; set; } // optional for walk-in
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Technician is required.")]
        public int TechnicianID { get; set; }

        [Required(ErrorMessage = "Service type is required.")]
        public string ServiceType { get; set; }

        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "Cost is required.")]
        public decimal Cost { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; }

        public List<SelectListItem> Technicians { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Clients { get; set; } = new List<SelectListItem>();
    }
}
