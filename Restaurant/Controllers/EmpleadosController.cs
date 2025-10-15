using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using RESTAURANT.Data;

namespace Restaurant.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class EmpleadosController : Controller
    {
        private readonly AppDbContext _context;

        public EmpleadosController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var empleados = await _context.Personas
                .Include(p => p.Usuario)
                .ThenInclude(u => u.Rol)
                .Where(p => p.Usuario != null && p.Usuario.Rol.Nombre == "Empleado")
                .ToListAsync();
            return View(empleados);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Rest_Persona empleado)
        {
            if (ModelState.IsValid)
            {
                empleado.FechaRegistro = DateTime.Now;
                _context.Personas.Add(empleado);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(empleado);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var empleado = await _context.Personas.FindAsync(id);
            if (empleado == null) return NotFound();
            return View(empleado);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Rest_Persona empleado)
        {
            if (id != empleado.PersonaId) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(empleado);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(empleado);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var empleado = await _context.Personas.FindAsync(id);
            if (empleado == null) return NotFound();
            return View(empleado);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empleado = await _context.Personas.FindAsync(id);
            if (empleado != null)
            {
                _context.Personas.Remove(empleado);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
