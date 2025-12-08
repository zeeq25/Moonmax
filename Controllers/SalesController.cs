using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moonmax.Data;
using Moonmax.Models;
using Moonmax.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Moonmax.Controllers
{
    public class SalesController : Controller
    {
        private readonly AppDbContext _context;

        public SalesController(AppDbContext context)
        {
            _context = context;
        }


        // GET: Sales/CreateInvoice/5
        public async Task<IActionResult> CreateInvoice(int jobId)
        {
            // Fetch the job including its parts
            var jobOrder = await _context.JobOrders
                .Include(j => j.JobParts)
                .Include(j => j.Client)
                .FirstOrDefaultAsync(j => j.JobID == jobId);

            if (jobOrder == null)
            {
                return NotFound();
            }

            // Calculate total cost: JobOrder + all parts
            decimal totalPartsCost = jobOrder.JobParts.Sum(p => p.TotalCost);
            decimal totalInvoiceAmount = jobOrder.Cost + totalPartsCost;

            // Generate a new invoice number (simple example, you can customize)
            string invoiceNumber = $"INV-{DateTime.Now:yyyyMMddHHmmss}";

            // Create invoice
            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                ClientID = jobOrder.ClientID ?? 0, // handle walk-in jobs
                JobID = jobOrder.JobID,
                Amount = totalInvoiceAmount,
                PaymentType = jobOrder.Client?.PaymentType ?? "Cash", // use client's payment type
                DateIssued = DateTime.Now,
                DueDate = jobOrder.DueDate,
                Status = "Pending"
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            // Redirect to invoice list or details page
            return RedirectToAction("Index", "Sales");
        }

        // GET: Sales
        public async Task<IActionResult> Index()
        {
            // Completed invoices
            var invoices = await _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.JobOrder)
                .ThenInclude(j => j.JobParts)
                .Select(i => new InvoiceViewModel
                {
                    InvoiceID = i.InvoiceID,
                    InvoiceNumber = i.InvoiceNumber,
                    ClientName = i.Client.Name,
                    JobID = i.JobID,
                    Amount = i.Amount,
                    PaymentType = i.PaymentType,
                    DateIssued = i.DateIssued,
                    DueDate = i.DueDate,
                    Status = i.Status
                })
                .ToListAsync();

            // Jobs that are in progress but don't have an invoice yet
            var inProgressJobs = await _context.JobOrders
                .Include(j => j.Client)
                .Where(j => j.Status == "In Progress")
                .Where(j => !_context.Invoices.Any(inv => inv.JobID == j.JobID))
                .Select(j => new JobOrderListingVM
                {
                    JobID = j.JobID,
                    Client = j.ClientID == null ? $"Walk-In ({j.ContactNumber})" : j.Client.Name,
                    ServiceType = j.ServiceType,
                    Cost = j.Cost
                })
                .ToListAsync();

            var vm = new SalesIndexViewModel
            {
                Invoices = invoices,
                InProgressJobs = inProgressJobs
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
                    TotalRevenue = c.Invoices.Sum(i => i.Amount),
                    Outstanding = c.Invoices.Where(i => i.Status != "Paid").Sum(i => i.Amount)
                })
                .ToListAsync();

            return View(clients);
        }


        // GET: Sales/CreateCustomer
        public IActionResult CreateCustomer()
        {
            // Payment type options
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

            return RedirectToAction("CustomerList");
        }

        // POST: Sales/ReceivePayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReceivePayment(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.JobOrder) // make sure JobOrder is included
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

                        int newQty = inventoryItem.QuantityInStock;

                        // Get logged in user ID
                        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                        int parsedUserId = int.Parse(userId);

                        // CREATE STOCK MOVEMENT RECORD (OUT)
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

                // Mark the JobOrder as Completed
                invoice.JobOrder.Status = "Completed";
            }

            // Save everything in one call
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }











    }
}
