using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moonmax.Models
{

    [Table("Technician")]
    public class Technicians
    {
        [Key]
        public int TechnicianID { get; set; }
        public string Name { get; set; }

        public ICollection<JobOrder> JobOrders { get; set; } = new List<JobOrder>();
    }
}
