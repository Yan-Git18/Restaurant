using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Restaurant.ViewModels;
using RESTAURANT.Data;

namespace Restaurant.Controllers
{
    [Authorize(Roles = "Administrador, Cajero")]
    public class VentasController : Controller
    {
        private readonly AppDbContext _context;

        public VentasController(AppDbContext context)
        {
            _context = context;
        }

        private List<Rest_Pedido> GetPedidosDisponibles()
        {
            return _context.Pedidos
                .Include(p => p.DetallesPedido)
                .Where(p => p.Venta == null && p.Estado == EstadoPedido.Pendiente.ToString())
                .Include(p => p.Cliente)
                .ToList();
        }

        
        public async Task<IActionResult> Index()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Pedido)
                    .ThenInclude(p => p.Cliente)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            return View(ventas);
        }

        
        public IActionResult Crear()
        {
            ViewBag.Pedidos = GetPedidosDisponibles();
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(VentaCreateVm vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Pedidos = GetPedidosDisponibles();
                return View(vm);
            }

            var pedido = await _context.Pedidos
                .Include(p => p.DetallesPedido)
                    .ThenInclude(d => d.Producto)
                .Include(p => p.Venta)
                .FirstOrDefaultAsync(p => p.Id == vm.PedidoId);

            if (pedido == null)
                return NotFound();

            // No crear venta si ya existe
            if (pedido.Venta != null)
            {
                TempData["Mensaje"] = "Este pedido ya tiene una venta registrada.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Crear));
            }

            // Validar impuesto
            if (vm.Impuesto < 0 || vm.Impuesto > 1)
            {
                TempData["Mensaje"] = "El valor del impuesto debe estar entre 0 y 1.";
                TempData["Tipo"] = "danger";
                return RedirectToAction(nameof(Crear));
            }

            decimal subtotal = pedido.DetallesPedido.Sum(d => d.Subtotal);
            decimal descuento = vm.Descuento < 0 ? 0 : vm.Descuento;
            decimal neto = Math.Max(subtotal - descuento, 0);
            decimal montoImpuesto = neto * vm.Impuesto;
            decimal totalFinal = neto + montoImpuesto;

            // Validar stock
            foreach (var d in pedido.DetallesPedido)
            {
                var prod = await _context.Productos.FindAsync(d.ProductoId);
                if (prod == null || prod.Stock < d.Cantidad)
                {
                    TempData["Mensaje"] = $"Stock insuficiente para {d.Producto?.Nombre ?? "producto"}";
                    TempData["Tipo"] = "danger";
                    return RedirectToAction(nameof(Crear));
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var venta = new Rest_Venta
                {
                    Fecha = DateTime.Now,
                    Total = totalFinal,
                    Descuento = descuento,
                    Impuesto = vm.Impuesto,
                    Estado = EstadoVenta.Registrada.ToString(),
                    PedidoId = pedido.Id
                };

                _context.Ventas.Add(venta);

                // Reducir stock de productos
                foreach (var d in pedido.DetallesPedido)
                {
                    var prod = await _context.Productos.FindAsync(d.ProductoId);
                    prod.Stock -= d.Cantidad;
                    if (prod.Stock < 0) prod.Stock = 0;
                    _context.Productos.Update(prod);
                }

                // Cambiar estado del pedido a Atendido
                pedido.Estado = EstadoPedido.Atendido.ToString();
                _context.Pedidos.Update(pedido);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Mensaje"] = "Venta registrada correctamente.";
                TempData["Tipo"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Mensaje"] = "Error al procesar la venta: " + ex.Message;
                TempData["Tipo"] = "danger";
                return RedirectToAction(nameof(Crear));
            }
        }


        public async Task<IActionResult> Editar(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Pedido)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null) return NotFound();

            ViewBag.Pedidos = _context.Pedidos.ToListAsync();
            return View(venta);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Rest_Venta venta)
        {
            if (id != venta.Id) return NotFound();
            
            ViewBag.Pedidos = _context.Pedidos.ToListAsync();               

            try
            {
                var ventaDb = await _context.Ventas.FindAsync(id);
                if (ventaDb == null) return NotFound();

                // Solo se permite editar estado, impuesto o descuento
                ventaDb.Estado = venta.Estado;
                ventaDb.Descuento = venta.Descuento;
                ventaDb.Impuesto = venta.Impuesto;

                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Venta actualizada correctamente.";
                TempData["Tipo"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["Mensaje"] = "Error al actualizar la venta.";
                TempData["Tipo"] = "danger";
                return RedirectToAction(nameof(Index));
            }
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmar(int id)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null) return NotFound();

            var pedido = await _context.Pedidos
                .Include(p => p.DetallesPedido)
                .FirstOrDefaultAsync(p => p.Id == venta.PedidoId);

            if (pedido != null)
            {
                foreach (var d in pedido.DetallesPedido)
                {
                    var prod = await _context.Productos.FindAsync(d.ProductoId);
                    if (prod != null)
                    {
                        prod.Stock += d.Cantidad;
                        _context.Productos.Update(prod);
                    }
                }

                pedido.Estado = EstadoPedido.Anulado.ToString();
                _context.Pedidos.Update(pedido);
            }

            venta.Estado = EstadoVenta.Anulada.ToString();
            _context.Ventas.Update(venta);

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Venta anulada y stock restaurado.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Detalles(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Pedido)
                    .ThenInclude(p => p.Cliente)
                .Include(v => v.Pagos)
                .Include(v => v.Comprobante)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
                return NotFound();

            return View(venta);
        }

    }
}
