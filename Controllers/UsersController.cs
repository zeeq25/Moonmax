using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moonmax.Data;
using Moonmax.Models;
using Moonmax.ViewModels;

namespace Moonmax.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // LIST USERS
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();

            var vm = new UsersIndexViewModel
            {
                Users = users,
                CreateModel = new CreateUserViewModel() // for modal binding
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

            // Update fields
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.Role = model.Role;
            user.IsActive = model.IsActive;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

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

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "User deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
