using Microsoft.AspNetCore.Mvc;

namespace Restaurant.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
