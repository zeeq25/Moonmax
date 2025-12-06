using Microsoft.AspNetCore.Mvc;

namespace YourApp.Controllers   // ← change to your actual namespace
{
    public class SalesController : Controller
    {
        // /Sales or /Sales/Index
        public IActionResult Index()
        {
            return View();
        }

        // /Sales/Customerlist
        public IActionResult Customerlist()
        {
            return View();
        }
    }
}
