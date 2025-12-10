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
            return View(); // You can show a summary page or redirect to SalesReport
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