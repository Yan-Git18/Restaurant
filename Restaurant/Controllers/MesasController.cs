using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using RESTAURANT.Data;

namespace Restaurant.Controllers
{
    [Authorize(Roles = "Administrador, Mesero")]
    public class MesasController : Controller
    {
        private readonly AppDbContext _context;

        public MesasController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var mesas = await _context.Mesas
                .Where(m => m.Activo)
                .ToListAsync();

            return View(mesas);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Rest_Mesa mesa)
        {
            if (!ModelState.IsValid) return View(mesa);

            // Validar duplicado
            var existe = await _context.Mesas.AnyAsync(m => m.Numero == mesa.Numero);
            if (existe)
            {
                ModelState.AddModelError("Numero", $"Ya existe una mesa con el número {mesa.Numero}.");
                return View(mesa);
            }

            mesa.FechaCreacion = DateTime.Now;
            _context.Add(mesa);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Mesa registrada correctamente";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Editar(int id)
        {
            var mesa = await _context.Mesas.FindAsync(id);
            if (mesa == null) return NotFound();

            return View(mesa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Rest_Mesa mesa)
        {
            if (!ModelState.IsValid)
                return View(mesa);

            var mesas = await _context.Mesas.FindAsync(mesa.MesaId);
            if (mesas == null) return NotFound();

            // Validar duplicado (excluyendo la misma mesa)
            var existe = await _context.Mesas
                .AnyAsync(m => m.Numero == mesa.Numero && m.MesaId != mesa.MesaId);

            if (existe)
            {
                ModelState.AddModelError("Numero", $"Ya existe otra mesa con el número {mesa.Numero}.");
                return View(mesa);
            }

            mesas.Numero = mesa.Numero;
            mesas.Capacidad = mesa.Capacidad;
            mesas.Estado = mesa.Estado;
            mesas.Observaciones = mesa.Observaciones;

            _context.Mesas.Update(mesas);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Mesa actualizada correctamente";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmar(int id)
        {
            var mesa = await _context.Mesas
                .Include(m => m.Reservas)
                .Include(m => m.Pedidos)
                .FirstOrDefaultAsync(m => m.MesaId == id);

            if (mesa == null)
            {
                return NotFound();
            }

            // No desactivar si tiene reservas o pedidos asociados
            if (mesa.Reservas.Any() || mesa.Pedidos.Any())
            {
                TempData["Mensaje"] = $"No se puede desactivar la mesa #{mesa.Numero} porque tiene reservas o pedidos asociados.";
                TempData["Tipo"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            mesa.Activo = false;
            _context.Mesas.Update(mesa);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Mesa desactivada correctamente";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }
    }
}
