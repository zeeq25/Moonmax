using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Moonmax.Data;
using Moonmax.Models;
using Moonmax.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace Moonmax.Controllers
{
    public class JobOrdersController : Controller
    {
        private readonly AppDbContext _db;

        public JobOrdersController(AppDbContext db)
        {
            _db = db;
        }



        // INDEX
        public async Task<IActionResult> Index()
        {
            var jobOrders = await _db.JobOrders
                .Include(j => j.Client)
                .Include(j => j.Technician)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

            
           


            var vm = new JobOrdersIndexViewModel
            {
                PendingJobs = jobOrders.Count(j => j.Status == "Pending"),
                InProgressJobs = jobOrders.Count(j => j.Status == "In Progress"),
                CompletedJobs = jobOrders.Count(j => j.Status == "Completed"),
                TotalRevenue = jobOrders.Sum(j => j.Cost),
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
                Status = "Pending" // <-- force Pending
            };

            _db.JobOrders.Add(jobOrder);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Job Order created successfully!";
            return RedirectToAction(nameof(Index));
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

                return View(vm);
            }

            var inventoryItem = await _db.Inventories.FirstOrDefaultAsync(i => i.InventoryID == vm.InventoryID);
            if (inventoryItem == null) return BadRequest("Invalid inventory item.");

            var part = new JobPart
            {
                JobID = vm.JobID,
                InventoryID = vm.InventoryID,
                Quantity = vm.Quantity,
                UnitCost = inventoryItem.UnitCost
            };

            _db.JobParts.Add(part);
            await _db.SaveChangesAsync();

            // ✅ Update JobOrder status to "In Progress"
            var jobOrder = await _db.JobOrders.FindAsync(vm.JobID);
            if (jobOrder != null && jobOrder.Status == "Pending")
            {
                jobOrder.Status = "In Progress";
                await _db.SaveChangesAsync();
            }

            TempData["Success"] = "Part added successfully!";
            return RedirectToAction(nameof(AddParts), new { id = vm.JobID });
        }

        // GET: JobOrders/Edit/5
        public IActionResult Edit(int id)
        {
            var jobOrder = _db.JobOrders
                .Include(j => j.Client)    // if you want client info
                .Include(j => j.Technician) // optional
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

            // For dropdowns
            vm.Clients = _db.Client
                .Select(c => new SelectListItem { Value = c.ClientID.ToString(), Text = c.Name })
                .ToList();

            vm.Technicians = _db.Technician
                .Select(t => new SelectListItem { Value = t.TechnicianID.ToString(), Text = t.Name })
                .ToList();

            // Populate Status dropdown
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
        public IActionResult Edit(EditJobOrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload dropdowns
                model.Clients = _db.Client.Select(c => new SelectListItem { Value = c.ClientID.ToString(), Text = c.Name }).ToList();
                model.Technicians = _db.Technician.Select(t => new SelectListItem { Value = t.TechnicianID.ToString(), Text = t.Name }).ToList();
                return View(model);
            }

            var jobOrder = _db.JobOrders.Find(model.JobID);
            if (jobOrder == null)
                return NotFound();

            // Update fields
            jobOrder.ClientID = model.ClientID;
            jobOrder.ServiceType = model.ServiceType;
            jobOrder.TechnicianID = model.TechnicianID;
            jobOrder.DueDate = model.DueDate;
            jobOrder.Cost = model.Cost;
            jobOrder.Status = model.Status;

            _db.SaveChanges();

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
            // Find the part
            var part = await _db.JobParts.FindAsync(id);
            if (part == null)
                return NotFound();

            // Get JobID to redirect back
            int jobId = part.JobID;

            // Remove the part
            _db.JobParts.Remove(part);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Part removed successfully!";
            return RedirectToAction("AddParts", new { id = jobId });
        }


        [HttpGet]
        public IActionResult ViewJob(int id)
        {
            var job = _db.JobOrders
                .Include(j => j.Client)
                .Include(j => j.Technician)
                .Include(j => j.JobParts)
                    .ThenInclude(p => p.Inventory) // Assuming Inventory has PartName & UnitCost
                .Where(j => j.JobID == id)
                .AsEnumerable() // move to in-memory
                .Select(j => new JobOrderDetailsViewModel
                {
                    JobID = j.JobID,
                    Client = j.ClientID == null ? $"Walk-In ({j.ContactNumber})" : j.Client!.Name,
                    ServiceType = j.ServiceType,
                    Technician = j.Technician?.Name ?? "",
                    Created = j.CreatedAt,                // <-- use CreatedAt
                    DueDate = j.DueDate,
                    Cost = j.Cost,
                    Status = j.Status,
                    Parts = j.JobParts.Select(p => new JobOrderPartViewModel
                    {
                        PartName = p.Inventory.PartName,
                        Quantity = p.Quantity,
                        UnitCost = p.UnitCost
                    }).ToList()
                })
                .FirstOrDefault();

            if (job == null) return NotFound();

            return PartialView("_JobOrderDetailsModal", job);
        }




    }
}
