using System.ComponentModel.DataAnnotations;

namespace Moonmax.ViewModels
{
    public class CreateUserViewModel
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, MaxLength(100), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "";

        public bool IsActive { get; set; } = true;

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
