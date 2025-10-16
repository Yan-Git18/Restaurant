using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using RESTAURANT.Data;
using System.Diagnostics;

namespace Restaurant.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalCategorias = await _context.Categorias.CountAsync();
            //var totalPersonas = await _context.Personas.CountAsync();
            var totalComprobantes = await _context.Comprobantes.CountAsync();
            //var totalInventario = await _context.Inventarios.CountAsync();
            var totalMesas = await _context.Mesas.CountAsync();
            var totalPagos = await _context.Pagos.CountAsync();
            var totalPedidos = await _context.Pedidos.CountAsync();
            var totalReservas = await _context.Reservas.CountAsync();
            var totalRoles = await _context.Roles.CountAsync();
            var totalUsuarios = await _context.Usuarios.CountAsync();
            var totalVentas = await _context.Ventas.CountAsync();
            var totalProductos = await _context.Productos.CountAsync();


            ViewData["TotalCategorias"] = totalCategorias;
            ViewData["TotalComprobantes"] = totalComprobantes;
            ViewData["TotalMesas"] = totalMesas;
            ViewData["TotalPagos"] = totalPagos;
            ViewData["TotalPedidos"] = totalPedidos;
            ViewData["TotalReservas"] = totalReservas;
            ViewData["TotalRoles"] = totalRoles;
            ViewData["TotalUsuarios"] = totalUsuarios;
            ViewData["TotalVentas"] = totalVentas;
            ViewData["TotalProductos"] = totalProductos;

            // Autocompletar datos si el usuario está logueado
            if (User.Identity!.IsAuthenticated)
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

                if (int.TryParse(userIdClaim, out int userId))
                {
                    var persona = await _context.Personas
                        .FirstOrDefaultAsync(p => p.UsuarioId == userId);

                    if (persona != null)
                    {
                        ViewBag.NombreCliente = persona.Nombre + " " + (persona.Apellidos ?? "");
                        ViewBag.TelefonoCliente = persona.Telefono;
                        ViewBag.EmailCliente = persona.Correo;
                        ViewBag.ClienteId = persona.PersonaId;
                    }
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
