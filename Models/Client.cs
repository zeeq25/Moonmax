using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Moonmax.Models
{
    public class Client
    {
        [Key]
        public int ClientID { get; set; }
        public string Name { get; set; }
        

        public ICollection<JobOrder> JobOrders { get; set; } = new List<JobOrder>();
    }
}
