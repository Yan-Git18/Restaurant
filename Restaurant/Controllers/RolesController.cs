using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using RESTAURANT.Data;

namespace Restaurant.Controllers
{
    public class RolesController : Controller
    {
        private readonly AppDbContext _context;

        public RolesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Roles.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Rest_Rol rol)
        {
            if (ModelState.IsValid)
            {
                _context.Roles.Add(rol);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rol);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null) return NotFound();
            return View(rol);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Rest_Rol rol)
        {
            if (id != rol.RolId) return NotFound();
            if (ModelState.IsValid)
            {
                _context.Update(rol);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rol);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null) return NotFound();
            return View(rol);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol != null)
            {
                _context.Roles.Remove(rol);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
