using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Employee")] // optional if using Claims-based authorization
public class EmployeeDashboardController : Controller
{


    // GET: EmployeeDashboard
    public IActionResult Index()
    {
        // Optional: check role from Claims to ensure employee access
        var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        if (role != "Employee")
        {
            return RedirectToAction("Index", "Dashboard"); // redirect to admin if not employee
        }

        // Set layout data
        ViewData["Title"] = "Employee Dashboard";
        ViewData["Nav"] = "Dashboard"; // highlights the active menu item
        ViewData["AppShell"] = true;   // ensures sidebar & topbar are rendered

        return View();
    }
}
