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
            var mesas = await _context.Mesas.ToListAsync();
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
            var mesas = new Rest_Mesa
            {
                Numero = mesa.Numero,
                Capacidad = mesa.Capacidad,
                Estado = mesa.Estado,
                Observaciones = mesa.Observaciones
            };

            _context.Add(mesas);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Mesa registrada correctamente";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
            
            
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

            // ⚠️ Validación: no eliminar si tiene reservas o pedidos asociados
            if (mesa.Reservas.Any() || mesa.Pedidos.Any())
            {
                TempData["Mensaje"] = $"No se puede eliminar la mesa #{mesa.Numero} porque tiene reservas o pedidos asociados.";
                TempData["Tipo"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            _context.Mesas.Remove(mesa);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Mesa eliminada correctamente";
            TempData["Tipo"] = "success";

            return RedirectToAction(nameof(Index));
        }
    }
}
