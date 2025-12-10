using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moonmax.Data;



namespace Moonmax.Controllers
{
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // KPIs
            var totalRevenue = _context.Invoices
                .Where(i => i.Status == "Paid")
                .Sum(i => i.Amount);

            var totalJobs = _context.JobOrders.Count();
            var avgJobValue = totalJobs > 0 ? _context.JobOrders.Average(j => j.Cost) : 0;

            // Total Inventory Items (dynamic)
            var totalInventoryItems = _context.Inventories.Sum(i => i.QuantityInStock);
            // <-- Replace "Inventories" with your inventory table name

            // Pass data to ViewBag
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalJobs = totalJobs;
            ViewBag.AvgJobValue = avgJobValue;
            ViewBag.TotalInventoryItems = totalInventoryItems;

            // Monthly sales trend (last 6 months)
            var monthlySales = _context.Invoices
                .Where(i => i.Status == "Paid")
                .GroupBy(i => new { i.DateIssued.Year, i.DateIssued.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(i => i.Amount)
                })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .AsEnumerable()  // Move to memory
                .TakeLast(6)     // Safe now
                .ToList();



            ViewBag.Months = monthlySales.Select(m => $"{m.Month}/{m.Year}").ToList();
            ViewBag.MonthlyRevenue = monthlySales.Select(m => m.Revenue).ToList();


            // Service type distribution
            var serviceRevenue = _context.JobOrders
                .GroupBy(j => j.ServiceType)
                .Select(g => new { ServiceType = g.Key, Revenue = g.Sum(j => j.Cost) })
                .ToList();

            ViewBag.ServiceTypes = serviceRevenue.Select(s => s.ServiceType).ToList();
            ViewBag.ServiceRevenue = serviceRevenue.Select(s => s.Revenue).ToList();

            // Top clients
            var topClients = _context.Invoices
                .Include(i => i.Client)
                .GroupBy(i => i.Client.Name)
                .Select(g => new { ClientName = g.Key, Revenue = g.Sum(i => i.Amount) })
                .OrderByDescending(c => c.Revenue)
                .Take(5)
                .ToList();

            ViewBag.TopClients = topClients.Select(c => c.ClientName).ToList();
            ViewBag.TopClientsRevenue = topClients.Select(c => c.Revenue).ToList();

            // Inventory turnover (example: counts per category)
            var turnover = _context.Inventories
                .GroupBy(i => i.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToList();

            ViewBag.TurnoverCategories = turnover.Select(t => t.Category).ToList();
            ViewBag.TurnoverValues = turnover.Select(t => t.Count).ToList();

            return View();
        }


        [HttpGet]
        public IActionResult SalesReport()
        {
            // KPIs
            var totalRevenue = _context.Invoices
                .Where(i => i.Status == "Paid")
                .Sum(i => i.Amount);
            var totalJobs = _context.JobOrders.Count();
            var avgJobValue = totalJobs > 0 ? _context.JobOrders.Average(j => j.Cost) : 0;

            // Top client by revenue
            var topClient = _context.Invoices
                .Include(i => i.Client)
                .GroupBy(i => i.ClientID)
                .Select(g => new
                {
                    ClientName = g.FirstOrDefault().Client.Name,
                    Revenue = g.Sum(i => i.Amount)
                })
                .OrderByDescending(x => x.Revenue)
                .FirstOrDefault();

            // Monthly sales trend
            var monthlyRevenue = _context.Invoices
                .GroupBy(i => new { i.DateIssued.Year, i.DateIssued.Month })
                .Select(g => new Moonmax.ViewModels.MonthlyRevenueViewModel
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(i => i.Amount)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            // Sales by service type
            var serviceRevenue = _context.JobOrders
                .GroupBy(j => j.ServiceType)
                .Select(g => new Moonmax.ViewModels.ServiceRevenueViewModel
                {
                    ServiceType = g.Key,
                    Revenue = g.Sum(j => j.Cost)
                })
                .ToList();

            // Invoices
            var allInvoices = _context.Invoices
                .Include(i => i.Client)
                .OrderByDescending(i => i.DateIssued)
                .ToList();

            // Pass to ViewBag (or ViewModel if you prefer strongly typed)
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalJobs = totalJobs;
            ViewBag.AvgJobValue = avgJobValue;
            ViewBag.TopClient = topClient;
            ViewBag.MonthlyRevenue = monthlyRevenue;
            ViewBag.ServiceRevenue = serviceRevenue;
            ViewBag.AllInvoices = allInvoices;

            return View();
        }
    }

}