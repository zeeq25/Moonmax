using Microsoft.AspNetCore.Mvc;

namespace Moonmax.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
