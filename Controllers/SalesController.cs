using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Moonmax.Data;
using Moonmax.Models;
using Moonmax.Services;  // ⬅️ ADD THIS
using Moonmax.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;  // ⬅️ ADD THIS
using System.Threading.Tasks;

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
            ViewBag.PaymentTypes = new[] { "Cash", "PDC" };
            return View();
        }

        // POST: Sales/CreateCustomer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCustomer(CustomerFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PaymentTypes = new[] { "Cash", "PDC" };
                return View(model);
            }

            // 🔒 Prevent duplicate Walk-In customer
            if (model.ClientType == "Walk-In")
            {
                bool walkInExists = await _context.Client
                    .AnyAsync(c => c.ClientType == "Walk-In");

                if (walkInExists)
                {
                    ModelState.AddModelError("ClientType", "Walk-In customer already exists.");
                    ViewBag.PaymentTypes = new[] { "Cash", "PDC" };
                    return View(model);
                }
            }

            var client = new Client
            {
                Name = model.Name,
                Email = model.Email,
                ContactNumber = model.Phone,
                PaymentType = model.PaymentType,
                ClientType = model.ClientType ?? "MainClient" // ✅ default safety
            };

            _context.Client.Add(client);
            await _context.SaveChangesAsync();

            // ⬇️⬇️⬇️ AUDIT LOG ⬇️⬇️⬇️
            await _auditService.LogAsync(
                userId: GetCurrentUserId(),
                action: "CREATE",
                module: "Sales & Billing",
                description: $"Created customer: {client.Name} | Type: {client.ClientType} | Payment: {client.PaymentType}",
                targetId: client.ClientID
            );
            // ⬆️⬆️⬆️ END AUDIT LOG ⬆️⬆️⬆️

            return RedirectToAction("CustomerList");
        }


        // GET: Sales/EditCustomer/5
        public async Task<IActionResult> EditCustomer(int id)
        {
            // First, fetch the client
            var client = await _context.Client.FindAsync(id);
            if (client == null)
                return NotFound();

            // Create the Customer Type select list
            var customerTypes = new List<SelectListItem>
    {
        new SelectListItem { Value = "MainClient", Text = "Main Client" },
        new SelectListItem
        {
            Value = "Walk-In",
            Text = "Walk-In (system reserved)",
            Disabled = client.ClientType != "Walk-In"
        }
    };

            // ✅ Pass the list to the view
            ViewBag.CustomerTypes = customerTypes;

            var model = new CustomerFormViewModel
            {
                ClientID = client.ClientID, // make sure ClientID is included for POST
                Name = client.Name,
                Email = client.Email,
                Phone = client.ContactNumber,
                PaymentType = client.PaymentType,
                ClientType = client.ClientType
            };

            ViewBag.PaymentTypes = new[] { "Cash", "PDC" };

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



        // POST: Sales/ProcessPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(PaymentFormViewModel model)
        {
            System.Diagnostics.Debug.WriteLine($"=== PAYMENT FORM SUBMITTED ===");
            System.Diagnostics.Debug.WriteLine($"InvoiceID: {model.InvoiceID}");
            System.Diagnostics.Debug.WriteLine($"PaymentMethod: {model.PaymentMethod}");
            System.Diagnostics.Debug.WriteLine($"AmountPaid: {model.AmountPaid}");
            System.Diagnostics.Debug.WriteLine($"PaymentDate: {model.PaymentDate}");
            System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Validation Error - {key}: {error.ErrorMessage}");
                    }
                }

                TempData["Error"] = "Invalid payment data. Please check all required fields.";
                return RedirectToAction("Index");
            }

            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.Client)
                    .Include(i => i.JobOrder)
                        .ThenInclude(j => j.JobParts)
                            .ThenInclude(p => p.Inventory) // ⬅️ IMPORTANT: Include Inventory
                    .Include(i => i.Payments)
                    .FirstOrDefaultAsync(i => i.InvoiceID == model.InvoiceID);

                if (invoice == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Invoice not found: {model.InvoiceID}");
                    TempData["Error"] = "Invoice not found.";
                    return RedirectToAction("Index");
                }

                System.Diagnostics.Debug.WriteLine($"Found invoice: {invoice.InvoiceNumber}");

                // VALIDATE: For Cash payments, don't require check fields
                if (model.PaymentMethod == "Cash")
                {
                    model.CheckNumber = null;
                    model.BankName = null;
                    model.CheckDate = null;
                }
                // VALIDATE: For Check payments, require check fields
                else if (model.PaymentMethod == "Check")
                {
                    if (string.IsNullOrWhiteSpace(model.CheckNumber) ||
                        string.IsNullOrWhiteSpace(model.BankName) ||
                        !model.CheckDate.HasValue)
                    {
                        TempData["Error"] = "For check payments, Check Number, Bank Name, and Check Date are required.";
                        return RedirectToAction("Index");
                    }
                }

                // Create Payment record
                var payment = new Payment
                {
                    InvoiceID = invoice.InvoiceID,
                    PaymentMethod = model.PaymentMethod,
                    AmountPaid = model.AmountPaid,
                    PaymentDate = model.PaymentDate,
                    CheckNumber = model.CheckNumber ?? "",
                    BankName = model.BankName ?? "",
                    CheckDate = model.CheckDate,
                    ReferenceNumber = model.ReferenceNumber ?? "",
                    Notes = model.Notes ?? "",
                    ProcessedByUserID = GetCurrentUserId(),
                    PDCStatus = model.PaymentMethod == "Check" ? "Received" : null,
                    BounceReason = "",
                    CreatedAt = DateTime.Now
                };

                _context.Payments.Add(payment);

                // Update Invoice Status
                var totalPaid = invoice.Payments.Sum(p => p.AmountPaid) + model.AmountPaid;

                string oldStatus = invoice.Status; // Track old status

                if (totalPaid >= invoice.Amount)
                {
                    invoice.Status = "Paid";

                    // ⬇️⬇️⬇️ ADD STOCK DEDUCTION WHEN FULLY PAID ⬇️⬇️⬇️
                    if (invoice.JobOrder != null && invoice.JobOrder.Status != "Completed")
                    {
                        System.Diagnostics.Debug.WriteLine($"Processing stock deduction for Job #{invoice.JobOrder.JobID}");

                        foreach (var part in invoice.JobOrder.JobParts)
                        {
                            if (part.Inventory != null)
                            {
                                int previousQty = part.Inventory.QuantityInStock;

                                // Deduct from reserved quantity
                                part.Inventory.ReservedQuantity -= part.Quantity;
                                if (part.Inventory.ReservedQuantity < 0)
                                    part.Inventory.ReservedQuantity = 0;

                                // Deduct from actual stock
                                part.Inventory.QuantityInStock -= part.Quantity;
                                if (part.Inventory.QuantityInStock < 0)
                                    part.Inventory.QuantityInStock = 0;

                                int newQty = part.Inventory.QuantityInStock;

                                System.Diagnostics.Debug.WriteLine($"Stock deduction: {part.Inventory.PartName} | Previous: {previousQty} | Deducted: {part.Quantity} | New: {newQty}");

                                // Create stock movement record
                                var movement = new StockMovement
                                {
                                    InventoryID = part.Inventory.InventoryID,
                                    MovementType = "OUT",
                                    Quantity = part.Quantity,
                                    PreviousQuantity = previousQty,
                                    NewQuantity = newQty,
                                    JobOrderID = invoice.JobID,
                                    UserID = GetCurrentUserId(),
                                    MovementDate = DateTime.Now
                                };

                                _context.StockMovement.Add(movement);
                            }
                        }

                        // Mark job as completed
                        invoice.JobOrder.Status = "Completed";

                        System.Diagnostics.Debug.WriteLine($"Job #{invoice.JobOrder.JobID} marked as Completed");
                    }
                    // ⬆️⬆️⬆️ END STOCK DEDUCTION ⬆️⬆️⬆️
                }
                else
                {
                    invoice.Status = "Partially Paid";
                }

                System.Diagnostics.Debug.WriteLine($"Saving payment... Total paid: {totalPaid}, Invoice amount: {invoice.Amount}");

                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine("Payment saved successfully!");

                // ⬇️⬇️⬇️ ENHANCED AUDIT LOG ⬇️⬇️⬇️
                string clientName = invoice.Client?.Name ?? $"Walk-In ({invoice.JobOrder?.ContactNumber})";
                int totalParts = invoice.JobOrder?.JobParts?.Count ?? 0;

                string auditDescription = $"Payment received for Invoice {invoice.InvoiceNumber} - Client: {clientName}, Amount: ₱{model.AmountPaid:N2}, Method: {model.PaymentMethod}";

                if (invoice.Status == "Paid" && oldStatus != "Paid")
                {
                    auditDescription += $", Job #{invoice.JobID} completed, {totalParts} parts deducted from inventory";
                }

                await _auditService.LogAsync(
                    userId: GetCurrentUserId(),
                    action: "PAYMENT_RECEIVED",
                    module: "Sales & Billing",
                    description: auditDescription,
                    targetId: invoice.InvoiceID
                );
                // ⬆️⬆️⬆️ END AUDIT LOG ⬆️⬆️⬆️

                TempData["Success"] = $"Payment of ₱{model.AmountPaid:N2} processed successfully!" +
                                     (invoice.Status == "Paid" ? " Invoice is now fully paid and job completed." : "");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack: {ex.StackTrace}");
                TempData["Error"] = $"Error processing payment: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }




    
}