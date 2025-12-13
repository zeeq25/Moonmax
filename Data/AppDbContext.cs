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
        public DbSet<PurchaseOrders> PurchaseOrders { get; set; } 
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; } 

        //SALES
        public DbSet<Invoice> Invoices { get; set; }

        //STOCK MOVEMENT
        public DbSet<StockMovement> StockMovement { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add decimal precision here
            modelBuilder.Entity<Invoice>()
                .Property(i => i.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<JobOrder>()
                .Property(j => j.Cost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<JobOrderPart>()
                .Property(jp => jp.Cost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<JobPart>()
                .Property(jp => jp.UnitCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PurchaseOrderItem>()
                .Property(poi => poi.UnitPrice)
                .HasPrecision(18, 2);

            // You can also configure other relationships or constraints here
        }

    }


}
