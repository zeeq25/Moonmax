using Microsoft.AspNetCore.Mvc.Rendering;

public class EditJobOrderViewModel
{
    public int JobID { get; set; }
    public int ClientID { get; set; }
    public string ServiceType { get; set; }
    public int TechnicianID { get; set; }
    public DateTime Created { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal Cost { get; set; }
    public string Status { get; set; }

    public List<SelectListItem> Clients { get; set; } = new();
    public List<SelectListItem> Technicians { get; set; } = new();

    // Add this for status dropdown
    public List<SelectListItem> StatusList { get; set; } = new();
}
