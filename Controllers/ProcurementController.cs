using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moonmax.Data;
using Moonmax.Models;
using Moonmax.ViewModels;
using System.Linq;

namespace Moonmax.Controllers
{
    [Authorize]
    public class ProcurementController : Controller
    {
        private readonly AppDbContext _db;

        public ProcurementController(AppDbContext context)
        {
            _db = context;
        }

        public IActionResult Index()
        {
            // Fetch purchase orders with related supplier and items
            var pos = _db.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Items)
                .ToList();

            return View(pos); // Pass the list to the view
        }

        // ============================
        // Index (Suppliers Tab)
        // ============================

        public IActionResult Suppliers()
        {
            var suppliers = _db.Suppliers
                .Select(s => new SupplierViewModel
                {
                    SupplierID = s.SupplierID,
                    SupplierName = s.SupplierName,
                    ContactNumber = s.ContactNumber,
                    Email = s.Email,
                    ProductLine = s.ProductLine,
                    PaymentTerms = s.PaymentTerms,
                    Status = s.Status
                }).ToList();

            return View(suppliers);
        }

       

        // ============================
        // GET: Create Supplier
        // ============================
        [HttpGet]
        public IActionResult CreateSupplier()
        {
            return View(new CreateSupplierViewModel());
        }

        // ============================
        // POST: Create Supplier
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSupplier(CreateSupplierViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var supplier = new Supplier
            {
                SupplierName = model.SupplierName,
                ContactNumber = model.ContactNumber,
                Email = model.Email,
                ProductLine = model.ProductLine,
                PaymentTerms = model.PaymentTerms,
                Status = model.Status
            };

            _db.Suppliers.Add(supplier);
            _db.SaveChanges();

            return RedirectToAction("Suppliers");
        }



        // GET: Procurement/CreatePurchaseOrder
        [HttpGet]
        public IActionResult CreatePurchaseOrder()
        {
            var vm = new CreatePurchaseOrderViewModel
            {
                Suppliers = _db.Suppliers
                                    .Where(s => s.Status == "Active")
                                    .ToList()
            };

            return View("~/Views/Procurement/CreatePurchaseOrder.cshtml", vm);
        }

        // POST: Procurement/CreatePurchaseOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePurchaseOrder(CreatePurchaseOrderViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Suppliers = _db.Suppliers
                                        .Where(s => s.Status == "Active")
                                        .ToList();
                return View("~/Views/Procurement/CreatePurchaseOrder.cshtml", vm);
            }

            // Create the PurchaseOrder entity
            var po = new PurchaseOrders
            {
                SupplierID = vm.SupplierID,
                DateCreated = DateTime.Now,
                ExpectedDelivery = vm.ExpectedDelivery,
                Status = vm.Status
            };

            _db.PurchaseOrders.Add(po);
            _db.SaveChanges(); // Save first to get the generated PurchaseOrderID

            // Add the items using fully qualified class name to avoid ambiguity
            foreach (var item in vm.Items)
            {
                _db.PurchaseOrderItems.Add(new Moonmax.Models.PurchaseOrderItem
                {
                    PurchaseOrderID = po.PurchaseOrderID,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Category = item.Category
                });
            }

            _db.SaveChanges();

            // Redirect to Index of Procurement (your Purchase Orders list)
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeactivateSupplier(int id)
        {
            var supplier = await _db.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();

            supplier.Status = "Inactive";
            supplier.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Supplier deactivated successfully.";
            return RedirectToAction("Suppliers");
        }

        [HttpPost]
        public async Task<IActionResult> ActivateSupplier(int id)
        {
            var supplier = await _db.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();

            supplier.Status = "Active";
            supplier.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Supplier activated successfully.";
            return RedirectToAction("Suppliers");
        }

        // GET: display edit form
        public async Task<IActionResult> EditSupplier(int id)
        {
            var supplier = await _db.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();

            var vm = new EditSupplierViewModel
            {
                SupplierID = supplier.SupplierID,
                SupplierName = supplier.SupplierName,
                ContactNumber = supplier.ContactNumber,
                Email = supplier.Email,
                ProductLine = supplier.ProductLine,
                PaymentTerms = supplier.PaymentTerms,
                Status = supplier.Status
            };

            return View(vm);
        }

        // POST: save changes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSupplier(EditSupplierViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var supplier = await _db.Suppliers.FindAsync(vm.SupplierID);
            if (supplier == null) return NotFound();

            supplier.SupplierName = vm.SupplierName;
            supplier.ContactNumber = vm.ContactNumber;
            supplier.Email = vm.Email;
            supplier.ProductLine = vm.ProductLine;
            supplier.PaymentTerms = vm.PaymentTerms;
            supplier.Status = vm.Status;

            _db.Update(supplier);
            await _db.SaveChangesAsync();

            return RedirectToAction("Suppliers");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Receive(int id)
        {
            // Get the purchase order and its items
            var po = await _db.PurchaseOrders
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.PurchaseOrderID == id);

            if (po == null)
                return NotFound();

            // Update PO status to "Delivered"
            po.Status = "Delivered";

            // Loop through each item in the PO
            foreach (var item in po.Items)
            {
                // Find inventory by PartName
                var inventoryItem = await _db.Inventories
                    .FirstOrDefaultAsync(i => i.PartName == item.ProductName);

                if (inventoryItem != null)
                {
                    // Increment existing stock
                    inventoryItem.QuantityInStock += item.Quantity;
                }
                else
                {
                    // Add new inventory record if it doesn't exist
                    var newInventory = new Inventory
                    {
                        PartName = item.ProductName,
                        QuantityInStock = item.Quantity,
                        UnitCost = item.UnitPrice,
                        Category = item.Category
                    };
                    _db.Inventories.Add(newInventory);
                }
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Purchase Order #{po.PurchaseOrderID} received successfully!";
            return RedirectToAction(nameof(Index));
        }

    }
}
