using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moonmax.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceID { get; set; }

        [Required]
        [MaxLength(50)]
        public string InvoiceNumber { get; set; }

        [Required]
        [ForeignKey("Client")]
        public int ClientID { get; set; }
        public Client Client { get; set; }

        [Required]
        [ForeignKey("JobOrder")]
        public int JobID { get; set; }
        public JobOrder JobOrder { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required] // ← ADD THIS
        [MaxLength(50)]
        public string PaymentType { get; set; }

        [Required] // ← ADD THIS
        public DateTime DateIssued { get; set; }

        public DateTime? DueDate { get; set; }

        [Required] // ← ADD THIS
        [MaxLength(50)]
        public string Status { get; set; }

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>(); // ← ADD INITIALIZATION
    }
}
