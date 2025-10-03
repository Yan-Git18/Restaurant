using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RESTAURANT.Data;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    public class MesasController : Controller
    {
        private readonly AppDbContext _context;

        public MesasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Mesas
        public async Task<IActionResult> Index()
        {
            var mesas = await _context.Mesas
                .Where(m => m.Activo) // solo las activas
                .ToListAsync();

            return View(mesas);
        }


        // GET: Mesas/Crear
        public IActionResult Crear()
        {
            return View();
        }

        // POST: Mesas/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Rest_Mesa mesa)
        {
            if (ModelState.IsValid)
            {
                // validar duplicado
                var existe = await _context.Mesas.AnyAsync(m => m.Numero == mesa.Numero);
                if (existe)
                {
                    ModelState.AddModelError("Numero", $"Ya existe una mesa con el número {mesa.Numero}.");
                    return View(mesa);
                }

                mesa.FechaCreacion = DateTime.Now; // si quieres tener fecha
                _context.Add(mesa);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Mesa registrada correctamente";
                TempData["Tipo"] = "success";
                return RedirectToAction(nameof(Index));
            }

            return View(mesa);
        }

        // GET: Mesas/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var mesa = await _context.Mesas.FindAsync(id);
            if (mesa == null) return NotFound();

            return View(mesa);
        }

        // POST: Mesas/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Rest_Mesa mesa)
        {
            var mesas = await _context.Mesas.FindAsync(mesa.MesaId);
            if (mesas == null) return NotFound();

            // validar duplicado (excluyendo la misma mesa)
            var existe = await _context.Mesas.AnyAsync(m => m.Numero == mesa.Numero && m.MesaId != mesa.MesaId);
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

        // POST: Mesas/Eliminar
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

            // ⚠️ Validación: no desactivar si tiene reservas o pedidos asociados
            if (mesa.Reservas.Any() || mesa.Pedidos.Any())
            {
                TempData["Mensaje"] = $"No se puede desactivar la mesa #{mesa.Numero} porque tiene reservas o pedidos asociados.";
                TempData["Tipo"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            mesa.Activo = false; // 👈 solo desactivar
            _context.Mesas.Update(mesa);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Mesa desactivada correctamente";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }
    }
}
