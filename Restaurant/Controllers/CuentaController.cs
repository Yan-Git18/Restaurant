using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Models;
using Restaurant.ViewModels;

namespace Restaurant.Controllers
{
    public class CuentaController : Controller
    {
        

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        

        public IActionResult AccessDenied()
        {
            return View();
        }

        public IActionResult Perfil()
        {
            return View();
        }
    }
}