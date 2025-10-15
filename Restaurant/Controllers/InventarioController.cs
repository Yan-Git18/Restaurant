using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using RESTAURANT.Data;

namespace Restaurant.Controllers
{
    [Authorize(Roles = "Administrador, Empleado")]
    public class InventarioController : Controller
    {
        private readonly AppDbContext _context;

        public InventarioController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var inventarios = await _context.Inventarios
                .Include(i => i.Productos)
                .ToListAsync();

            var inventarioConStock = inventarios.Select(i => new Rest_InventarioViewModel
            {
                Id = i.Id,
                Nombre = i.Nombre,
                Descripcion = i.Descripcion,
                UnidadMedida = i.UnidadMedida,
                StockTotal = i.Productos.Sum(p => p.Stock),
                StockMinimo = i.StockMinimo,
                FechaActualizacion = i.FechaActualizacion
            }).ToList();

            return View(inventarioConStock);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Rest_Inventario inv)
        {
            inv.FechaActualizacion = DateTime.Now;
            await _context.Inventarios.AddAsync(inv);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Se agregó el insumo al inventario.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var inventario = await _context.Inventarios.FindAsync(id);

            if (inventario == null)
                return NotFound();

            return View(inventario);
        }

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
            return RedirectToAction(nameof(Index));
        }
    }
}
