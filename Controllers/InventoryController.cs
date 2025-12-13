using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Moonmax.Data;
using Moonmax.Models;
using Moonmax.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Moonmax.Controllers
{

    [Authorize]
    public class InventoryController : Controller
    {
        private readonly AppDbContext _db;

        public InventoryController(AppDbContext db)
        {
            _db = db;
        }

        // ---------------------------
        // INDEX
        // ---------------------------
        public async Task<IActionResult> Index()
        {
            var items = await _db.Inventories
                .Include(i => i.Supplier)
                .OrderBy(i => i.PartName)
                .Select(i => new InventoryListingVM
                {
                    InventoryID = i.InventoryID,
                    Category = i.Category,
                    PartName = i.PartName,
                    UnitCost = i.UnitCost,
                    QuantityInStock = i.QuantityInStock,
                    SupplierName = i.Supplier.SupplierName
                })
                .ToListAsync();

            return View(items);
        }

        // ---------------------------
        // CREATE - GET
        // ---------------------------
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new InventoryFormVM
            {
                Suppliers = GetSuppliersSelectList(),
                Categories = GetCategoriesSelectList()
            };
            return View(vm);
        }

        // ---------------------------
        // CREATE - POST
        // ---------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryFormVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Suppliers = GetSuppliersSelectList();
                vm.Categories = GetCategoriesSelectList();
                return View(vm);
            }

            var inventory = new Inventory
            {
                Category = vm.Category,
                PartName = vm.PartName,
                UnitCost = vm.UnitCost,
                QuantityInStock = vm.QuantityInStock,
                ReorderLevel = vm.ReorderLevel,
                SupplierID = vm.SupplierID,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _db.Inventories.Add(inventory);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Inventory item added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------
        // EDIT - GET
        // ---------------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var inventory = await _db.Inventories.FindAsync(id);
            if (inventory == null) return NotFound();

            var vm = new InventoryFormVM
            {
                InventoryID = inventory.InventoryID,
                Category = inventory.Category,
                PartName = inventory.PartName,
                UnitCost = inventory.UnitCost,
                QuantityInStock = inventory.QuantityInStock,
                ReorderLevel = inventory.ReorderLevel,
                SupplierID = inventory.SupplierID,
                Suppliers = GetSuppliersSelectList(),
                Categories = GetCategoriesSelectList()
            };

            return View(vm);
        }

        // ---------------------------
        // EDIT - POST
        // ---------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InventoryFormVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Suppliers = GetSuppliersSelectList();
                vm.Categories = GetCategoriesSelectList();
                return View(vm);
            }

            var inventory = await _db.Inventories.FindAsync(vm.InventoryID);
            if (inventory == null) return NotFound();

            inventory.Category = vm.Category;
            inventory.PartName = vm.PartName;
            inventory.UnitCost = vm.UnitCost;
            inventory.QuantityInStock = vm.QuantityInStock;
            inventory.ReorderLevel = vm.ReorderLevel;
            inventory.SupplierID = vm.SupplierID;
            inventory.UpdatedAt = DateTime.Now;

            _db.Inventories.Update(inventory);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Inventory item updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------
        // DELETE
        // ---------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var inventory = await _db.Inventories.FindAsync(id);
            if (inventory == null) return NotFound();

            _db.Inventories.Remove(inventory);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Inventory item deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------
        // HELPER METHODS
        // ---------------------------
        private List<SelectListItem> GetSuppliersSelectList()
        {
            var suppliers = _db.Suppliers
                .Select(s => new SelectListItem
                {
                    Value = s.SupplierID.ToString(),
                    Text = s.SupplierName
                })
                .ToList();

            suppliers.Insert(0, new SelectListItem { Value = "", Text = "-- Select Supplier --" });
            return suppliers;
        }

        private List<SelectListItem> GetCategoriesSelectList()
        {
            // Example static categories, replace with DB if you have a table
            var categories = new List<string> { "Hoses", "Fittings", "Lubricants", "Tools","Supplies", "Others" };

            return categories.Select(c => new SelectListItem
            {
                Value = c,
                Text = c
            }).ToList();
        }

        // ---------------------------
        // STOCK ALERTS
        // ---------------------------
        public async Task<IActionResult> Stockalert()
        {
            var lowStockItems = await _db.Inventories
                .Where(i => i.QuantityInStock <= i.ReorderLevel)
                .Include(i => i.Supplier)
                .OrderBy(i => i.PartName)
                .Select(i => new InventoryListingVM
                {
                    InventoryID = i.InventoryID,
                    PartName = i.PartName,
                    Category = i.Category,
                    QuantityInStock = i.QuantityInStock,
                    UnitCost = i.UnitCost,
                    SupplierName = i.Supplier.SupplierName,
                    ReorderLevel = i.ReorderLevel
                })
                .ToListAsync();

            return View(lowStockItems);
        }


    }
}
