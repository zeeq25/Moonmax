using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moonmax.Data;
using Moonmax.Models;
using Moonmax.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace Moonmax.Controllers
{

    [Authorize(Roles = "Admin, Employee")]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // ================================
            // 1. MONTHLY REVENUE (Invoices, excluding Pending)
            // ================================
            var monthlyRevenue = _context.Invoices
                .Where(i => i.Status != "Pending")
                .GroupBy(i => new { i.DateIssued.Year, i.DateIssued.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Revenue = g.Sum(x => x.Amount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            ViewBag.Months = (monthlyRevenue?.Select(x => new DateTime(x.Year, x.Month, 1)
                                                .ToString("MMM yyyy"))
                                  .ToList()) ?? new List<string>();

            ViewBag.MonthlyRevenue = (monthlyRevenue?.Select(x => x.Revenue).ToList()) ?? new List<decimal>();

            // =========================================
            // 2. STOCK BALANCE (Inventory quantities)
            // =========================================
            var stock = _context.Inventories
                .Select(i => new
                {
                    Item = i.PartName,
                    Qty = i.QuantityInStock
                })
                .ToList();

            ViewBag.StockItems = (stock?.Select(x => x.Item).ToList()) ?? new List<string>();
            ViewBag.StockValues = (stock?.Select(x => x.Qty).ToList()) ?? new List<int>();

            // =========================================
            // 3. JOBS PER SERVICE TYPE
            // =========================================
            var jobsPerService = _context.JobOrders
                .GroupBy(j => j.ServiceType)
                .Select(g => new
                {
                    ServiceType = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            ViewBag.ServiceTypes = (jobsPerService?.Select(x => x.ServiceType).ToList()) ?? new List<string>();
            ViewBag.ServiceCounts = (jobsPerService?.Select(x => x.Count).ToList()) ?? new List<int>();

            return View();
        }





        // GET: Reports/SalesReport
        public IActionResult SalesReport(DateTime? fromDate, DateTime? toDate)
        {
            // KPI Calculations for SalesReport
            ViewBag.TotalRevenue = _context.Invoices
                .Where(i => i.Status != "Pending")
                .Sum(i => i.Amount);

            ViewBag.InvoiceCount = _context.Invoices.Count();

            ViewBag.AvgInvoiceValue = _context.Invoices
                .Where(i => i.Status != "Pending")
                .Any() ? _context.Invoices.Where(i => i.Status != "Pending").Average(i => i.Amount) : 0;

            ViewBag.TopServiceRevenue = _context.JobOrders
                .GroupBy(j => j.ServiceType)
                .Select(g => new { ServiceType = g.Key, Revenue = g.Sum(j => j.Cost) })
                .OrderByDescending(x => x.Revenue)
                .FirstOrDefault()?.ServiceType ?? "N/A";

            ViewBag.OutstandingReceivables = _context.Invoices
                .Where(i => i.Status == "Pending")
                .Sum(i => i.Amount);

            // Set default range if not provided
            var startDate = fromDate ?? DateTime.Now.AddDays(-30);
            var endDate = toDate ?? DateTime.Now;

            // -------------------------------
            // 1. Fetch invoices (with related Client and JobOrder)
            // -------------------------------
            var invoices = _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.JobOrder)
                .Where(i => i.DateIssued >= startDate && i.DateIssued <= endDate)
                .OrderByDescending(i => i.DateIssued)
                .AsEnumerable() // <- move to in-memory processing
                .Select(i => new InvoiceReportVM
                {
                    InvoiceNumber = i.InvoiceNumber,
                    ClientName = i.Client.Name,               // navigation property
                    ServiceType = i.JobOrder.ServiceType,     // navigation property
                    Amount = i.Amount,
                    PaymentType = i.PaymentType,
                    DateIssued = i.DateIssued,
                    DueDate = i.DueDate,
                    Status = i.Status,
                  
                })
                .ToList();

            // -------------------------------
            // 2. Monthly revenue for chart
            // -------------------------------
            var monthlyRevenue = _context.Invoices
                .Where(i => i.Status != "Pending")
                .GroupBy(i => new { i.DateIssued.Year, i.DateIssued.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Revenue = g.Sum(x => x.Amount)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            // -------------------------------
            // 3. Service type revenue for chart
            // -------------------------------
            var serviceRevenue = _context.JobOrders
                .GroupBy(j => j.ServiceType)
                .Select(g => new
                {
                    ServiceType = g.Key,
                    Revenue = g.Sum(j => j.Cost)
                })
                .ToList();

            // -------------------------------
            // 4. Prepare ViewModel
            // -------------------------------
            var vm = new SalesReportVM
            {
                InvoiceReport = invoices,
                Months = monthlyRevenue.Select(x => new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy")).ToList(),
                MonthlyRevenue = monthlyRevenue.Select(x => x.Revenue).ToList(),
                ServiceTypes = serviceRevenue.Select(x => x.ServiceType).ToList(),
                ServiceRevenue = serviceRevenue.Select(x => x.Revenue).ToList(),
                TotalRevenue = invoices.Sum(x => x.Amount),
                InvoiceCount = invoices.Count,
                AvgInvoiceValue = invoices.Any() ? invoices.Average(x => x.Amount) : 0,
                TopServiceRevenue = serviceRevenue.OrderByDescending(x => x.Revenue).FirstOrDefault()?.ServiceType ?? "-"
            };

            return View(vm);
        }


        // GET: Reports/JobReport
        public IActionResult JobReport(string? status, DateTime? fromDate, DateTime? toDate)
        {
            var startDate = DateTime.Now.AddDays(-30);
            var endDate = DateTime.Now;

            // Base query including JobParts
            var query = _context.JobOrders
                .Include(j => j.Client)
                .Include(j => j.Technician)
                .Include(j => j.JobParts)
                    .ThenInclude(p => p.Inventory) // Include Inventory for part names
                .Where(j => j.CreatedAt >= startDate && j.CreatedAt <= endDate)
                .AsQueryable();

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status) && status != "All")
                query = query.Where(j => j.Status == status);

            // Map to JobOrderRecordVM including parts
            var jobs = query
                .Include(j => j.JobParts) // Include related parts
                .ThenInclude(p => p.Inventory) // Include inventory details
                .OrderByDescending(j => j.CreatedAt)
                .AsEnumerable()
                .Select(j => new JobOrderRecordVM
                {
                    JobID = j.JobID,
                    JobOrderNumber = $"JOB-{j.JobID:0000}",
                    ClientName = j.Client?.Name ?? "Walk-In",
                    TechnicianName = j.Technician?.Name ?? "N/A",
                    ServiceType = j.ServiceType,
                    Cost = j.Cost,
                    Status = j.Status,
                    ContactNumber = j.ContactNumber,
                    CreatedAt = j.CreatedAt,
                    DueDate = j.DueDate,
                    Parts = j.JobParts?.Select(p => $"{p.Inventory.PartName} x{p.Quantity}").ToList() ?? new List<string>()
                })
                .ToList();

            // KPIs
            var totalJobs = jobs.Count;
            var completedJobs = jobs.Count(j => j.Status == "Completed");
            var pendingJobs = jobs.Count(j => j.Status == "Pending");
            var jobRevenue = jobs.Where(j => j.Status == "Completed").Sum(j => j.Cost);

            // Chart: Jobs by status
            var chartStatuses = new[] { "Pending", "In Progress", "Completed" };
            var chartCounts = chartStatuses.Select(s => jobs.Count(j => j.Status == s)).ToList();

            // Chart: Jobs by service type
            var serviceGroups = jobs.GroupBy(j => j.ServiceType)
                                    .Select(g => new { ServiceType = g.Key, Count = g.Count() })
                                    .OrderByDescending(x => x.Count)
                                    .ToList();
            var chartServiceTypes = serviceGroups.Select(x => x.ServiceType).ToList();
            var chartServiceCounts = serviceGroups.Select(x => x.Count).ToList();

            // Chart: Jobs over time
            var dateGroups = jobs.GroupBy(j => j.CreatedAt.Date)
                                 .Select(g => new { Date = g.Key, Count = g.Count() })
                                 .OrderBy(x => x.Date)
                                 .ToList();
            var chartDates = dateGroups.Select(x => x.Date.ToString("yyyy-MM-dd")).ToList();
            var chartDateCounts = dateGroups.Select(x => x.Count).ToList();

            // Build ViewModel
            var vm = new JobOrderReportVM
            {
                JobOrderList = jobs,
                TotalJobs = totalJobs,
                CompletedJobs = completedJobs,
                PendingJobs = pendingJobs,
                JobRevenue = jobRevenue,
                ChartStatuses = chartStatuses.ToList(),
                ChartCounts = chartCounts,
                ChartServiceTypes = chartServiceTypes,
                ChartServiceCounts = chartServiceCounts,
                ChartDates = chartDates,
                ChartDateCounts = chartDateCounts,
                FiltersStatuses = new List<string> { "Pending", "In Progress", "Completed" },
                FiltersServiceTypes = jobs.Select(j => j.ServiceType).Distinct().OrderBy(s => s).ToList()
            };

            return View(vm);
        }


        // GET: Reports/InventoryReport
        public async Task<IActionResult> InventoryReport(DateTime? fromDate, DateTime? toDate)

        {

            var vm = new InventoryReportVM();

            // Filter dates
            var start = fromDate ?? DateTime.Now.AddDays(-30);
            var end = toDate ?? DateTime.Now;

            // Get inventories
            var inventories = await _context.Inventories
                .Include(i => i.Supplier)
                .ToListAsync();

            vm.Inventories = inventories.Select(i => new InventoryReportVM.InventoryItemVM
            {
                InventoryID = i.InventoryID,
                Category = i.Category,
                PartName = i.PartName,
                UnitCost = i.UnitCost,
                QuantityInStock = i.QuantityInStock,
                ReorderLevel = i.ReorderLevel,
                SupplierName = i.Supplier?.SupplierName ?? "-",
                //CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt


            }).ToList();

            // KPIs
            vm.TotalStockValue = inventories.Sum(i => i.UnitCost * i.QuantityInStock);
            vm.LowStockCount = inventories.Count(i => i.QuantityInStock <= i.ReorderLevel);

            var stockMovements = await _context.StockMovement
                .Where(s => s.MovementDate >= start && s.MovementDate <= end)
                .ToListAsync();

            vm.TotalStockAdded = stockMovements
                .Where(s => s.MovementType == "IN").Sum(s => s.Quantity);

            vm.TotalStockUsed = stockMovements
                .Where(s => s.MovementType == "OUT").Sum(s => s.Quantity);

            // Chart: Top 5 consumed items (OUT only)
            var topConsumed = stockMovements
                .Where(s => s.MovementType == "OUT")
                .GroupBy(s => s.Inventory.PartName)
                .Select(g => new { PartName = g.Key, QuantityUsed = g.Sum(s => s.Quantity) })
                .OrderByDescending(g => g.QuantityUsed)
                .Take(5)
                .ToList();

            vm.UsageDates = topConsumed.Select(x => x.PartName).ToList(); // Labels = top 5 items
            vm.UsageQuantities = topConsumed.Select(x => x.QuantityUsed).ToList(); // Values = quantities used

            
            ViewBag.FromDate = start.ToString("yyyy-MM-dd");
            ViewBag.ToDate = end.ToString("yyyy-MM-dd");

            return View(vm);
        }
    }

}