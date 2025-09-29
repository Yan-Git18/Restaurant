using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Crear(ReservaFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Crear cliente con los datos del formulario
            var cliente = new Rest_Persona
            {
                Nombre = model.Nombre,
                Telefono = model.Telefono
            };

            // Crear reserva
            var reserva = new Rest_Reserva
            {
                FechaHora = DateTime.Parse($"{model.Fecha:yyyy-MM-dd} {model.Hora}"),
                NumeroPersonas = model.Personas == "8+" ? 8 : int.Parse(model.Personas),
                Observaciones = $"Ocasion: {model.Ocasion} | Comentarios: {model.Comentarios}",
                Estado = "Pendiente",
                FechaCreacion = DateTime.Now,
                Cliente = cliente,
                MesaId = 1
            };

            _context.Reservas.Add(reserva);
            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Confirmacion()
        {
            return View();
        }
    }
}