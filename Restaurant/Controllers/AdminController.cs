using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTAURANT.Data;

namespace Restaurant.Controllers
{
    [Authorize(Roles = "Administrador, Cajero, Mesero, Cliente")]
    public class AdminController : Controller
    {
        
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
