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
            return View(reservas); // Asegúrate de que la vista Index reciba List<Rest_Reserva>
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

                // 🔥 Redirección según origen
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


        // CONFIRMACION
        public IActionResult Confirmacion()
        {
            ViewBag.Mensaje = TempData["Success"];
            return View();
        }
    }
}
