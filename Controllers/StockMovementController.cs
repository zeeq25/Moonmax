using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moonmax.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Moonmax.Controllers
{
    public class StockMovementController : Controller
    {
        private readonly AppDbContext _context;

        public StockMovementController(AppDbContext context)
        {
            _context = context;
        }

        // MAIN REPORT VIEW
        public async Task<IActionResult> Index(string filter, int? inventoryId)
        {
            var movements = _context.StockMovement
                .Include(s => s.Inventory)
                .Include(s => s.PurchaseOrder)
                .Include(s => s.JobOrder)
                .Include(s => s.User)
                .AsQueryable();

            // FILTER BY TYPE
            if (!string.IsNullOrEmpty(filter))
            {
                movements = movements.Where(m => m.MovementType == filter);
            }

            // FILTER BY INVENTORY ITEM
            if (inventoryId.HasValue)
            {
                movements = movements.Where(m => m.InventoryID == inventoryId.Value);
            }

            movements = movements.OrderByDescending(m => m.MovementDate);

            return View(await movements.ToListAsync());
        }

        // REPORT: PROCUREMENT RECEIVING
        public async Task<IActionResult> Received()
        {
            var data = await _context.StockMovement
                .Include(s => s.Inventory)
                .Include(s => s.PurchaseOrder)
                .Where(s => s.MovementType == "IN")
                .OrderByDescending(s => s.MovementDate)
                .ToListAsync();

            return View(data);
        }

        // REPORT: JOB ORDER MATERIAL USAGE
        public async Task<IActionResult> Used()
        {
            var data = await _context.StockMovement
                .Include(s => s.Inventory)
                .Include(s => s.JobOrder)
                .Where(s => s.MovementType == "OUT")
                .OrderByDescending(s => s.MovementDate)
                .ToListAsync();

            return View(data);
        }
    }
}
