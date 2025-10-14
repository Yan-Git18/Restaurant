using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Restaurant.ViewModels;
using RESTAURANT.Data;

namespace Restaurant.Controllers
{
    public class ReservasController : Controller
    {
        private readonly AppDbContext _context;

        public ReservasController(AppDbContext context)
        {
            _context = context;
        }

        // LISTAR RESERVAS
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var reservasQuery = _context.Reservas
                .Include(r => r.Cliente)
                .Include(r => r.Mesa)
                .OrderByDescending(r => r.FechaHora)
                .AsQueryable();

            if (User.Identity!.IsAuthenticated)
            {
                var personaIdClaim = User.Claims.FirstOrDefault(c => c.Type == "PersonaId")?.Value;
                if (int.TryParse(personaIdClaim, out int personaId) && personaId > 0)
                {
                    reservasQuery = reservasQuery.Where(r => r.ClienteId == personaId);
                }
            }

            var reservas = await reservasQuery.ToListAsync();
            return View(reservas);
        }

        // CREAR RESERVA (GET)
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var model = new ReservaFormViewModel();

            // Autocompletar datos si hay usuario logueado
            var personaIdClaim = User.Claims.FirstOrDefault(c => c.Type == "PersonaId")?.Value;
            if (!string.IsNullOrEmpty(personaIdClaim) && int.TryParse(personaIdClaim, out int personaId) && personaId > 0)
            {
                var persona = await _context.Personas.FindAsync(personaId);
                if (persona != null)
                {
                    model.Nombre = persona.Nombre;
                    model.Telefono = persona.Telefono;
                    model.Email = persona.Correo;
                }
            }

            return View(model);
        }

        // CREAR RESERVA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ReservaFormViewModel model, string? origen = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Validar fecha y hora
                if (model.Fecha == default || string.IsNullOrEmpty(model.Hora))
                {
                    ModelState.AddModelError("", "Fecha u hora inválida.");
                    return View(model);
                }

                if (!TimeSpan.TryParse(model.Hora, out TimeSpan hora))
                {
                    ModelState.AddModelError("", "Formato de hora inválido.");
                    return View(model);
                }

                DateTime fechaHora = model.Fecha.Date.Add(hora);
                int numeroPersonas = model.Personas == "8+" ? 8 : int.Parse(model.Personas);

                Rest_Persona cliente;

                var personaIdClaim = User.Claims.FirstOrDefault(c => c.Type == "PersonaId")?.Value;
                if (!string.IsNullOrEmpty(personaIdClaim) && int.TryParse(personaIdClaim, out int personaId) && personaId > 0)
                {
                    cliente = await _context.Personas.FindAsync(personaId);
                    if (cliente == null)
                    {
                        cliente = new Rest_Persona
                        {
                            Nombre = model.Nombre,
                            Telefono = model.Telefono,
                            Correo = model.Email,
                            FechaRegistro = DateTime.Now
                        };
                        _context.Personas.Add(cliente);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    cliente = new Rest_Persona
                    {
                        Nombre = model.Nombre,
                        Telefono = model.Telefono,
                        Correo = model.Email,
                        FechaRegistro = DateTime.Now
                    };
                    _context.Personas.Add(cliente);
                    await _context.SaveChangesAsync();
                }

                var reserva = new Rest_Reserva
                {
                    FechaHora = fechaHora,
                    NumeroPersonas = numeroPersonas,
                    Observaciones = $"Ocasion: {model.Ocasion} | Comentarios: {model.Comentarios}",
                    Estado = "Pendiente",
                    FechaCreacion = DateTime.Now,
                    ClienteId = cliente.PersonaId,
                    MesaId = 1
                };

                _context.Reservas.Add(reserva);
                await _context.SaveChangesAsync();

                TempData["Success"] = "¡Reserva registrada correctamente!";

                if (origen == "Administrador")
                    return RedirectToAction("Index", "Reservas");
                else
                    return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                return View(model);
            }
        }

        // VER DETALLE DE RESERVA
        [HttpGet]
        public async Task<IActionResult> Detalles(int id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Cliente)
                .FirstOrDefaultAsync(r => r.ReservaId == id);

            if (reserva == null)
                return NotFound();

            var model = new ReservaFormViewModel
            {
                Nombre = reserva.Cliente?.Nombre ?? "",
                Telefono = reserva.Cliente?.Telefono ?? "",
                Email = reserva.Cliente?.Correo ?? "",
                Fecha = reserva.FechaHora.Date,
                Hora = reserva.FechaHora.ToString("HH:mm"),
                Personas = reserva.NumeroPersonas >= 8 ? "8+" : reserva.NumeroPersonas.ToString(),
                Ocasion = ExtraerValor(reserva.Observaciones, "Ocasion"),
                Comentarios = ExtraerValor(reserva.Observaciones, "Comentarios")
            };

            return View(model);
        }


        // EDITAR RESERVA (GET)
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Cliente)
                .FirstOrDefaultAsync(r => r.ReservaId == id);

            if (reserva == null)
                return NotFound();

            var model = new ReservaFormViewModel
            {
                Nombre = reserva.Cliente?.Nombre ?? "",
                Telefono = reserva.Cliente?.Telefono ?? "",
                Email = reserva.Cliente?.Correo ?? "",
                Fecha = reserva.FechaHora.Date,
                Hora = reserva.FechaHora.ToString("HH:mm"),
                Personas = reserva.NumeroPersonas >= 8 ? "8+" : reserva.NumeroPersonas.ToString(),
                Ocasion = ExtraerValor(reserva.Observaciones, "Ocasion"),
                Comentarios = ExtraerValor(reserva.Observaciones, "Comentarios")
            };

            return View(model);
        }

        // EDITAR RESERVA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ReservaFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var reserva = await _context.Reservas
                .Include(r => r.Cliente)
                .FirstOrDefaultAsync(r => r.ReservaId == id);

            if (reserva == null)
                return NotFound();

            try
            {
                reserva.FechaHora = model.Fecha.Date.Add(TimeSpan.Parse(model.Hora));
                reserva.NumeroPersonas = model.Personas == "8+" ? 8 : int.Parse(model.Personas);
                reserva.Observaciones = $"Ocasion: {model.Ocasion} | Comentarios: {model.Comentarios}";
                reserva.FechaCreacion = DateTime.Now;

                if (reserva.Cliente != null)
                {
                    reserva.Cliente.Nombre = model.Nombre;
                    reserva.Cliente.Telefono = model.Telefono;
                    reserva.Cliente.Correo = model.Email;
                }

                _context.Update(reserva);
                await _context.SaveChangesAsync();

                TempData["Success"] = "¡Reserva actualizada correctamente!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                return View(model);
            }
        }

        // CONFIRMACION
        public IActionResult Confirmacion()
        {
            ViewBag.Mensaje = TempData["Success"];
            return View();
        }

        // 🔹 MÉTODO AUXILIAR PARA EXTRAER DATOS DE "Observaciones"
        private string? ExtraerValor(string? observaciones, string clave)
        {
            if (string.IsNullOrEmpty(observaciones)) return null;

            var partes = observaciones.Split('|', StringSplitOptions.RemoveEmptyEntries);
            foreach (var parte in partes)
            {
                if (parte.Trim().StartsWith(clave + ":", StringComparison.OrdinalIgnoreCase))
                    return parte.Split(':')[1].Trim();
            }
            return null;
        }
    }
}
