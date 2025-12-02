using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Restaurant.ViewModels;
using RESTAURANT.Data;
using System.Security.Claims;

namespace Restaurant.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly AppDbContext _context;

        public ReservasController(AppDbContext context)
        {
            _context = context;
        }

        // PANEL ADMINISTRADOR: Ver todas las reservas con filtro
        [Authorize(Roles = "Administrador, Mesero")]
        public async Task<IActionResult> Index(string filtro)
        {
            var reservas = _context.Reservas.Include(r => r.Cliente).AsQueryable();

            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.ToLower();
                if (filtro == "confirmadas")
                    reservas = reservas.Where(r => r.Estado == "Confirmada");
                else if (filtro == "canceladas")
                    reservas = reservas.Where(r => r.Estado == "Cancelada");
            }

            var lista = await reservas.OrderByDescending(r => r.FechaHora).ToListAsync();
            ViewBag.FiltroActual = filtro ?? "todas";

            return View(lista);
        }

        // CLIENTE: Ver solo sus reservas
        [Authorize(Roles = "Administrador, Cliente")]
        public async Task<IActionResult> MisReservas()
        {
            List<Rest_Reserva> reservas;

            if (User.IsInRole("Administrador"))
            {
                reservas = await _context.Reservas
                    .Include(r => r.Cliente)
                    .OrderByDescending(r => r.FechaHora)
                    .ToListAsync();
            }
            else
            {
                var personaIdClaim = User.FindFirstValue("PersonaId");
                if (string.IsNullOrEmpty(personaIdClaim) || !int.TryParse(personaIdClaim, out int personaId))
                    return RedirectToAction("Login", "Cuenta");

                reservas = await _context.Reservas
                    .Include(r => r.Mesa)
                    .Where(r => r.ClienteId == personaId)
                    .OrderByDescending(r => r.FechaHora)
                    .ToListAsync();
            }

            return View(reservas);
        }

        /// CONFIRMAR reserva (solo admin o mesero)
        [Authorize(Roles = "Administrador, Mesero")]
        [HttpPost]
        [Route("Reservas/Confirmar/{id}")]
        public async Task<IActionResult> Confirmar(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
                return NotFound();

            reserva.Estado = "Confirmada";
            await _context.SaveChangesAsync();

            TempData["Success"] = "La reserva ha sido confirmada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // CANCELAR reserva (solo admin o mesero)
        [Authorize(Roles = "Administrador, Mesero")]
        [HttpPost]
        [Route("Reservas/CancelarAdmin/{id}")]
        public async Task<IActionResult> CancelarAdmin(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
                return NotFound();

            reserva.Estado = "Cancelada";
            await _context.SaveChangesAsync();

            TempData["Success"] = "La reserva ha sido cancelada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // CREAR reserva
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var model = new ReservaFormViewModel();

            if (User.IsInRole("Administrador"))
            {
                var clientes = await _context.Personas
                    .Include(p => p.Usuario)
                    .Where(p => p.Usuario != null && p.Usuario.Rol.Nombre == "Cliente")
                    .Select(p => new { p.PersonaId, NombreCompleto = p.Nombre + " " + (p.Apellidos ?? "") })
                    .ToListAsync();

                ViewBag.Clientes = new SelectList(clientes, "PersonaId", "NombreCompleto");
            }
            else if (User.IsInRole("Cliente"))
            {
                var personaIdClaim = User.FindFirstValue("PersonaId");
                if (int.TryParse(personaIdClaim, out int personaId))
                {
                    var persona = await _context.Personas.FindAsync(personaId);
                    if (persona != null)
                    {
                        model.Nombre = persona.Nombre;
                        model.Telefono = persona.Telefono;
                        model.Email = persona.Correo;
                        model.ClienteId = persona.PersonaId;
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ReservaFormViewModel model)
        {
            // Si el modelo no es válido (faltan campos o errores)
            if (!ModelState.IsValid)
            {
                // Si el usuario es cliente, volvemos al Index Home manteniendo los errores
                if (User.IsInRole("Cliente"))
                {
                    ViewBag.NombreCliente = model.Nombre;
                    ViewBag.TelefonoCliente = model.Telefono;
                    ViewBag.EmailCliente = model.Email;

                    // Esto devuelve la vista del Home y muestra los errores en el formulario
                    return View("~/Views/Home/Index.cshtml", model);
                }

                // Si es admin, seguimos mostrando su vista "Crear"
                if (User.IsInRole("Administrador"))
                {
                    var clientes = await _context.Personas
                        .Include(p => p.Usuario)
                        .Where(p => p.Usuario != null && p.Usuario.Rol.Nombre == "Cliente")
                        .Select(p => new { p.PersonaId, NombreCompleto = p.Nombre + " " + (p.Apellidos ?? "") })
                        .ToListAsync();

                    ViewBag.Clientes = new SelectList(clientes, "PersonaId", "NombreCompleto", model.ClienteId);
                }

                return View(model);
            }

            // Validar fecha y hora
            if (model.Fecha == default || string.IsNullOrEmpty(model.Hora) || !TimeSpan.TryParse(model.Hora, out var hora))
            {
                ModelState.AddModelError("", "Fecha u hora inválida.");
                return User.IsInRole("Cliente")
                    ? View("~/Views/Home/Index.cshtml", model)
                    : View(model);
            }

            int clienteId;
            if (User.IsInRole("Administrador"))
            {
                if (!model.ClienteId.HasValue || model.ClienteId.Value <= 0)
                {
                    ModelState.AddModelError("", "Debe seleccionar un cliente.");
                    return View(model);
                }
                clienteId = model.ClienteId.Value;
            }
            else
            {
                var personaIdClaim = User.FindFirstValue("PersonaId");
                if (!int.TryParse(personaIdClaim, out clienteId))
                    return RedirectToAction("Login", "Cuenta");
            }

            DateTime fechaHora = model.Fecha.Date.Add(TimeSpan.Parse(model.Hora));
            int numeroPersonas = model.Personas == "8+" ? 8 : int.Parse(model.Personas);

            var reserva = new Rest_Reserva
            {
                FechaHora = fechaHora,
                NumeroPersonas = numeroPersonas,
                Observaciones = $"Ocasión: {model.Ocasion ?? "Ninguna"} | Comentarios: {model.Comentarios ?? "Sin comentarios"}",
                Estado = "Pendiente",
                FechaCreacion = DateTime.Now,
                ClienteId = clienteId,
                MesaId = 1
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            TempData["Success"] = "¡Reserva registrada correctamente!";

            // Si es cliente, vuelve a MisReservas; si es admin, a su panel
            return User.IsInRole("Administrador") ? RedirectToAction("Index") : RedirectToAction("MisReservas");
        }

        // EDITAR reserva (solo admin)
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Cliente)
                .FirstOrDefaultAsync(r => r.ReservaId == id);

            if (reserva == null) return NotFound();

            // Separar ocasión y comentarios desde Observaciones
            string ocasion = reserva.Observaciones?
                .Split('|')
                .FirstOrDefault(o => o.Contains("Ocasión:"))
                ?.Replace("Ocasión:", "")
                .Trim();

            string comentarios = reserva.Observaciones?
                .Split('|')
                .FirstOrDefault(o => o.Contains("Comentarios:"))
                ?.Replace("Comentarios:", "")
                .Trim();

            // 🔥 AQUÍ INCLUYO LOS DATOS QUE FALTABAN
            var model = new ReservaFormViewModel
            {
                ClienteId = reserva.ClienteId,
                Nombre = reserva.Cliente?.Nombre,
                Telefono = reserva.Cliente?.Telefono,
                Email = reserva.Cliente?.Correo,
                Fecha = reserva.FechaHora.Date,
                Hora = reserva.FechaHora.ToString("HH:mm"),
                Personas = reserva.NumeroPersonas >= 8 ? "8+" : reserva.NumeroPersonas.ToString(),
                Ocasion = ocasion,
                Comentarios = comentarios
            };

            var clientes = await _context.Personas
                .Include(p => p.Usuario)
                .Where(p => p.Usuario != null && p.Usuario.Rol.Nombre == "Cliente")
                .Select(p => new { p.PersonaId, NombreCompleto = p.Nombre + " " + (p.Apellidos ?? "") })
                .ToListAsync();

            ViewBag.Clientes = new SelectList(clientes, "PersonaId", "NombreCompleto", model.ClienteId);
            return View(model);
        }



        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ReservaFormViewModel model)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null) return NotFound();

            if (!ModelState.IsValid)
            {
                var clientes = await _context.Personas
                    .Include(p => p.Usuario)
                    .Where(p => p.Usuario != null && p.Usuario.Rol.Nombre == "Cliente")
                    .Select(p => new { p.PersonaId, NombreCompleto = p.Nombre + " " + (p.Apellidos ?? "") })
                    .ToListAsync();

                ViewBag.Clientes = new SelectList(clientes, "PersonaId", "NombreCompleto", model.ClienteId);
                return View(model);
            }

            reserva.FechaHora = model.Fecha.Date.Add(TimeSpan.Parse(model.Hora));
            reserva.NumeroPersonas = model.Personas == "8+" ? 8 : int.Parse(model.Personas);
            reserva.Observaciones = $"Ocasión: {model.Ocasion} | Comentarios: {model.Comentarios}";
            reserva.ClienteId = model.ClienteId ?? reserva.ClienteId;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Reserva actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // DETALLES de reserva
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Detalles(int id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Cliente)
                .FirstOrDefaultAsync(r => r.ReservaId == id);

            if (reserva == null) return NotFound();

            var model = new ReservaFormViewModel
            {
                Nombre = reserva.Cliente?.Nombre,
                Telefono = reserva.Cliente?.Telefono,
                Email = reserva.Cliente?.Correo,
                Fecha = reserva.FechaHora.Date,
                Hora = reserva.FechaHora.ToString("HH:mm"),
                Personas = reserva.NumeroPersonas.ToString(),
                Ocasion = reserva.Observaciones?.Split('|').FirstOrDefault(o => o.Contains("Ocasión:"))?.Replace("Ocasión:", "").Trim(),
                Comentarios = reserva.Observaciones?.Split('|').FirstOrDefault(o => o.Contains("Comentarios:"))?.Replace("Comentarios:", "").Trim()
            };

            return User.IsInRole("Cliente") ? View("DetallesCliente", model) : View("Detalles", model);
        }

        // OBTENER CLIENTE (para autocompletar)
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ObtenerCliente(int id)
        {
            var cliente = await _context.Personas
                .Where(p => p.PersonaId == id)
                .Select(p => new { nombre = p.Nombre, telefono = p.Telefono, correo = p.Correo })
                .FirstOrDefaultAsync();

            if (cliente == null)
                return NotFound();

            return Json(cliente);
        }


        // CANCELAR reserva (CLIENTE)
        [Authorize(Roles = "Cliente")]
        [HttpPost]
        [Route("Reservas/Cancelar/{id}")]
        public async Task<IActionResult> Cancelar(int id)
        {
            var personaIdClaim = User.FindFirstValue("PersonaId");
            if (!int.TryParse(personaIdClaim, out int personaId))
                return Unauthorized();

            var reserva = await _context.Reservas
                .Where(r => r.ReservaId == id && r.ClienteId == personaId)
                .FirstOrDefaultAsync();

            if (reserva == null)
                return NotFound();

            reserva.Estado = "Cancelada";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Has cancelado tu reserva.";
            return RedirectToAction("MisReservas");
        }

    }
}
