using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTAURANT.Data;

namespace Restaurant.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        //private readonly AppDbContext _context;
        public async Task<IActionResult> Index()
        {
            //var totalCategorias = await _context.Categorias.CountAsync();
            ////var totalPersonas = await _context.Personas.CountAsync();
            //var totalComprobantes = await _context.Comprobantes.CountAsync();
            ////var totalInventario = await _context.Inventarios.CountAsync();
            //var totalMesas = await _context.Mesas.CountAsync();
            //var totalPagos = await _context.Pagos.CountAsync();
            //var totalPedidos = await _context.Pedidos.CountAsync();
            //var totalReservas = await _context.Reservas.CountAsync();
            //var totalRoles = await _context.Roles.CountAsync();
            //var totalUsuarios = await _context.Usuarios.CountAsync();
            //var totalVentas = await _context.Ventas.CountAsync();
            //var totalProductos = await _context.Productos.CountAsync();


            //ViewData["TotalCategorias"] = totalCategorias;
            //ViewData["TotalComprobantes"] = totalComprobantes;
            //ViewData["TotalMesas"] = totalMesas;
            //ViewData["TotalPagos"] = totalPagos;
            //ViewData["TotalPedidos"] = totalPedidos;
            //ViewData["TotalReservas"] = totalReservas;
            //ViewData["TotalRoles"] = totalRoles;
            //ViewData["TotalUsuarios"] = totalUsuarios;
            //ViewData["TotalVentas"] = totalVentas;
            //ViewData["TotalProductos"] = totalProductos;

            return View();
        }
    }
}
