using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using RESTAURANT.Data;

namespace Restaurant.Controllers
{
    public class ClientesController : Controller
    {
        private readonly AppDbContext _context;

        public ClientesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var clientes = await _context.Personas
                .Include(p => p.Usuario)
                .ThenInclude(u => u.Rol)
                .Where(p => p.Usuario != null && p.Usuario.Rol.Nombre == "Cliente")
                .ToListAsync();
            return View(clientes);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Rest_Persona cliente)
        {
            if (ModelState.IsValid)
            {
                cliente.FechaRegistro = DateTime.Now;
                _context.Personas.Add(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cliente = await _context.Personas.FindAsync(id);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Rest_Persona cliente)
        {
            if (id != cliente.PersonaId) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _context.Personas.FindAsync(id);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Personas.FindAsync(id);
            if (cliente != null)
            {
                _context.Personas.Remove(cliente);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
