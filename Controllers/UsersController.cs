using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moonmax.Data;
using Moonmax.Models;
using Moonmax.ViewModels;
using Moonmax.Services;  // ADD THIS
using System.Security.Claims;  // ADD THIS

namespace Moonmax.Controllers
{
    [Authorize]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAuditService _auditService;  // ADD THIS

        // UPDATE CONSTRUCTOR
        public UsersController(AppDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;  // ADD THIS
        }

        // LIST USERS
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            var technicians = await _context.Technician.ToListAsync();

            var vm = new UsersIndexViewModel
            {
                Users = users,
                Technicians = technicians,
                CreateModel = new CreateUserViewModel(),
            };

            return View(vm);
        }

        // CREATE USER (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind(Prefix = "CreateModel")] CreateUserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Attempt Failed. Check your input.";
                return RedirectToAction(nameof(Index));
            }

            var user = new User
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                Role = vm.Role,
                IsActive = vm.IsActive,
                CreatedAt = DateTime.Now
            };

            // Hash the password
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, vm.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // ===== ADD AUDIT LOG HERE =====
            await _auditService.LogAsync(
                userId: GetCurrentUserId(),
                action: "CREATE",
                module: "User Management",
                description: $"Created new user: {user.FirstName} {user.LastName} ({user.Email}) with role {user.Role}",
                targetId: user.UserID
            );
            // ===============================

            TempData["Success"] = "User added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // EDIT USER (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new EditUserViewModel
            {
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            };

            return View(vm);
        }

        // EDIT USER (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users.FindAsync(model.UserID);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");
            }

            // ===== TRACK CHANGES FOR AUDIT =====
            string oldEmail = user.Email;
            string oldRole = user.Role;
            bool oldIsActive = user.IsActive;
            // ====================================

            // Update fields
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.Role = model.Role;
            user.IsActive = model.IsActive;

            // ===== BUILD CHANGE DESCRIPTION =====
            var changes = new List<string>();
            if (oldEmail != model.Email) changes.Add($"Email: {oldEmail} → {model.Email}");
            if (oldRole != model.Role) changes.Add($"Role: {oldRole} → {model.Role}");
            if (oldIsActive != model.IsActive) changes.Add($"Status: {(oldIsActive ? "Active" : "Inactive")} → {(model.IsActive ? "Active" : "Inactive")}");
            // ====================================

            // Update password only if entered
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                var hasher = new PasswordHasher<User>();
                user.PasswordHash = hasher.HashPassword(user, model.Password);
                changes.Add("Password changed");  // ADD THIS
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // ===== ADD AUDIT LOG HERE =====
            string changeDescription = changes.Count > 0
                ? $"Updated user {user.FirstName} {user.LastName}: {string.Join(", ", changes)}"
                : $"Updated user {user.FirstName} {user.LastName}";

            await _auditService.LogAsync(
                userId: GetCurrentUserId(),
                action: "UPDATE",
                module: "User Management",
                description: changeDescription,
                targetId: user.UserID
            );
            // ==============================

            TempData["Success"] = "User updated successfully!";
            return RedirectToAction("Index");
        }

        // DELETE USER
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            // ===== STORE INFO BEFORE DELETION =====
            string userName = $"{user.FirstName} {user.LastName}";
            string userEmail = user.Email;
            // ======================================

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // ===== ADD AUDIT LOG HERE =====
            await _auditService.LogAsync(
                userId: GetCurrentUserId(),
                action: "DELETE",
                module: "User Management",
                description: $"Deleted user: {userName} ({userEmail})",
                targetId: id
            );
            // ==============================

            TempData["Success"] = "User deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // CREATE TECHNICIAN
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTechnician([Bind(Prefix = "CreateTechnicianModel")] CreateTechnicianViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Failed to add technician. Check your input.";
                return RedirectToAction(nameof(Index));
            }

            var tech = new Technicians
            {
                Name = vm.Name
            };

            _context.Technician.Add(tech);
            await _context.SaveChangesAsync();

            // ===== ADD AUDIT LOG HERE =====
            await _auditService.LogAsync(
                userId: GetCurrentUserId(),
                action: "CREATE",
                module: "User Management",
                description: $"Created new technician: {tech.Name}",
                targetId: tech.TechnicianID
            );
            // ==============================

            TempData["Success"] = "Technician added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ===== ADD THIS HELPER METHOD =====
        private int GetCurrentUserId()
        {
            // Get user ID from claims (ASP.NET Identity)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            // Fallback: try getting from email claim and lookup in database
            var email = User.Identity?.Name;
            if (!string.IsNullOrEmpty(email))
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == email);
                if (user != null)
                {
                    return user.UserID;
                }
            }

            // If all else fails, return 0 (system/unknown user)
            return 0;
        }
        // ==================================
    }
}