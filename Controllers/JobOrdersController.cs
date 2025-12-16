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
    [Authorize]
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
                    TechnicianID = j.TechnicianID
                }).ToList()
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

                StatusList = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Pending", Text = "Pending" }
                },

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

        // CREATE - POST
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

            var jobOrder = new JobOrder
            {
                ClientID = vm.ClientID,
                ContactNumber = vm.ContactNumber,
                TechnicianID = vm.TechnicianID,
                ServiceType = vm.ServiceType,
                CreatedAt = System.DateTime.Now,
                DueDate = vm.DueDate,
                Cost = vm.Cost,
                Status = "Pending"
            };

            _db.JobOrders.Add(jobOrder);
            await _db.SaveChangesAsync();

            // ⬇️⬇️⬇️ ADD AUDIT LOG HERE ⬇️⬇️⬇️
            // Get client name for better description
            var client = await _db.Client.FindAsync(vm.ClientID);
            string clientName = client != null ? client.Name : $"Walk-In ({vm.ContactNumber})";

            var technician = await _db.Technician.FindAsync(vm.TechnicianID);
            string technicianName = technician?.Name ?? "Unassigned";

            await _auditService.LogAsync(
                userId: GetCurrentUserId(),
                action: "CREATE",
                module: "Job Orders",
                description: $"Created Job Order #{jobOrder.JobID} for {clientName} - Service: {jobOrder.ServiceType}, Technician: {technicianName}, Cost: ₱{jobOrder.Cost:N2}",
                targetId: jobOrder.JobID
            );
            // ⬆️⬆️⬆️ END AUDIT LOG ⬆️⬆️⬆️

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

            var vm = new AddPartsViewModel
            {
                JobID = job.JobID,
                JobClient = job.ClientID == null ? $"Walk-In ({job.ContactNumber})" : job.Client!.Name,
                ServiceType = job.ServiceType ?? "",
                Parts = parts.Select(p => new JobPartListingVM
                {
                    PartID = p.PartID,
                    PartName = p.Inventory.PartName,
                    Quantity = p.Quantity,
                    UnitCost = p.UnitCost,
                    TotalCost = p.TotalCost
                }).ToList(),
                InventoryItems = await _db.Inventories
                    .Select(i => new SelectListItem { Value = i.InventoryID.ToString(), Text = i.PartName })
                    .ToListAsync()
            };

            return View(vm);
        }

        // ADD PARTS POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddParts(AddPartsViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await ReloadInventoryDropdownAndParts(vm);
                return View(vm);
            }

            var inventoryItem = await _db.Inventories.FirstOrDefaultAsync(i => i.InventoryID == vm.InventoryID);
            if (inventoryItem == null) return BadRequest("Invalid inventory item.");

            // Soft stock check for this addition
            if (vm.Quantity > inventoryItem.QuantityInStock)
            {
                TempData["Error"] = $"Insufficient stock for {inventoryItem.PartName}. " +
                                    $"Available: {inventoryItem.QuantityInStock}, Requested: {vm.Quantity}.";
                await ReloadInventoryDropdownAndParts(vm);
                return View(vm);
            }

            // Add part
            var part = new JobPart
            {
                JobID = vm.JobID,
                InventoryID = vm.InventoryID,
                Quantity = vm.Quantity,
                UnitCost = inventoryItem.UnitCost
            };

            _db.JobParts.Add(part);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Part added successfully!";
            return RedirectToAction(nameof(AddParts), new { id = vm.JobID });
        }

        // Helper method to reload inventory dropdown and parts table
        private async Task ReloadInventoryDropdownAndParts(AddPartsViewModel vm)
        {
            vm.InventoryItems = await _db.Inventories
                .Select(i => new SelectListItem { Value = i.InventoryID.ToString(), Text = i.PartName })
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
                }).ToListAsync();
        }

        // Confirm Parts POST
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

            const int reorderLevel = 10;
            var stockErrors = new List<string>();

            foreach (var group in parts.GroupBy(p => p.InventoryID))
            {
                var inventoryItem = group.First().Inventory;
                var totalQuantityForJob = group.Sum(p => p.Quantity);
                var availableStock = inventoryItem.QuantityInStock - inventoryItem.ReservedQuantity;

                if (availableStock - totalQuantityForJob < reorderLevel)
                {
                    stockErrors.Add(inventoryItem.PartName);
                }
            }

            if (stockErrors.Any())
            {
                TempData["Error"] = "Parts below minimum stock - " + string.Join(", ", stockErrors);
                return RedirectToAction(nameof(AddParts), new { id = JobID });
            }

            foreach (var group in parts.GroupBy(p => p.InventoryID))
            {
                var inventoryItem = group.First().Inventory;
                var totalQuantityForJob = group.Sum(p => p.Quantity);
                inventoryItem.ReservedQuantity += totalQuantityForJob;
            }
            await _db.SaveChangesAsync();

            if (job.Status == "Pending")
            {
                job.Status = "In Progress";
                await _db.SaveChangesAsync();
            }

            TempData["Success"] = "Parts confirmed successfully!";
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

        // GET: JobOrders/RemovePart/5
        [HttpGet]
        public async Task<IActionResult> RemovePart(int id)
        {
            var part = await _db.JobParts.FindAsync(id);
            if (part == null)
                return NotFound();

            int jobId = part.JobID;

            _db.JobParts.Remove(part);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Part removed successfully!";
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
        // ⬆️⬆️⬆️ END HELPER METHOD ⬆️⬆️⬆️
    }
}