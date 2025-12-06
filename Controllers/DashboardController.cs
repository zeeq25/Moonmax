using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moonmax.ViewModels;

namespace Moonmax.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var vm = new DashboardViewModel
            {
                // You only have Users table so far:
                TotalUsers = 5,
                ActiveUsers = 5,

                // Placeholder values for future modules:
                ActiveRepairs = 0,
                CompletedJobsMonth = 0,
                TotalInventoryStock = 0,
                MonthlySales = 0,

                // Placeholder values for today's summary:
                NewWorkOrders = 10,
                CompletedJobsToday = 10,
                InvoicesSent = 10,
                PendingPayments = 1000,

                RecentActivity = new List<string>
                {
                    "System initialized – no activity data yet."
                }

                
            };

            ViewData["Nav"] = "Dashboard";
            return View(vm);
        }
    }
}
