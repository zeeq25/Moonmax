using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moonmax.Data;
using Moonmax.ViewModels;

namespace Moonmax.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeDashboardController : Controller
    {
        private readonly AppDbContext _db;

        public EmployeeDashboardController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var now = DateTime.Now;
            var firstDayMonth = new DateTime(now.Year, now.Month, 1);

            var vm = new DashboardViewModel
            {
                // JOB ORDERS
                ActiveRepairs = _db.JobOrders.Count(j => j.Status == "In Progress"),
                CompletedJobsMonth = _db.JobOrders.Count(j =>
                    j.Status == "Completed" &&
                    j.DueDate >= firstDayMonth
                ),
                // INVENTORY
                TotalInventoryStock = _db.Inventories.Sum(i => i.QuantityInStock),

                // TODAY SUMMARY
                NewWorkOrders = _db.JobOrders.Count(j => j.CreatedAt.Date == now.Date),
                CompletedJobsToday = _db.JobOrders.Count(j =>
                    j.Status == "Completed" &&
                    j.DueDate != null &&
                    j.DueDate.Value.Date == now.Date),

                // ACTIVITY FEED
                RecentActivity = _db.JobOrders
                    .Include(j => j.Client)
                    .OrderByDescending(j => j.CreatedAt)
                    .Take(5)
                    .Select(j => $"Job Order #{j.JobID} created for {j.Client.Name}")
                    .ToList(),

                // LOW STOCK ITEMS
                LowStockItems = _db.Inventories
                    .Where(i => i.QuantityInStock <= i.ReorderLevel)
                    .OrderBy(i => i.QuantityInStock)
                    .Select(i => new LowStockItem
                    {
                        ItemName = i.PartName,
                        Quantity = i.QuantityInStock,
                        ReorderLevel = i.ReorderLevel
                    })
                    .ToList()
            };

            // Work Orders by Service
            var workOrdersByService = _db.JobOrders
                .Where(j => j.Status == "Active" || j.Status == "Completed")
                .GroupBy(j => j.ServiceType)
                .Select(g => new { Service = g.Key, Count = g.Count() })
                .ToList();
            vm.WorkOrderServices = workOrdersByService.Select(x => x.Service).ToList();
            vm.WorkOrderCounts = workOrdersByService.Select(x => x.Count).ToList();

            // Weekly Output
            vm.WeeklyLabels = Enumerable.Range(0, 7)
                .Select(i => now.AddDays(-i).ToString("MMM dd")).Reverse().ToList();
            vm.WeeklyOutput = Enumerable.Range(0, 7)
                .Select(i => _db.JobOrders.Count(j =>
                    j.Status == "Completed" &&
                    j.DueDate.HasValue &&
                    j.DueDate.Value.Date == now.AddDays(-i).Date))
                .Reverse()
                .ToList();

            // Inventory Values
            var inventories = _db.Inventories.ToList();
            vm.InventoryItems = inventories.Select(i => i.PartName).ToList();
            vm.InventoryValues = inventories.Select(i => i.QuantityInStock).ToList();

            return View(vm);
        }
    }
}