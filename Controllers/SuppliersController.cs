using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moonmax.Data;
using Moonmax.Models;

namespace Moonmax.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly AppDbContext _context;

        public SuppliersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Suppliers
        public async Task<IActionResult> Index()
        {
            var suppliers = await _context.Suppliers.ToListAsync();
            return View(suppliers);
        }

        // GET: Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                supplier.CreatedAt = DateTime.Now;
                supplier.UpdatedAt = DateTime.Now;

                _context.Add(supplier);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();

            return View(supplier);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                supplier.UpdatedAt = DateTime.Now;
                _context.Update(supplier);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        // GET: Details
        public async Task<IActionResult> Details(int id)
        {
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierID == id);

            if (supplier == null) return NotFound();

            return View(supplier);
        }

        // DELETE
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return Json(new { success = false });

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
