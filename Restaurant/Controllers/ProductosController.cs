using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using RESTAURANT.Data; // Ajusta si tu namespace es distinto
using System.Linq;

namespace Restaurant.Controllers
{
    public class ProductosController : Controller
    {
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Inventario)
                .Where(p => p.Activo)   // solo activos en el índice
                .OrderByDescending(p => p.FechaCreacion)
                .ToListAsync();

            return View(productos);
        }


        // GET: Crear
        [HttpGet]
        public IActionResult Crear()
        {
            // Construyo listas como en tu ejemplo original
            var categorias = _context.Categorias
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Nombre })
                .ToList();

            var inventarios = _context.Inventarios
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Nombre })
                .ToList();

            ViewBag.Categorias = categorias;
            ViewBag.Inventarios = inventarios;
            return View();
        }

        // POST: Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Rest_Producto p)
        {
            var producto = new Rest_Producto
            {
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                ImagenUrl = p.ImagenUrl,
                Disponible = p.Disponible,
                FechaCreacion = DateTime.Now,
                CategoriaId = p.CategoriaId,
                InventarioId = p.InventarioId,
                Stock = p.Stock   // 👈 ahora lo guardamos
            };

            await _context.Productos.AddAsync(producto);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Se agregó el producto.";
            TempData["Tipo"] = "success";
            return RedirectToAction("Index");
        }

        // GET: Editar
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var producto = _context.Productos.FirstOrDefault(u => u.Id == id);

            if (producto == null)
            {
                return NotFound();
            }

            ViewBag.Categorias = _context.Categorias
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Nombre })
                .ToList();

            ViewBag.Inventarios = _context.Inventarios
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Nombre })
                .ToList();

            return View(producto);
        }

        // POST: Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Rest_Producto p)
        {
            var original = await _context.Productos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == p.Id);
            if (original != null)
            {
                p.FechaCreacion = original.FechaCreacion;
            }

            _context.Productos.Update(p);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Se actualizó el producto.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }

        // POST: EliminarConfirmar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmar(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                TempData["Mensaje"] = "Producto no encontrado.";
                TempData["Tipo"] = "error";
                return RedirectToAction(nameof(Index));
            }

            // Validación: no desactivar si está en pedidos actuales (opcional)
            var enDetalles = await _context.DetallesPedido.AnyAsync(d => d.ProductoId == id);
            if (enDetalles)
            {
                // en lugar de eliminar, marcamos inactivo; si quieres bloquear desactivación puedes retornar error
                producto.Activo = false;
                _context.Productos.Update(producto);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Producto desactivado (está presente en pedidos históricos).";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            // Si no está en detalles, igual lo desactivamos (no remove)
            producto.Activo = false;
            _context.Productos.Update(producto);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Producto desactivado correctamente.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }

    }
}
