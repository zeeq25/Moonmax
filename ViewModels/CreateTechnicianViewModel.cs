using System.ComponentModel.DataAnnotations;

namespace Moonmax.ViewModels
{
    public class CreateTechnicianViewModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
