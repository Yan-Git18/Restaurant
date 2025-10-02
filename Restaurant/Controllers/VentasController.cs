using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using RESTAURANT.Data;
namespace Restaurant.Controllers
{
    public class VentasController : Controller
    {
        private readonly AppDbContext _context;
        public VentasController (AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public IActionResult Index()
        {
            var pedidos = _context.Pedidos
                .Where(p => p.Estado == "Entregado" || p.Estado == "Pendiente") // Ajusta el filtro según tu lógica
                .OrderByDescending(p => p.Fecha)
                .Select(p => new {
                 value = p.Id,
                 text = $"Pedido #{p.Id} - {p.Cliente.Nombre} - {p.Fecha:dd/MM/yyyy HH:mm}"
        })
        .ToList();

            ViewBag.Pedidos = pedidos;
            return View();
        }
        [HttpPost]
        public IActionResult Crear(int[] ProductosSeleccionados)
        {
            // 1. Obtener los pedidos seleccionados
            var pedidos = _context.Pedidos
                .Include(p => p.DetallesPedido)
                .Where(p => ProductosSeleccionados.Contains(p.Id))
                .ToList();

            // 2. Calcular el total de la venta
            decimal totalVenta = pedidos
                .SelectMany(p => p.DetallesPedido)
                .Sum(d => d.Subtotal);

            // 3. Crear la venta
            var venta = new Rest_Venta
            {
                Fecha = DateTime.Now,
                Total = totalVenta,
                Descuento = 0, // Puedes ajustar esto si tienes lógica de descuentos
                Impuesto = 0.18m, // Por ejemplo, 18% IGV
                Estado = "Pagada", // O el estado que corresponda
                PedidoId = pedidos.First().Id // Si solo asocias a un pedido, ajusta según tu lógica
            };

            // 4. Asociar la venta a los pedidos seleccionados (opcional, si tu modelo lo permite)
            foreach (var pedido in pedidos)
            {
                pedido.Venta = venta;
            }
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
