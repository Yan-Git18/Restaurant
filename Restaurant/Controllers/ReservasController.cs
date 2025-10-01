using Microsoft.AspNetCore.Mvc;
using Restaurant.Models;
using Restaurant.ViewModels;
using RESTAURANT.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Restaurant.Controllers
{
    public class ReservasController : Controller
    {
        private readonly AppDbContext _context;

        public ReservasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var model = new ReservaFormViewModel();

            if (User.Identity!.IsAuthenticated)
            {
                // Obtener Persona asociada al usuario logueado
                var personaIdClaim = User.Claims.FirstOrDefault(c => c.Type == "PersonaId")?.Value;

                if (int.TryParse(personaIdClaim, out int personaId) && personaId > 0)
                {
                    var persona = await _context.Personas.FindAsync(personaId);

                    if (persona != null)
                    {
                        // Autocompletar datos
                        model.Nombre = persona.Nombre;
                        model.Telefono = persona.Telefono;
                        model.Email = persona.Correo;
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(ReservaFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Rest_Persona cliente;

            if (User.Identity!.IsAuthenticated)
            {
                // Buscar Persona del usuario logueado
                var personaIdClaim = User.Claims.FirstOrDefault(c => c.Type == "PersonaId")?.Value;

                if (int.TryParse(personaIdClaim, out int personaId) && personaId > 0)
                {
                    cliente = await _context.Personas.FindAsync(personaId);

                    if (cliente == null)
                    {
                        ModelState.AddModelError("", "No se encontró el cliente asociado.");
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "No se pudo obtener el cliente logueado.");
                    return View(model);
                }
            }
            else
            {
                // Crear nuevo cliente (persona sin usuario asociado)
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

            // Crear reserva
            var reserva = new Rest_Reserva
            {
                FechaHora = DateTime.Parse($"{model.Fecha:yyyy-MM-dd} {model.Hora}"),
                NumeroPersonas = model.Personas == "8+" ? 8 : int.Parse(model.Personas),
                Observaciones = $"Ocasion: {model.Ocasion} | Comentarios: {model.Comentarios}",
                Estado = "Pendiente",
                FechaCreacion = DateTime.Now,
                ClienteId = cliente.PersonaId,
                MesaId = 1 // luego deberías asignar mesa según disponibilidad
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            return RedirectToAction("Confirmacion");
        }

        public IActionResult Confirmacion()
        {
            return RedirectToAction("Index", "Home");
        }

    }
}