using Microsoft.EntityFrameworkCore;
using Moonmax.Models;

namespace Moonmax.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        //USER MANAGEMENT
        public DbSet<User> Users { get; set; }


        //JOB ORDERS 
        public DbSet<JobOrder> JobOrders { get; set; }
        public DbSet<JobPart> JobParts { get; set; }
        public DbSet<Client> Client { get; set; }
        public DbSet<Technicians> Technician { get; set; }
        public DbSet<JobOrderPart> JobOrderParts { get; set; }

        //INVENTORY
        public DbSet<Inventory> Inventories { get; set; }

        //SUPPLIERS
        public DbSet<Supplier> Suppliers { get; set; }

        //PURCHASE ORDERS
        public DbSet<PurchaseOrders> PurchaseOrders { get; set; } // ✅ add this
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; } // ✅ add this
    }
}
