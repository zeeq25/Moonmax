using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moonmax.Data;
using Moonmax.Models;
using Moonmax.ViewModels;
using Moonmax.Services;  // ⬅️ ADD THIS
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;  // ⬅️ ADD THIS

namespace Moonmax.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAuditService _auditService;  // ⬅️ ADD THIS

        // ⬅️ UPDATE CONSTRUCTOR
        public SalesController(AppDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;  // ⬅️ ADD THIS
        }

        // GET: Sales/CreateInvoice/5
        public async Task<IActionResult> CreateInvoice(int jobId)
        {
            var jobOrder = await _context.JobOrders
                .Include(j => j.JobParts)
                .Include(j => j.Client)
                .FirstOrDefaultAsync(j => j.JobID == jobId);

            if (jobOrder == null)
            {
                return NotFound();
            }

            decimal totalPartsCost = jobOrder.JobParts.Sum(p => p.TotalCost);
            decimal totalInvoiceAmount = jobOrder.Cost + totalPartsCost;

            string invoiceNumber = $"INV-{DateTime.Now:yyyyMMddHHmmss}";

            DateTime dateIssued = DateTime.Now;
            DateTime? dueDate = null;

            string paymentType = jobOrder.Client?.PaymentType ?? "Cash";

            switch (paymentType)
            {
                case "PDC-15 DAYS":
                    dueDate = dateIssued.AddDays(15);
                    break;
                case "PDC-30 DAYS":
                    dueDate = dateIssued.AddDays(30);
                    break;
                case "PDC-60 DAYS":
                    dueDate = dateIssued.AddDays(60);
                    break;
                default:
                    dueDate = dateIssued;
                    break;
            }

            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                ClientID = jobOrder.ClientID,
                JobID = jobOrder.JobID,
                Amount = totalInvoiceAmount,
                PaymentType = jobOrder.Client?.PaymentType ?? "Cash",
                DateIssued = DateTime.Now,
                DueDate = dueDate,
                Status = "Pending"
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            // ⬇️⬇️⬇️ ADD AUDIT LOG HERE ⬇️⬇️⬇️
            string clientName = jobOrder.Client?.Name ?? $"Walk-In ({jobOrder.ContactNumber})";

            await _auditService.LogAsync(
                userId: GetCurrentUserId(),
                action: "CREATE",
                module: "Sales & Billing",
                description: $"Created Invoice {invoiceNumber} for {clientName} - Job #{jobOrder.JobID}, Amount: ₱{totalInvoiceAmount:N2}, Payment: {paymentType}",
                targetId: invoice.InvoiceID
            );
            // ⬆️⬆️⬆️ END AUDIT LOG ⬆️⬆️⬆️

            return RedirectToAction("Index", "Sales");
        }

        // GET: Sales       
        public async Task<IActionResult> Index()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.JobOrder)
                .ThenInclude(j => j.JobParts)
                .Select(i => new InvoiceViewModel
                {
                    InvoiceID = i.InvoiceID,
                    InvoiceNumber = i.InvoiceNumber,

                    ClientName = i.Client.Name == "Walk-In"
                    ? $"Walk-In ({i.JobOrder.ContactNumber})"
                    : i.Client.Name,

                    ContactNumber = i.Client.Name == "Walk-In"
                    ? i.JobOrder.ContactNumber
                    : i.Client.ContactNumber,

                    JobID = i.JobID,
                    Amount = i.Amount,
                    PaymentType = i.PaymentType,
                    DateIssued = i.DateIssued,
                    DueDate = i.DueDate,
                    Status = i.Status
                })
                .ToListAsync();

            // REMOVED: InProgressJobs query

            var totalSales = invoices.Sum(i => i.Amount);
            var paidInvoices = invoices
                .Where(i => i.Status.ToLower() == "paid")
                .Sum(i => i.Amount);
            var pendingPDCs = invoices
                .Where(i => i.Status.ToLower() == "pending")
                .Sum(i => i.Amount);
            var activeCustomers = invoices
                .Select(i => i.ClientName)
                .Distinct()
                .Count();

            var vm = new SalesIndexViewModel
            {
                Invoices = invoices,
                InProgressJobs = new List<JobOrderListingVM>(), // Empty list
                TotalSales = totalSales,
                PaidInvoices = paidInvoices,
                PendingPDCs = pendingPDCs,
                ActiveCustomers = activeCustomers
            };

            return View(vm);
        }

        // GET - CUSTOMER LIST
        public async Task<IActionResult> CustomerList()
        {
            var clients = await _context.Client
                .Select(c => new CustomerViewModel
                {
                    ClientID = c.ClientID,
                    Name = c.Name,
                    Email = c.Email,
                    Phone = c.ContactNumber,
                    PaymentType = c.PaymentType,
                    TotalTransactions = c.Invoices.Count(),

                    TotalRevenue = c.Invoices
                        .Where(i => i.Status == "Paid")
                        .Sum(i => i.Amount),

                    Outstanding = c.Invoices
                        .Where(i => i.Status != "Paid")
                        .Sum(i => i.Amount)
                })
                .ToListAsync();

            return View(clients);
        }

        // GET: Sales/CreateCustomer
        public IActionResult CreateCustomer()
        {
            ViewBag.PaymentTypes = new[] { "Cash", "PDC-15 DAYS", "PDC-30 DAYS", "PDC-60 DAYS" };
            return View();
        }

        // POST: Sales/CreateCustomer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCustomer(CustomerFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PaymentTypes = new[] { "Cash", "PDC-15 DAYS", "PDC-30 DAYS", "PDC-60 DAYS" };
                return View(model);
            }

            var client = new Client
            {
                Name = model.Name,
                Email = model.Email,
                ContactNumber = model.Phone,
                PaymentType = model.PaymentType
            };

            _context.Client.Add(client);
            await _context.SaveChangesAsync();

            // ⬇️⬇️⬇️ ADD AUDIT LOG HERE ⬇️⬇️⬇️
            await _auditService.LogAsync(
                userId: GetCurrentUserId(),
                action: "CREATE",
                module: "Sales & Billing",
                description: $"Created new customer: {client.Name} - Payment Terms: {client.PaymentType}",
                targetId: client.ClientID
            );
            // ⬆️⬆️⬆️ END AUDIT LOG ⬆️⬆️⬆️

            return RedirectToAction("CustomerList");
        }

        // POST: Sales/ReceivePayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReceivePayment(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.JobOrder)
                .ThenInclude(j => j.JobParts)
                .FirstOrDefaultAsync(i => i.InvoiceID == id);

            if (invoice == null)
                return NotFound();

            // Mark invoice as Paid
            invoice.Status = "Paid";

            // Reduce inventory for all job parts
            if (invoice.JobOrder != null)
            {
                foreach (var part in invoice.JobOrder.JobParts)
                {
                    var inventoryItem = await _context.Inventories
                        .FirstOrDefaultAsync(i => i.InventoryID == part.InventoryID);

                    if (inventoryItem != null)
                    {
                        int previousQty = inventoryItem.QuantityInStock;

                        inventoryItem.QuantityInStock -= part.Quantity;
                        if (inventoryItem.QuantityInStock < 0)
                            inventoryItem.QuantityInStock = 0;

                        inventoryItem.ReservedQuantity -= part.Quantity;
                        if (inventoryItem.ReservedQuantity < 0)
                            inventoryItem.ReservedQuantity = 0;

                        int newQty = inventoryItem.QuantityInStock;

                        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                        int parsedUserId = int.Parse(userId);

                        var movement = new StockMovement
                        {
                            InventoryID = inventoryItem.InventoryID,
                            MovementType = "OUT",
                            Quantity = part.Quantity,
                            PreviousQuantity = previousQty,
                            NewQuantity = newQty,
                            JobOrderID = invoice.JobID,
                            UserID = parsedUserId,
                            MovementDate = DateTime.Now
                        };

                        _context.StockMovement.Add(movement);
                    }
                }

                invoice.JobOrder.Status = "Completed";
            }

            await _context.SaveChangesAsync();

            // ⬇️⬇️⬇️ ADD AUDIT LOG HERE ⬇️⬇️⬇️
            string clientName = invoice.Client?.Name ?? "Unknown";
            int totalParts = invoice.JobOrder?.JobParts?.Count ?? 0;

            await _auditService.LogAsync(
                userId: GetCurrentUserId(),
                action: "PAYMENT_RECEIVED",
                module: "Sales & Billing",
                description: $"Payment received for Invoice {invoice.InvoiceNumber} - Client: {clientName}, Amount: ₱{invoice.Amount:N2}, Job #{invoice.JobID} completed, {totalParts} parts deducted from inventory",
                targetId: invoice.InvoiceID
            );
            // ⬆️⬆️⬆️ END AUDIT LOG ⬆️⬆️⬆️

            return RedirectToAction("Index");
        }

        // GET: Sales/EditCustomer/5
        public async Task<IActionResult> EditCustomer(int id)
        {
            var client = await _context.Client.FindAsync(id);
            if (client == null)
                return NotFound();

            var model = new CustomerFormViewModel
            {
                ClientID = client.ClientID,
                Name = client.Name,
                Email = client.Email,
                Phone = client.ContactNumber,
                PaymentType = client.PaymentType
            };

            ViewBag.PaymentTypes = new[] { "Cash", "PDC-15 DAYS", "PDC-30 DAYS", "PDC-60 DAYS" };

            return View(model);
        }

        // POST: Sales/EditCustomer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCustomer(CustomerFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PaymentTypes = new[] { "Cash", "PDC-15 DAYS", "PDC-30 DAYS", "PDC-60 DAYS" };
                return View(model);
            }

            var client = await _context.Client.FindAsync(model.ClientID);
            if (client == null)
                return NotFound();

            // ⬇️ TRACK CHANGES FOR AUDIT
            var changes = new List<string>();
            if (client.Name != model.Name) changes.Add($"Name: {client.Name} → {model.Name}");
            if (client.Email != model.Email) changes.Add($"Email: {client.Email} → {model.Email}");
            if (client.PaymentType != model.PaymentType) changes.Add($"Payment Terms: {client.PaymentType} → {model.PaymentType}");

            client.Name = model.Name;
            client.Email = model.Email;
            client.ContactNumber = model.Phone;
            client.PaymentType = model.PaymentType;

            await _context.SaveChangesAsync();

            // ⬇️⬇️⬇️ ADD AUDIT LOG HERE ⬇️⬇️⬇️
            if (changes.Any())
            {
                await _auditService.LogAsync(
                    userId: GetCurrentUserId(),
                    action: "UPDATE",
                    module: "Sales & Billing",
                    description: $"Updated customer {client.Name}: {string.Join(", ", changes)}",
                    targetId: client.ClientID
                );
            }
            // ⬆️⬆️⬆️ END AUDIT LOG ⬆️⬆️⬆️

            return RedirectToAction("CustomerList");
        }

        // GET: /Sales/ViewInvoicePartial/5
        public async Task<IActionResult> ViewInvoicePartial(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.JobOrder)
                    .ThenInclude(j => j.Technician)
                .Include(i => i.JobOrder)
                    .ThenInclude(j => j.JobParts)
                        .ThenInclude(p => p.Inventory)
                .FirstOrDefaultAsync(i => i.InvoiceID == id);

            if (invoice == null)
                return NotFound();

            return PartialView("_ViewInvoicePartial", invoice);
        }

        // GET: /Sales/ViewJobOrderPartial/5
        public async Task<IActionResult> ViewJobOrderPartial(int id)
        {
            var job = await _context.JobOrders
                .Include(j => j.Client)
                .Include(j => j.Technician)
                .Include(j => j.JobParts)
                    .ThenInclude(p => p.Inventory)
                .FirstOrDefaultAsync(j => j.JobID == id);

            if (job == null)
                return PartialView("_ViewJobOrderPartial", null);

            return PartialView("_ViewJobOrderPartial", job);
        }

        // ⬇️⬇️⬇️ ADD THIS HELPER METHOD ⬇️⬇️⬇️
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            var email = User.Identity?.Name;
            if (!string.IsNullOrEmpty(email))
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == email);
                if (user != null)
                {
                    return user.UserID;
                }
            }

            return 0;
        }
        // ⬆️⬆️⬆️ END HELPER METHOD ⬆️⬆️⬆️
    }
}