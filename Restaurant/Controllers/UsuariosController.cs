using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Restaurant.ViewModels;
using RESTAURANT.Data;

namespace Restaurant.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios.Include(u => u.Rol).ToListAsync();
            return View(usuarios);
        }

        public IActionResult Create()
        {
            ViewBag.RolId = new SelectList(_context.Roles, "RolId", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = new Rest_Usuario
                {
                    Correo = model.Correo,
                    RolId = model.RolId,
                    Activo = model.Activo,
                    FechaCreacion = DateTime.Now,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password!)
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // Crear la persona asociada al usuario
                var persona = new Rest_Persona
                {
                    Nombre = model.Nombre ?? "Sin nombre",
                    Apellidos = model.Apellidos,
                    Telefono = model.Telefono ?? "",
                    Correo = model.Correo,
                    UsuarioId = usuario.UsuarioId,
                    FechaRegistro = DateTime.Now
                };

                _context.Personas.Add(persona);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.RolId = new SelectList(_context.Roles, "RolId", "Nombre", model.RolId);
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Cliente)
                .FirstOrDefaultAsync(u => u.UsuarioId == id);

            if (usuario == null) return NotFound();

            var vm = new UsuarioViewModel
            {
                UsuarioId = usuario.UsuarioId,
                Correo = usuario.Correo,
                RolId = usuario.RolId,
                Activo = usuario.Activo,
                FechaCreacion = usuario.FechaCreacion,
                Nombre = usuario.Cliente?.Nombre,
                Apellidos = usuario.Cliente?.Apellidos,
                Telefono = usuario.Cliente?.Telefono
            };

            ViewBag.RolId = new SelectList(_context.Roles, "RolId", "Nombre", usuario.RolId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UsuarioViewModel model)
        {
            if (id != model.UsuarioId) return NotFound();

            if (ModelState.IsValid)
            {
                var usuarioDb = await _context.Usuarios
                    .Include(u => u.Cliente)
                    .FirstOrDefaultAsync(u => u.UsuarioId == id);

                if (usuarioDb == null) return NotFound();

                usuarioDb.Correo = model.Correo;
                usuarioDb.RolId = model.RolId;
                usuarioDb.Activo = model.Activo;

                if (!string.IsNullOrEmpty(model.Password))
                {
                    usuarioDb.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                }

                // Actualizar datos de persona
                if (usuarioDb.Cliente != null)
                {
                    usuarioDb.Cliente.Nombre = model.Nombre ?? usuarioDb.Cliente.Nombre;
                    usuarioDb.Cliente.Apellidos = model.Apellidos ?? usuarioDb.Cliente.Apellidos;
                    usuarioDb.Cliente.Telefono = model.Telefono ?? usuarioDb.Cliente.Telefono;
                    usuarioDb.Cliente.Correo = model.Correo;
                }

                _context.Update(usuarioDb);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.RolId = new SelectList(_context.Roles, "RolId", "Nombre", model.RolId);
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.UsuarioId == id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
