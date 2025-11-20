using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Restaurant.ViewModels;
using RESTAURANT.Data;

namespace Restaurant.Controllers
{
    [Authorize(Roles = "Administrador, Cajero, Mesero")]
    public class PedidosController : Controller
    {
        private readonly AppDbContext _context;

        public PedidosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Pedidos
        public async Task<IActionResult> Index()
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Mesa)
                .Include(p => p.DetallesPedido)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();

            return View(pedidos);
        }

        // GET: Pedidos/Detalles/5
        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null) return NotFound();

            var pedido = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Mesa)
                .Include(p => p.DetallesPedido)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pedido == null) return NotFound();

            return View(pedido);
        }

        private void CargarCombos(int? clienteId = null, int? mesaId = null)
        {
            var clientes = _context.Personas
                .Select(p => new
                {
                    PersonaId = p.PersonaId,
                    NombreCompleto = p.Nombre + " " + (p.Apellidos ?? "")
                })
                .ToList();
            ViewBag.Clientes = new SelectList(clientes, "PersonaId", "NombreCompleto", clienteId);

            ViewBag.Mesas = new SelectList(_context.Mesas.ToList(), "MesaId", "Numero", mesaId);

            ViewBag.Productos = new SelectList(
                _context.Productos
                    .Where(p => p.Disponible && p.Activo)
                    .Select(p => new
                    {
                        Id = p.Id,
                        Nombre = p.Nombre + " - S/ " + p.Precio
                    })
                    .ToList(),
                "Id", "Nombre"
            );

            ViewBag.ProductosJson = _context.Productos
                .Where(p => p.Disponible && p.Activo)
                .Select(p => new
                {
                    productoId = p.Id,
                    nombre = p.Nombre,
                    precio = p.Precio
                })
                .ToList();
        }

        public IActionResult Crear()
        {
            CargarCombos();
            return View(new PedidoCreateEditVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(PedidoCreateEditVm vm)
        {
            if (vm == null) return BadRequest();

            if (vm.Detalles == null || !vm.Detalles.Any())
                ModelState.AddModelError("", "Debe agregar al menos un producto al pedido.");

            if (!ModelState.IsValid)
            {
                CargarCombos(vm.ClienteId, vm.MesaId);
                return View(vm);
            }

            var pedido = new Rest_Pedido
            {
                Fecha = DateTime.Now,
                Estado = EstadoPedido.Pendiente.ToString(),
                Observaciones = vm.Observaciones,
                TipoPedido = vm.TipoPedido,
                ClienteId = vm.ClienteId,
                MesaId = vm.MesaId,
                DetallesPedido = new List<Rest_DetallePedido>()
            };

            decimal total = 0m;
            foreach (var d in vm.Detalles)
            {
                if (d == null || d.ProductoId <= 0 || d.Cantidad <= 0) continue;

                var detalle = new Rest_DetallePedido
                {
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad,
                    Subtotal = d.Precio * d.Cantidad
                };

                pedido.DetallesPedido.Add(detalle);
                total += detalle.Subtotal;
            }

            pedido.Total = total;

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Pedido registrado correctamente.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var pedido = await _context.Pedidos
                .Include(p => p.DetallesPedido)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            // Bloquear edición si ya está atendido
            if (pedido.Estado == EstadoPedido.Atendido.ToString())
            {
                TempData["Mensaje"] = "No se puede editar un pedido ya atendido.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            CargarCombos(pedido.ClienteId, pedido.MesaId);

            var vm = new PedidoCreateEditVm
            {
                Id = pedido.Id,
                ClienteId = pedido.ClienteId,
                MesaId = pedido.MesaId,
                TipoPedido = pedido.TipoPedido,
                Estado = pedido.Estado,
                Observaciones = pedido.Observaciones,
                Total = pedido.Total,
                Detalles = pedido.DetallesPedido.Select(d => new DetallePedidoVm
                {
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad,
                    Precio = d.Subtotal / d.Cantidad
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, PedidoCreateEditVm vm)
        {
            if (id != vm.Id) return NotFound();

            var pedido = await _context.Pedidos
                .Include(p => p.DetallesPedido)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            if (pedido.Estado == EstadoPedido.Atendido.ToString())
            {
                TempData["Mensaje"] = "No se puede editar un pedido ya atendido.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            if (vm.Detalles == null || !vm.Detalles.Any())
                ModelState.AddModelError("", "Debe agregar al menos un producto al pedido.");

            if (!ModelState.IsValid)
            {
                CargarCombos(vm.ClienteId, vm.MesaId);
                return View(vm);
            }

            pedido.ClienteId = vm.ClienteId;
            pedido.MesaId = vm.MesaId;
            pedido.TipoPedido = vm.TipoPedido;
            pedido.Estado = vm.Estado;
            pedido.Observaciones = vm.Observaciones;
            pedido.Fecha = DateTime.Now;

            _context.DetallesPedido.RemoveRange(pedido.DetallesPedido);

            decimal total = 0m;
            pedido.DetallesPedido = new List<Rest_DetallePedido>();

            foreach (var d in vm.Detalles)
            {
                if (d == null || d.ProductoId <= 0 || d.Cantidad <= 0) continue;

                var detalle = new Rest_DetallePedido
                {
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad,
                    Subtotal = d.Precio * d.Cantidad
                };

                pedido.DetallesPedido.Add(detalle);
                total += detalle.Subtotal;
            }

            pedido.Total = total;

            _context.Update(pedido);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Pedido actualizado correctamente.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> EliminarConfirmar(int id)
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Venta)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null) return NotFound();

            if (pedido.Venta != null)
            {
                TempData["Mensaje"] = "No se puede eliminar un pedido que ya tiene venta.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Pedido eliminado correctamente.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }
    }
}
