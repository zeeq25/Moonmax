using System.ComponentModel.DataAnnotations;

namespace Moonmax.ViewModels
{
    public class CreateTechnicianViewModel
    {
        [Required(ErrorMessage = "Technician Name is required.")]
        public string Name { get; set; } = string.Empty;
    }
}
