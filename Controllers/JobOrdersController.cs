using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Moonmax.Data;
using Moonmax.Models;
using Moonmax.ViewModels;
using Moonmax.Services;  // ⬅️ ADD THIS
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;  // ⬅️ ADD THIS

namespace Moonmax.Controllers
{
    [Authorize(Roles ="Admin, Employee")]
    public class JobOrdersController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IAuditService _auditService;  // ⬅️ ADD THIS

        // ⬅️ UPDATE CONSTRUCTOR
        public JobOrdersController(AppDbContext db, IAuditService auditService)
        {
            _db = db;
            _auditService = auditService;  // ⬅️ ADD THIS
        }

        // INDEX
        public async Task<IActionResult> Index(string searchTerm = "", string statusFilter = "all")
        {
            var query = _db.JobOrders
                .Include(j => j.Client)
                .Include(j => j.Technician)
                .AsQueryable();

            // Apply search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(j =>
                    j.JobID.ToString().Contains(searchTerm) ||
                    (j.Client != null && j.Client.Name.ToLower().Contains(searchTerm)) ||
                    j.ServiceType.ToLower().Contains(searchTerm));
            }

            // Apply status filter
            if (statusFilter != "all")
            {
                var statusMap = new Dictionary<string, string>
                {
                    { "pending","Pending" },
                    { "in-progress", "In Progress" },
                    { "completed", "Completed" }
                };
                if (statusMap.ContainsKey(statusFilter))
                {
                    var statusValue = statusMap[statusFilter];
                    query = query.Where(j => j.Status == statusValue);
                }
            }

            var jobOrders = await query
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            var vm = new JobOrdersIndexViewModel
            {
                PendingJobs = await _db.JobOrders.CountAsync(j => j.Status == "Pending"),
                InProgressJobs = await _db.JobOrders.CountAsync(j => j.Status == "In Progress"),
                CompletedJobs = await _db.JobOrders.CountAsync(j => j.Status == "Completed"),

                TotalRevenue = await _db.JobOrders
                    .Where(j => j.Status == "Completed")
                    .SumAsync(j => j.Cost),

                JobOrders = jobOrders.Select(j => new JobOrderListingVM
                {
                    JobID = j.JobID,
                    Client = j.ClientID == null ? $"Walk-In ({j.ContactNumber})" : j.Client!.Name,
                    ServiceType = j.ServiceType,
                    Technician = j.Technician?.Name ?? "",
                    Created = j.CreatedAt,
                    DueDate = j.DueDate,
                    Cost = j.Cost,
                    Status = j.Status,
                    TechnicianID = j.TechnicianID,
                    HasInvoice = _db.Invoices.Any(i => i.JobID == j.JobID) // ⬅️ ADD THIS


                })
                // ⬇️⬇️⬇️ CORRECTED: OrderByDescending on the COLLECTION ⬇️⬇️⬇️
                .OrderByDescending(j => j.Status == "In Progress" && !j.HasInvoice)
                .ThenByDescending(j => j.Status == "Pending")
                .ThenByDescending(j => j.Created)
                // ⬆️⬆️⬆️ END SORTING ⬆️⬆️⬆️
                .ToList()



            };

            return View(vm);
        }

        // CREATE - GET
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new CreateJobOrderViewModel
            {
                Technicians = _db.Technician
                    .Select(t => new SelectListItem { Value = t.TechnicianID.ToString(), Text = t.Name })
                    .ToList(),

                Clients = _db.Client
                    .Select(c => new SelectListItem { Value = c.ClientID.ToString(), Text = c.Name })
                    .ToList(),
            
                ServiceTypes = new List<SelectListItem>
        {
                new SelectListItem { Value = "Hydraulic Hose Fabrication", Text = "Hydraulic Hose Fabrication" },
                new SelectListItem { Value = "Turbo Repair", Text = "Turbo Repair" },
                new SelectListItem { Value = "Metal Fabrication", Text = "Metal Fabrication" },
                new SelectListItem { Value = "Hydraulic Pump Repair", Text = "Hydraulic Pump Repair" },
                new SelectListItem { Value = "Machining Job", Text = "Machining Job" },
                
        }
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateJobOrderViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Technicians = _db.Technician
                    .Select(t => new SelectListItem { Value = t.TechnicianID.ToString(), Text = t.Name })
                    .ToList();

                vm.Clients = _db.Client
                    .Select(c => new SelectListItem { Value = c.ClientID.ToString(), Text = c.Name })
                    .ToList();

                PopulateDropdowns(vm);
                return View(vm);
            }

            int clientId;

            

            // Handle Walk-In customer (when ClientID is null or 0 in the ViewModel)
            if (!vm.ClientID.HasValue || vm.ClientID.Value <= 0)
            {
                // Find the generic "Walk-In Customer" client
                var walkInClient = await _db.Client
                    .FirstOrDefaultAsync(c => c.Name == "Walk-In Customer" && c.ClientType == "Walk-In");

                if (walkInClient == null)
                {
                    // Create the generic Walk-In customer if it doesn't exist
                    walkInClient = new Client
                    {
                        Name = "Walk-In Customer",
                        ContactNumber = "N/A",
                        ClientType = "Walk-In",
                        CreatedAt = DateTime.Now,
                        PaymentType = "Cash"
                    };

                    _db.Client.Add(walkInClient);
                    await _db.SaveChangesAsync();
                }

                clientId = walkInClient.ClientID;
            }
            else
            {
                clientId = vm.ClientID.Value;
            }

            // ✅ Create Job Order
            var jobOrder = new JobOrder
            {
                ClientID = clientId,
                ContactNumber = vm.ContactNumber ?? "N/A", // Store the specific contact number
                TechnicianID = vm.TechnicianID,
                ServiceType = vm.ServiceType,
                CreatedAt = DateTime.Now,
                DueDate = vm.DueDate,
                Cost = vm.Cost,
                Status = "Pending"
            };

            _db.JobOrders.Add(jobOrder);
            await _db.SaveChangesAsync();

            // ✅ Audit log
            var client = await _db.Client.FindAsync(clientId);
            string clientName = client?.ClientType == "Walk-In"
                ? $"Walk-In ({vm.ContactNumber})"
                : client?.Name ?? "Unknown";

            var technician = await _db.Technician.FindAsync(vm.TechnicianID);
            string technicianName = technician?.Name ?? "Unassigned";

            await _auditService.LogAsync(
                userId: GetCurrentUserId(),
                action: "CREATE",
                module: "Job Orders",
                description: $"Created Job Order #{jobOrder.JobID} for {clientName} - Service: {jobOrder.ServiceType}, Technician: {technicianName}, Cost: ₱{jobOrder.Cost:N2}",
                targetId: jobOrder.JobID
            );

            TempData["Success"] = "Job Order created successfully!";
            return RedirectToAction(nameof(Index));
        }



        private void PopulateDropdowns(CreateJobOrderViewModel vm)
        {
            vm.Technicians = _db.Technician
                .Select(t => new SelectListItem { Value = t.TechnicianID.ToString(), Text = t.Name })
                .ToList();

            vm.Clients = _db.Client
                .Select(c => new SelectListItem { Value = c.ClientID.ToString(), Text = c.Name })
                .ToList();

            vm.ServiceTypes = new List<SelectListItem>
    {
                new SelectListItem { Value = "Hydraulic Hose Fabrication", Text = "Hydraulic Hose Fabrication" },
                new SelectListItem { Value = "Turbo Repair", Text = "Turbo Repair" },
                new SelectListItem { Value = "Metal Fabrication", Text = "Metal Fabrication" },
                new SelectListItem { Value = "Hydraulic Pump Repair", Text = "Hydraulic Pump Repair" },
                new SelectListItem { Value = "Machining Job", Text = "Machining Job" },
    };

             vm.StatusList = new List<SelectListItem>
            {
            new SelectListItem { Value = "Pending", Text = "Pending" },
            new SelectListItem { Value = "In Progress", Text = "In Progress" }
            };
            }

            // ========================================
            // COMPLETE FIXED ADDPARTS METHODS
            // ========================================

            // ADD PARTS GET
            [HttpGet]
            public async Task<IActionResult> AddParts(int id)
            {
                var job = await _db.JobOrders
                    .Include(j => j.Client)
                    .FirstOrDefaultAsync(j => j.JobID == id);

                if (job == null) return NotFound();

                var parts = await _db.JobParts
                    .Include(p => p.Inventory)
                    .Where(p => p.JobID == id)
                    .ToListAsync();



                // Get ALL distinct categories from inventory (ignore stock for category dropdown)
                var categories = await _db.Inventories
                    .Where(i => !string.IsNullOrWhiteSpace(i.Category))
                    .Select(i => i.Category.Trim())
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                var vm = new AddPartsViewModel
                {
                    JobID = job.JobID,
                    JobClient = job.ClientID == null ? $"Walk-In ({job.ContactNumber})" : job.Client!.Name,
                    ServiceType = job.ServiceType ?? "",
                    JobStatus = job.Status, // ⬅️ ADDED THIS
                    Parts = parts.Select(p => new JobPartListingVM
                    {
                        PartID = p.PartID,
                        PartName = p.Inventory.PartName,
                        Quantity = p.Quantity,
                        UnitCost = p.UnitCost,
                        TotalCost = p.TotalCost
                    }).ToList(),

                    // Populate categories - make sure there's at least a placeholder if empty
                    Categories = categories.Any()
                    ? categories.Select(c => new SelectListItem { Value = c, Text = c }).ToList()
                    : new List<SelectListItem>
                    {
                new SelectListItem { Value = "", Text = "No categories available", Disabled = true }
                    },


                    InventoryItems = await _db.Inventories
                        .Where(i => (i.QuantityInStock - i.ReservedQuantity) > 0) // Only show available items
                        .Select(i => new SelectListItem
                        {
                            Value = i.InventoryID.ToString(),
                            Text = $"{i.PartName} (Available: {i.QuantityInStock - i.ReservedQuantity})"
                        })
                        .ToListAsync()
                };



                return View(vm);
            }

            // ADD PARTS POST - PRODUCTION VERSION
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> AddParts(AddPartsViewModel vm)
            {
                // Validate basic requirements
                if (vm.InventoryID <= 0)
                {
                    ModelState.AddModelError("InventoryID", "Please select a part");
                }

                if (vm.Quantity <= 0)
                {
                    ModelState.AddModelError("Quantity", "Quantity must be greater than 0");
                }

                if (!ModelState.IsValid)
                {
                    TempData["AddPartsError"] = "Please fill in all required fields correctly.";
                    await ReloadInventoryDropdownAndParts(vm);
                    return View(vm);
                }

                // Get inventory item
                var inventoryItem = await _db.Inventories
                    .FirstOrDefaultAsync(i => i.InventoryID == vm.InventoryID);

                if (inventoryItem == null)
                {
                    TempData["AddPartsError"] = "Selected part not found in inventory.";
                    await ReloadInventoryDropdownAndParts(vm);
                    return View(vm);
                }

                // Calculate available stock (actual stock minus reserved)
                var availableStock = inventoryItem.QuantityInStock - inventoryItem.ReservedQuantity;

                // Check if we have enough stock
                if (vm.Quantity > availableStock)
                {
                    TempData["AddPartsError"] = $"Cannot add {vm.Quantity} units of {inventoryItem.PartName}. Only {availableStock} units available (Total Stock: {inventoryItem.QuantityInStock}, Reserved: {inventoryItem.ReservedQuantity}).";
                    await ReloadInventoryDropdownAndParts(vm);
                    return View(vm);
                }

                // Check reorder level (BLOCKING - must maintain minimum stock)
                const int reorderLevel = 10;
                var stockAfterAddition = availableStock - vm.Quantity;

                if (stockAfterAddition < reorderLevel)
                {
                    TempData["AddPartsError"] = $"❌ Cannot add {vm.Quantity} units of {inventoryItem.PartName}. This would bring stock below minimum level. Available: {availableStock}, After addition: {stockAfterAddition}, Minimum required: {reorderLevel}. Maximum you can add: {availableStock - reorderLevel} units.";
                    await ReloadInventoryDropdownAndParts(vm);
                    return View(vm);
                }

                try
                {
                    // Create the job part
                    var part = new JobPart
                    {
                        JobID = vm.JobID,
                        InventoryID = vm.InventoryID,
                        Quantity = vm.Quantity,
                        UnitCost = inventoryItem.UnitCost
                    };

                    _db.JobParts.Add(part);
                    await _db.SaveChangesAsync();

                    // Show success message (or warning if stock is low)
                    if (string.IsNullOrEmpty(TempData["AddPartsWarning"] as string))
                    {
                        TempData["AddPartsSuccess"] = $"Successfully added {vm.Quantity} units of {inventoryItem.PartName}!";
                    }

                    return RedirectToAction(nameof(AddParts), new { id = vm.JobID });
                }
                catch (Exception ex)
                {
                    TempData["AddPartsError"] = $"Error adding part: {ex.Message}";
                    await ReloadInventoryDropdownAndParts(vm);
                    return View(vm);
                }
            }
        

        // RELOAD HELPER
        private async Task ReloadInventoryDropdownAndParts(AddPartsViewModel vm)
        {
            var job = await _db.JobOrders
                .Include(j => j.Client)
                .FirstOrDefaultAsync(j => j.JobID == vm.JobID);

            if (job != null)
            {
                vm.JobStatus = job.Status;
                vm.JobClient = job.ClientID == null
                    ? $"Walk-In ({job.ContactNumber})"
                    : job.Client?.Name ?? "Unknown";
                vm.ServiceType = job.ServiceType ?? "";
            }

            vm.InventoryItems = await _db.Inventories
                .Where(i => (i.QuantityInStock - i.ReservedQuantity) > 0)
                .Select(i => new SelectListItem
                {
                    Value = i.InventoryID.ToString(),
                    Text = $"{i.PartName} (Available: {i.QuantityInStock - i.ReservedQuantity})"
                })
                .ToListAsync();

            vm.Parts = await _db.JobParts
                .Include(p => p.Inventory)
                .Where(p => p.JobID == vm.JobID)
                .Select(p => new JobPartListingVM
                {
                    PartID = p.PartID,
                    PartName = p.Inventory.PartName,
                    Quantity = p.Quantity,
                    UnitCost = p.UnitCost,
                    TotalCost = p.TotalCost
                })
                .ToListAsync();

            var categories = await _db.Inventories
    .Where(i => !string.IsNullOrWhiteSpace(i.Category))
    .Select(i => i.Category.Trim())
    .Distinct()
    .OrderBy(c => c)
    .ToListAsync();

            vm.Categories = categories.Any()
                ? categories.Select(c => new SelectListItem
                {
                    Value = c,
                    Text = c
                }).ToList()
                : new List<SelectListItem>
                {
        new SelectListItem
        {
            Value = "",
            Text = "No categories available",
            Disabled = true
        }
                };

        }

        // CONFIRM PARTS - ALLOW ZERO PARTS (Labor-Only Jobs)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmParts(int JobID)
        {
            var job = await _db.JobOrders.FindAsync(JobID);
            if (job == null) return NotFound();

            var parts = await _db.JobParts
                .Include(p => p.Inventory)
                .Where(p => p.JobID == JobID)
                .ToListAsync();

            // Check if already confirmed
            if (job.Status == "In Progress" || job.Status == "Completed")
            {
                TempData["AddPartsError"] = "Parts have already been confirmed for this job.";
                return RedirectToAction(nameof(AddParts), new { id = JobID });
            }

            // NEW: Allow confirmation even with no parts (labor-only jobs)
            if (!parts.Any())
            {
                // Just update status, no reservation needed
                job.Status = "In Progress";
                await _db.SaveChangesAsync();

                var clientNoparts = await _db.Client.FindAsync(job.ClientID);
                string clientNameNoParts = clientNoparts?.Name ?? $"Walk-In ({job.ContactNumber})";

                await _auditService.LogAsync(
                    userId: GetCurrentUserId(),
                    action: "UPDATE",
                    module: "Job Orders",
                    description: $"Confirmed Job #{job.JobID} ({clientNameNoParts}) - Labor only, no parts",
                    targetId: job.JobID
                );

                TempData["AddPartsSuccess"] = "✅ Job confirmed (labor only - no parts needed)";
                return RedirectToAction(nameof(AddParts), new { id = JobID });
            }

            // EXISTING LOGIC: If parts exist, validate and reserve stock
            var insufficientStockErrors = new List<string>();

            // Check stock for each unique inventory item
            foreach (var group in parts.GroupBy(p => p.InventoryID))
            {
                var inventoryItem = group.First().Inventory;
                var totalQuantityNeeded = group.Sum(p => p.Quantity);
                var availableStock = inventoryItem.QuantityInStock - inventoryItem.ReservedQuantity;

                // Critical: Check if we have enough stock
                if (totalQuantityNeeded > availableStock)
                {
                    insufficientStockErrors.Add($"{inventoryItem.PartName} (Need: {totalQuantityNeeded}, Available: {availableStock})");
                }
            }

            // Only block if insufficient stock
            if (insufficientStockErrors.Any())
            {
                TempData["AddPartsError"] = "❌ Cannot confirm parts - Insufficient stock: " + string.Join(", ", insufficientStockErrors);
                return RedirectToAction(nameof(AddParts), new { id = JobID });
            }

            // Reserve the stock (OVERBOOKING PROTECTION INTACT)
            foreach (var group in parts.GroupBy(p => p.InventoryID))
            {
                var inventoryItem = group.First().Inventory;
                var totalQuantityNeeded = group.Sum(p => p.Quantity);
                inventoryItem.ReservedQuantity += totalQuantityNeeded;
            }

            // Update job status to In Progress
            job.Status = "In Progress";
            await _db.SaveChangesAsync();

            // Audit log
            var client = await _db.Client.FindAsync(job.ClientID);
            string clientName = client?.Name ?? $"Walk-In ({job.ContactNumber})";

            await _auditService.LogAsync(
                userId: GetCurrentUserId(),
                action: "UPDATE",
                module: "Job Orders",
                description: $"Confirmed parts for Job #{job.JobID} ({clientName}) - Reserved {parts.Sum(p => p.Quantity)} items total",
                targetId: job.JobID
            );

            TempData["AddPartsSuccess"] = "✅ Parts confirmed and reserved successfully!";

            return RedirectToAction(nameof(AddParts), new { id = JobID });
        }

        // GET: JobOrders/Edit/5
        public IActionResult Edit(int id)
        {
            var jobOrder = _db.JobOrders
                .Include(j => j.Client)
                .Include(j => j.Technician)
                .FirstOrDefault(j => j.JobID == id);

            if (jobOrder == null)
                return NotFound();

            var vm = new EditJobOrderViewModel
            {
                JobID = jobOrder.JobID,
                ClientID = jobOrder.ClientID,
                ServiceType = jobOrder.ServiceType,
                TechnicianID = jobOrder.TechnicianID,
                Created = jobOrder.CreatedAt,
                DueDate = jobOrder.DueDate,
                Cost = jobOrder.Cost,
                Status = jobOrder.Status
            };

            vm.Clients = _db.Client
                .Select(c => new SelectListItem { Value = c.ClientID.ToString(), Text = c.Name })
                .ToList();

            vm.Technicians = _db.Technician
                .Select(t => new SelectListItem { Value = t.TechnicianID.ToString(), Text = t.Name })
                .ToList();

            vm.StatusList = new List<SelectListItem>();

            if (jobOrder.Status != "Completed")
            {
                vm.StatusList.Add(new SelectListItem { Value = "Pending", Text = "Pending" });
                vm.StatusList.Add(new SelectListItem { Value = "In Progress", Text = "In Progress" });
            }
            else
            {
                vm.StatusList.Add(new SelectListItem { Value = "Completed", Text = "Completed", Disabled = true });
            }

            return View(vm);
        }

        // POST: JobOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditJobOrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Clients = _db.Client.Select(c => new SelectListItem { Value = c.ClientID.ToString(), Text = c.Name }).ToList();
                model.Technicians = _db.Technician.Select(t => new SelectListItem { Value = t.TechnicianID.ToString(), Text = t.Name }).ToList();
                return View(model);
            }

            var jobOrder = _db.JobOrders.Find(model.JobID);
            if (jobOrder == null)
                return NotFound();

            // ⬇️ TRACK CHANGES FOR AUDIT
            string oldStatus = jobOrder.Status;
            decimal oldCost = jobOrder.Cost;

            // Update fields
            jobOrder.ClientID = model.ClientID;
            jobOrder.ServiceType = model.ServiceType;
            jobOrder.TechnicianID = model.TechnicianID;
            jobOrder.DueDate = model.DueDate;
            jobOrder.Cost = model.Cost;
            jobOrder.Status = model.Status;

            _db.SaveChanges();

            // ⬇️⬇️⬇️ ADD AUDIT LOG HERE ⬇️⬇️⬇️
            var changes = new List<string>();
            if (oldStatus != model.Status) changes.Add($"Status: {oldStatus} → {model.Status}");
            if (oldCost != model.Cost) changes.Add($"Cost: ₱{oldCost:N2} → ₱{model.Cost:N2}");

            if (changes.Any())
            {
                await _auditService.LogAsync(
                    userId: GetCurrentUserId(),
                    action: "UPDATE",
                    module: "Job Orders",
                    description: $"Updated Job Order #{jobOrder.JobID}: {string.Join(", ", changes)}",
                    targetId: jobOrder.JobID
                );
            }
            // ⬆️⬆️⬆️ END AUDIT LOG ⬆️⬆️⬆️

            return RedirectToAction(nameof(Index));
        }

        // GET: JobOrders/GetClientContact/5
        [HttpGet]
        public async Task<JsonResult> GetClientContact(int clientId)
        {
            var client = await _db.Client.FindAsync(clientId);
            if (client != null)
            {
                return Json(new { contactNumber = client.ContactNumber });
            }
            return Json(new { contactNumber = "" });
        }

        // REMOVE PART
        [HttpGet]
        public async Task<IActionResult> RemovePart(int id)
        {
            var part = await _db.JobParts
                .Include(p => p.Inventory)
                .Include(p => p.JobOrder)
                .FirstOrDefaultAsync(p => p.PartID == id);

            if (part == null)
                return NotFound();

            // Check if job is still pending (can't remove if confirmed)
            if (part.JobOrder.Status != "Pending")
            {
                TempData["AddPartsError"] = "Cannot remove parts after confirmation.";
                return RedirectToAction("AddParts", new { id = part.JobID });
            }

            int jobId = part.JobID;
            string partName = part.Inventory?.PartName ?? "Unknown Part";
            int quantity = part.Quantity;

            _db.JobParts.Remove(part);
            await _db.SaveChangesAsync();

            // Audit log
            await _auditService.LogAsync(
                userId: GetCurrentUserId(),
                action: "DELETE",
                module: "Job Orders",
                description: $"Removed part '{partName}' (Qty: {quantity}) from Job #{jobId}",
                targetId: jobId
            );

            TempData["AddPartsSuccess"] = $"Removed {quantity} units of {partName} successfully!";
            return RedirectToAction("AddParts", new { id = jobId });
        }

        // GET: /JobOrders/ViewJobOrderDetailsModal/5
        public async Task<IActionResult> ViewJobOrderDetailsModal(int id)
        {
            var job = await _db.JobOrders
                .Include(j => j.Client)
                .Include(j => j.Technician)
                .Include(j => j.JobParts)
                    .ThenInclude(p => p.Inventory)
                .FirstOrDefaultAsync(j => j.JobID == id);

            if (job == null)
                return PartialView("_JobOrderDetailsModal", null);

            var vm = new JobOrderDetailsViewModel
            {
                JobID = job.JobID,
                Client = job.Client != null ? job.Client.Name : $"Walk-In ({job.ContactNumber})",
                Technician = job.Technician?.Name ?? "-",
                ServiceType = job.ServiceType,
                Status = job.Status,
                Created = job.CreatedAt,
                DueDate = job.DueDate,
                Cost = job.Cost,
                Parts = job.JobParts.Select(p => new JobOrderPartViewModel
                {
                    PartName = p.Inventory?.PartName ?? "N/A",
                    Quantity = p.Quantity,
                    UnitCost = p.UnitCost
                }).ToList()
            };

            return PartialView("_JobOrderDetailsModal", vm);
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
                var user = _db.Users.FirstOrDefault(u => u.Email == email);
                if (user != null)
                {
                    return user.UserID;
                }
            }

            return 0;
        }


        [HttpGet]
        public async Task<IActionResult> CreateInvoice(int id)
        {
            try
            {
                // Step 1: Get the job order with all related data
                var jobOrder = await _db.JobOrders
                    .Include(j => j.JobParts)
                        .ThenInclude(p => p.Inventory)
                    .Include(j => j.Client)
                    .FirstOrDefaultAsync(j => j.JobID == id);

                if (jobOrder == null)
                {
                    TempData["Error"] = $"Job Order #{id} not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Step 2: Check if invoice already exists
                var existingInvoice = await _db.Invoices
                    .FirstOrDefaultAsync(inv => inv.JobID == id);

                if (existingInvoice != null)
                {
                    TempData["Error"] = $"Invoice already exists for this job: {existingInvoice.InvoiceNumber}";
                    return RedirectToAction(nameof(Index));
                }

                // Step 3: Check job status - must be "In Progress"
                if (jobOrder.Status != "In Progress")
                {
                    TempData["Error"] = $"Only jobs in progress can be invoiced. Current status: {jobOrder.Status}";
                    return RedirectToAction(nameof(Index));
                }

                // Step 4: Get ClientID
                int clientId = jobOrder.ClientID;

                // Step 5: Verify client exists
                var client = await _db.Client.FindAsync(clientId);
                if (client == null)
                {
                    TempData["Error"] = $"Client #{clientId} not found in database.";
                    return RedirectToAction(nameof(Index));
                }

                // Step 6: Calculate totals
                decimal totalPartsCost = jobOrder.JobParts?.Sum(p => p.TotalCost) ?? 0;
                decimal totalInvoiceAmount = jobOrder.Cost + totalPartsCost;

                // Step 7: Generate invoice number
                string invoiceNumber = $"INV-{DateTime.Now:yyyyMMddHHmmss}";

                // Step 8: Calculate due date based on payment type
                DateTime dateIssued = DateTime.Now;
                DateTime dueDate = dateIssued; // NOT NULLABLE - use DateTime instead of DateTime?
                string paymentType = !string.IsNullOrWhiteSpace(client.PaymentType)
                    ? client.PaymentType
                    : "Cash"; // ENSURE NOT NULL

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

                // Step 9: Create invoice - ENSURE ALL REQUIRED FIELDS ARE SET
                var invoice = new Invoice
                {
                    InvoiceNumber = invoiceNumber,
                    ClientID = clientId,
                    JobID = jobOrder.JobID,
                    Amount = totalInvoiceAmount,
                    PaymentType = paymentType, // GUARANTEED NOT NULL
                    DateIssued = dateIssued,
                    DueDate = dueDate, // NOT NULLABLE
                    Status = "Pending", // GUARANTEED NOT NULL
                    Payments = new List<Payment>() // INITIALIZE COLLECTION
                };

                _db.Invoices.Add(invoice);
                await _db.SaveChangesAsync();

                // Step 10: Audit log
                string clientName = client.ClientType == "Walk-In"
                    ? $"Walk-In ({jobOrder.ContactNumber})"
                    : client.Name;

                await _auditService.LogAsync(
                    userId: GetCurrentUserId(),
                    action: "CREATE",
                    module: "Invoices",
                    description: $"Created Invoice {invoiceNumber} for {clientName} - Job #{jobOrder.JobID}, Amount: ₱{totalInvoiceAmount:N2}, Payment: {paymentType}",
                    targetId: invoice.InvoiceID
                );

                TempData["Success"] = $"Invoice {invoiceNumber} created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Detailed error logging
                System.Diagnostics.Debug.WriteLine($"CreateInvoice Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                TempData["Error"] = $"Error creating invoice: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetPartsByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return Json(new List<object>());
            }

            var parts = await _db.Inventories
                .Where(i => i.Category.Trim() == category.Trim()
                         && (i.QuantityInStock - i.ReservedQuantity) > 0)
                .OrderBy(i => i.PartName)
                .Select(i => new
                {
                    value = i.InventoryID,
                    text = $"{i.PartName} (Available: {i.QuantityInStock - i.ReservedQuantity})"
                })
                .ToListAsync();

            return Json(parts);
        }
    }
}