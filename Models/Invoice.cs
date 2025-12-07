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


        // JobOrder Table
        [Required]
        [ForeignKey("JobOrder")]
        public int JobID { get; set; }
        public JobOrder JobOrder { get; set; }  

        [Required]
        public decimal Amount { get; set; } // JobOrder.Cost + sum(JobParts.TotalCost)

        [MaxLength(50)]
        public string PaymentType { get; set; }

        public DateTime DateIssued { get; set; }

        public DateTime? DueDate { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }  // Paid, Pending, Cancelled
    }
}
