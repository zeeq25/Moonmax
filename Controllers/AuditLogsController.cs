using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moonmax.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Moonmax.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : Controller
    {
        private readonly AppDbContext _context;

        public AuditLogsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AuditLogs
        public async Task<IActionResult> Index(string module = null)
        {
            var query = _context.AuditLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.Timestamp)
                .AsQueryable();

            // Filter by module if specified
            if (!string.IsNullOrEmpty(module))
            {
                query = query.Where(a => a.Module == module);
            }

            var logs = await query.Take(100).ToListAsync(); // Show last 100 logs

            ViewBag.CurrentModule = module;
            return View(logs);
        }
    }
}