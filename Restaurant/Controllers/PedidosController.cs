using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Restaurant.Models;
using RESTAURANT.Data;

namespace Restaurant.Controllers
{
    public class PedidosController : Controller
    {
        private readonly AppDbContext _context;
        public PedidosController(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }
        public IActionResult Index()
        {   
            var productos = _context.Productos.ToList();
            ViewBag.Productos = productos;
            return View();
        }
        [HttpPost]
        public IActionResult Crear(Rest_Pedido pedido, int[] ProductosSeleccionados)
        {
            // Asignar cliente genérico si no se recibe uno
            pedido.ClienteId = 1;

            // Guardar el pedido
            _context.Pedidos.Add(pedido);
            _context.SaveChanges();

            // Registrar los productos en el pedido (suponiendo que hay una tabla intermedia DetallePedido)
            foreach (var idProducto in ProductosSeleccionados)
            {
                var producto = _context.Productos.Find(idProducto);
                if (producto != null)
                {
                    var detPedido = new Rest_DetallePedido
                    {
                        PedidoId = pedido.Id,
                        ProductoId = idProducto,
                        Cantidad = 1, // Puedes cambiar esto si permites seleccionar cantidad
                        Subtotal = producto.Precio,
                        Observaciones = null
                    };
                    _context.DetallesPedido.Add(detPedido);
                }
            }
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
