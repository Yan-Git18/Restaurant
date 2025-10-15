using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using RESTAURANT.Data;

namespace Restaurant.Controllers
{
    [Authorize(Roles = "Administrador, Cajero")]
    public class PagosController : Controller
    {
        private readonly AppDbContext _context;

        public PagosController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var pagos = await _context.Pagos
                .Include(p => p.Venta)
                    .ThenInclude(v => v.Pedido)
                        .ThenInclude(p => p.Cliente)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            return View(pagos);
        }

        public IActionResult Crear()
        {
            var ventasDisponibles = _context.Ventas
                .Include(v => v.Pagos)
                .Include(v => v.Pedido)
                    .ThenInclude(p => p.Cliente)
                .ToList();

            // Solo ventas con saldo pendiente
            var ventasPendientes = ventasDisponibles
                .Where(v => v.Pagos.Sum(p => p.Monto) < v.Total)
                .Select(v => new
                {
                    v.Id,
                    Cliente = v.Pedido.Cliente.Nombre,
                    Total = v.Total,
                    Pagado = v.Pagos.Sum(p => p.Monto),
                    Pendiente = v.Total - v.Pagos.Sum(p => p.Monto)
                })
                .ToList();

            ViewBag.Ventas = ventasPendientes;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Rest_Pago pago)
        {
            var venta = await _context.Ventas
                .Include(v => v.Pagos)
                .FirstOrDefaultAsync(v => v.Id == pago.VentaId);

            if (venta == null)
            {
                ModelState.AddModelError("VentaId", "La venta seleccionada no existe.");
            }
            else
            {
                var totalPagado = venta.Pagos.Sum(p => p.Monto);
                var pendiente = venta.Total - totalPagado;

                if (pago.Monto > pendiente)
                {
                    ModelState.AddModelError("Monto", $"El monto excede el saldo pendiente (S/ {pendiente:F2}).");
                }
            }

            var totalPagadoActualizado = venta.Pagos.Sum(p => p.Monto) + pago.Monto;
            if (totalPagadoActualizado >= venta.Total)
            {
                venta.Estado = "Pagada";
                _context.Update(venta);
                await _context.SaveChangesAsync();
            }

            pago.FechaPago = DateTime.Now;
            _context.Add(pago);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Pago registrado exitosamente.";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }



        [HttpGet]
        public async Task<IActionResult> Detalles(int id)
        {
            var pago = await _context.Pagos
                .Include(p => p.Venta)
                    .ThenInclude(v => v.Pedido)
                        .ThenInclude(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pago == null)
                return NotFound();

            return View(pago);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var pago = await _context.Pagos
                .Include(p => p.Venta)
                    .ThenInclude(v => v.Pedido)
                        .ThenInclude(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pago == null)
                return NotFound();

            ViewBag.Ventas = await _context.Ventas
                .Include(v => v.Pedido)
                    .ThenInclude(p => p.Cliente)
                .ToListAsync();

            return View(pago);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Rest_Pago pago)
        {
            if (id != pago.Id)
                return NotFound();

            pago.FechaPago = DateTime.Now;
            _context.Update(pago);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Pago actualizado correctamente.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmar(int id)
        {
            var pago = await _context.Pagos.FindAsync(id);
            if (pago == null)
                return NotFound();

            _context.Pagos.Remove(pago);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Pago eliminado correctamente.";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }
    }
}
