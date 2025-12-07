using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Moonmax.Models
{
    public class Client
    {
        [Key]
        public int ClientID { get; set; }
        public string Name { get; set; }


        // NEW
        public string? ContactNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string PaymentType { get; set; } = "Cash"; // default


        // 🔹 Add this navigation property
        public ICollection<Invoice> Invoices { get; set; }



        public ICollection<JobOrder> JobOrders { get; set; } = new List<JobOrder>();
    }
}
