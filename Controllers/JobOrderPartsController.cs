using Microsoft.AspNetCore.Mvc;
using Moonmax.Data;
using Moonmax.Models;

namespace Moonmax.Controllers
{
    public class JobOrderPartsController : Controller
    {
        private readonly AppDbContext _context;

        public JobOrderPartsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: Add Part
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int jobId, string partName, string category, int quantity, decimal cost)
        {
            if (string.IsNullOrEmpty(partName) || string.IsNullOrEmpty(category) || quantity <= 0 || cost <= 0)
            {
                TempData["Error"] = "Please fill in all fields correctly.";
                return RedirectToAction("Index", "JobOrders");
            }

            var part = new JobOrderPart
            {
                JobID = jobId,
                PartName = partName,
                Category = category,
                Quantity = quantity,
                Cost = cost
            };

            _context.JobOrderParts.Add(part);
            _context.SaveChanges();

            TempData["Success"] = "Part added successfully!";
            return RedirectToAction("Index", "JobOrders");
        }

        // Optional: List Parts for a Job Order
        public IActionResult List(int jobId)
        {
            var parts = _context.JobOrderParts.Where(p => p.JobID == jobId).ToList();
            return PartialView("_JobOrderPartsList", parts);
        }
    }
}
