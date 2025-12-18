using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moonmax.Data;
using Moonmax.ViewModels;
using Microsoft.EntityFrameworkCore;


[Authorize(Roles ="Admin")]
public class DashboardController : Controller
{
    private readonly AppDbContext _db;
    public DashboardController(AppDbContext db)
    {
        _db = db;
    }
    public IActionResult Index()
    {
        var now = DateTime.Now;
        var firstDayMonth = new DateTime(now.Year, now.Month, 1);

        var vm = new DashboardViewModel
        {
            // USERS
            TotalUsers = _db.Users.Count(),
            ActiveUsers = _db.Users.Count(u => u.IsActive),
            // JOB ORDERS
            ActiveRepairs = _db.JobOrders.Count(j => j.Status == "In Progress"),
            CompletedJobsMonth = _db.JobOrders.Count(j =>
                j.Status == "Completed" &&
                j.DueDate >= firstDayMonth
            ),
            // INVENTORY
            TotalInventoryStock = _db.Inventories.Sum(i => i.QuantityInStock),
            // SALES
            MonthlySales = _db.Invoices
                .Where(i => i.Status == "Paid" && i.DateIssued >= firstDayMonth)
                .Sum(i => (decimal?)i.Amount) ?? 0,
            // TODAY SUMMARY
            NewWorkOrders = _db.JobOrders.Count(j => j.CreatedAt.Date == now.Date),
            CompletedJobsToday = _db.JobOrders.Count(j =>
                j.Status == "Completed" &&
                j.DueDate != null &&
                j.DueDate.Value.Date == now.Date),
            InvoicesSent = _db.Invoices.Count(i => i.DateIssued.Date == now.Date),
            PendingPayments = _db.Invoices
                .Where(i => i.Status == "Pending")
                .Sum(i => (decimal?)i.Amount) ?? 0,
            // ACTIVITY FEED
            RecentActivity = _db.JobOrders
                .Include(j => j.Client)
                .OrderByDescending(j => j.CreatedAt)
                .Take(5)
                .Select(j => $"Job Order #{j.JobID} created for {j.Client.Name}")
                .ToList(),

            // LOW STOCK ITEMS - NEW
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