using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using RESTAURANT.Data;

namespace Restaurant.Controllers
{
    public class InventarioController : Controller
    {
        private readonly AppDbContext _context;

        public InventarioController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Index
        public async Task<IActionResult> Index()
        {
            var inventario = await _context.Inventarios.ToListAsync();
            return View(inventario);
        }

        // GET: Crear
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        // POST: Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Rest_Inventario inv)
        {
            var inventario = new Rest_Inventario
            {
                Nombre = inv.Nombre,
                Descripcion = inv.Descripcion,
                UnidadMedida = inv.UnidadMedida,
                Stock = inv.Stock,
                StockMinimo = inv.StockMinimo,
                FechaActualizacion = DateTime.Now
            };

            await _context.Inventarios.AddAsync(inventario);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Se agregó el insumo al inventario.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }

        // GET: Editar
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var inventario = await _context.Inventarios.FindAsync(id);

            if (inventario == null)
                return NotFound();

            return View(inventario);
        }

        // POST: Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Rest_Inventario inv)
        {
            var inventario = await _context.Inventarios.FindAsync(inv.Id);

            if (inventario == null)
                return NotFound();

            inventario.Nombre = inv.Nombre;
            inventario.Descripcion = inv.Descripcion;
            inventario.UnidadMedida = inv.UnidadMedida;
            inventario.Stock = inv.Stock;
            inventario.StockMinimo = inv.StockMinimo;
            inventario.FechaActualizacion = DateTime.Now;

            _context.Inventarios.Update(inventario);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Se actualizó el insumo.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmar(int id)
        {
            var inventario = await _context.Inventarios
                .Include(i => i.Productos)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inventario == null)
            {
                return NotFound();
            }

            if (inventario.Productos.Any())
            {
                TempData["Mensaje"] = $"No se puede eliminar el inventario '{inventario.Nombre}' porque tiene productos asociados.";
                TempData["Tipo"] = "error";
                return RedirectToAction("Index");
            }

            _context.Inventarios.Remove(inventario);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Inventario eliminado correctamente.";
            TempData["Tipo"] = "success";
            return RedirectToAction("Index");
        }

    }
}
