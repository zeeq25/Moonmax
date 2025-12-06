using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moonmax.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("Id")]
        public int UserID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = null!; // non-nullable

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = null!; // non-nullable

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = null!; // non-nullable

        [Required]
        [Column("Password")]
        public string PasswordHash { get; set; } = null!; // non-nullable

        [Column("Role")]
        public string? Role { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
