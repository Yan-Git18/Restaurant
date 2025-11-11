using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Restaurant.Controllers;
using Restaurant.Models;
using Restaurant.ViewModels;
using RESTAURANT.Data;
using System.Security.Claims;
using Moq;

namespace Restaurant.Tests
{
    [TestClass]
    public class ReservasControllerTest
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private ClaimsPrincipal GetUser(string rol = "Cliente", int? personaId = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.Role, rol)
            };

            if (personaId.HasValue)
                claims.Add(new Claim("PersonaId", personaId.Value.ToString()));

            var identity = new ClaimsIdentity(claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        // -----------------------------------------
        // PRUEBAS GET
        // -----------------------------------------

        [TestMethod]
        public async Task Get_Crear_DeberiaRetornarVistaConDatosPrecargados_CuandoUsuarioEsCliente()
        {
            var context = GetDbContext();
            var persona = new Rest_Persona
            {
                PersonaId = 1,
                Nombre = "Juan",
                Telefono = "999",
                Correo = "juan@test.com"
            };
            context.Personas.Add(persona);
            await context.SaveChangesAsync();

            var controller = new ReservasController(context);
            controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = GetUser("Cliente", 1)
            };

            var result = await controller.Crear() as ViewResult;
            var model = result?.Model as ReservaFormViewModel;

            Assert.IsNotNull(result);
            Assert.AreEqual("Juan", model?.Nombre);
            Assert.AreEqual("999", model?.Telefono);
            Assert.AreEqual("juan@test.com", model?.Email);
        }


        [TestMethod]
        public async Task Post_Crear_DeberiaCrearReserva_CuandoClienteAutenticado()
        {
            var context = GetDbContext();

            var persona = new Rest_Persona
            {
                PersonaId = 1,
                Nombre = "Carlos",
                Telefono = "111",
                Correo = "carlos@test.com"
            };
            context.Personas.Add(persona);
            await context.SaveChangesAsync();

            var controller = new ReservasController(context);

            var user = GetUser("Cliente", 1);
            var httpContext = new DefaultHttpContext { User = user };

            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            controller.TempData = tempData;
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var model = new ReservaFormViewModel
            {
                Nombre = "Carlos",
                Telefono = "111",
                Email = "carlos@test.com",
                Fecha = DateTime.Today,
                Hora = "19:00",
                Personas = "4",
                Ocasion = "Cena",
                Comentarios = "Sin gluten"
            };

            var result = await controller.Crear(model) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("MisReservas", result.ActionName);
            Assert.AreEqual(1, await context.Reservas.CountAsync());
        }

        [TestMethod]
        public async Task Post_Crear_DeberiaCrearReserva_CuandoAdminCreaParaCliente()
        {
            var context = GetDbContext();

            var rol = new Rest_Rol { RolId = 1, Nombre = "Cliente" };
            var usuario = new Rest_Usuario { UsuarioId = 1, RolId = 1, Rol = rol, Correo = "cli@test.com", PasswordHash = "123" };
            var persona = new Rest_Persona { PersonaId = 1, Nombre = "Ana", Telefono = "999", Correo = "ana@test.com", Usuario = usuario };

            context.Roles.Add(rol);
            context.Usuarios.Add(usuario);
            context.Personas.Add(persona);
            await context.SaveChangesAsync();

            var controller = new ReservasController(context);

            var user = GetUser("Administrador");
            var httpContext = new DefaultHttpContext { User = user };

            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var model = new ReservaFormViewModel
            {
                ClienteId = 1,
                Nombre = "Ana",
                Telefono = "999",
                Email = "ana@test.com",
                Fecha = DateTime.Today,
                Hora = "18:00",
                Personas = "2",
                Ocasion = "Cena",
                Comentarios = "Tranquilo"
            };

            var result = await controller.Crear(model) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual(1, await context.Reservas.CountAsync());
        }

        [TestMethod]
        public async Task Post_Crear_DeberiaRetornarVistaInicio_CuandoModeloInvalido()
        {
            var context = GetDbContext();
            var controller = new ReservasController(context);
            controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = GetUser("Cliente", 1)
            };
            controller.ModelState.AddModelError("Nombre", "El nombre es obligatorio");

            var model = new ReservaFormViewModel
            {
                Telefono = "000",
                Email = "error@test.com"
            };

            var result = await controller.Crear(model) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("~/Views/Home/Index.cshtml", result.ViewName);
            Assert.AreEqual(0, await context.Reservas.CountAsync());
        }

        [TestMethod]
        public async Task Post_Crear_DeberiaRedirigirALogin_CuandoClienteNoTienePersonaId()
        {
            var context = GetDbContext();
            var controller = new ReservasController(context);
            controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = GetUser("Cliente", personaId: null)
            };

            var model = new ReservaFormViewModel
            {
                Nombre = "Error",
                Telefono = "000",
                Email = "error@test.com",
                Fecha = DateTime.Today,
                Hora = "18:00",
                Personas = "2"
            };

            var result = await controller.Crear(model) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Login", result.ActionName);
            Assert.AreEqual("Cuenta", result.ControllerName);
        }

        [TestMethod]
        public async Task Post_Crear_DeberiaGuardarReserva_CuandoClienteValido()
        {
            var context = GetDbContext();

            var persona = new Rest_Persona { PersonaId = 2, Nombre = "Luis", Telefono = "123", Correo = "luis@test.com" };
            context.Personas.Add(persona);
            await context.SaveChangesAsync();

            var controller = new ReservasController(context);
            var user = GetUser("Cliente", 2);
            var httpContext = new DefaultHttpContext { User = user };
            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var model = new ReservaFormViewModel
            {
                Nombre = "Luis",
                Telefono = "123",
                Email = "luis@test.com",
                Fecha = DateTime.Today.AddDays(1),
                Hora = "20:00",
                Personas = "3",
                Ocasion = "Cumpleaños",
                Comentarios = "Mesa cerca de ventana"
            };

            var result = await controller.Crear(model) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("MisReservas", result.ActionName);
            Assert.AreEqual(1, await context.Reservas.CountAsync());
        }

        [TestMethod]
        public async Task Post_Crear_DeberiaGuardarReserva_CuandoAdminCreaParaClienteExistente()
        {
            var context = GetDbContext();

            var persona = new Rest_Persona { PersonaId = 10, Nombre = "Andrea", Telefono = "555", Correo = "andrea@test.com" };
            context.Personas.Add(persona);
            await context.SaveChangesAsync();

            var controller = new ReservasController(context);
            var user = GetUser("Administrador");
            var httpContext = new DefaultHttpContext { User = user };
            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var model = new ReservaFormViewModel
            {
                ClienteId = 10,
                Nombre = "Andrea",
                Telefono = "555",
                Email = "andrea@test.com",
                Fecha = DateTime.Today.AddDays(2),
                Hora = "13:00",
                Personas = "2",
                Ocasion = "Reunión",
                Comentarios = "Mesa privada"
            };

            var result = await controller.Crear(model) as RedirectToActionResult;

            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual(1, await context.Reservas.CountAsync());
        }


        [TestMethod]
        public async Task Post_CancelarAdmin_DeberiaCambiarEstadoACancelada_CuandoReservaExiste()
        {
            var context = GetDbContext();

            var reserva = new Rest_Reserva
            {
                ReservaId = 1,
                Estado = "Pendiente",
                ClienteId = 1,
                FechaHora = DateTime.Now
            };
            context.Reservas.Add(reserva);
            await context.SaveChangesAsync();

            var controller = new ReservasController(context);
            var user = GetUser("Administrador");
            var httpContext = new DefaultHttpContext { User = user };

            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await controller.CancelarAdmin(1) as RedirectToActionResult;
            var updated = await context.Reservas.FirstAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Cancelada", updated.Estado);
        }

        [TestMethod]
        public async Task Post_CancelarAdmin_DeberiaCambiarEstadoACancelada_CuandoReservaPendiente()
        {
            var context = GetDbContext();

            var reserva = new Rest_Reserva
            {
                ReservaId = 3,
                Estado = "Pendiente",
                ClienteId = 1,
                FechaHora = DateTime.Now.AddHours(3)
            };
            context.Reservas.Add(reserva);
            await context.SaveChangesAsync();

            var controller = new ReservasController(context);
            var user = GetUser("Administrador");
            var httpContext = new DefaultHttpContext { User = user };
            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await controller.CancelarAdmin(3) as RedirectToActionResult;
            var actual = await context.Reservas.FindAsync(3);

            Assert.AreEqual("Cancelada", actual.Estado);
            Assert.AreEqual("Index", result.ActionName);
        }

        [TestMethod]
        public async Task Post_CancelarAdmin_DeberiaRetornarNotFound_CuandoReservaNoExiste()
        {
            var context = GetDbContext();

            var controller = new ReservasController(context);
            var user = GetUser("Administrador");
            var httpContext = new DefaultHttpContext { User = user };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await controller.CancelarAdmin(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}