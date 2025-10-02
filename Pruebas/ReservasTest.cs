using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Restaurant.Controllers;
using Restaurant.Models;
using Restaurant.ViewModels;
using RESTAURANT.Data;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Restaurant.Tests
{
    [TestClass]
    public class ReservasTest
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // BD en memoria
                .Options;

            return new AppDbContext(options);
        }

        private ClaimsPrincipal GetUser(int? personaId = null)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, "testuser") };

            if (personaId.HasValue)
                claims.Add(new Claim("PersonaId", personaId.Value.ToString()));

            var identity = new ClaimsIdentity(claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        // ================== ESCENARIOS EXITOSOS ==================

        [TestMethod]
        public async Task Get_Crear_ReturnsView_WithAutocompletedModel_WhenUserIsAuthenticated()
        {
            var context = GetDbContext();
            var persona = new Rest_Persona { PersonaId = 1, Nombre = "Juan", Telefono = "999", Correo = "juan@test.com" };
            context.Personas.Add(persona);
            await context.SaveChangesAsync();

            var controller = new ReservasController(context);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = GetUser(1) };

            var result = await controller.Crear() as ViewResult;
            var model = result?.Model as ReservaFormViewModel;

            Assert.IsNotNull(result);
            Assert.AreEqual("Juan", model?.Nombre);
            Assert.AreEqual("999", model?.Telefono);
            Assert.AreEqual("juan@test.com", model?.Email);
        }

        [TestMethod]
        public async Task Post_Crear_CreatesReserva_ForAuthenticatedUser()
        {
            var context = GetDbContext();
            var persona = new Rest_Persona { PersonaId = 1, Nombre = "Carlos", Telefono = "111", Correo = "carlos@test.com" };
            context.Personas.Add(persona);
            await context.SaveChangesAsync();

            var controller = new ReservasController(context);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = GetUser(1) };

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
            Assert.AreEqual("Confirmacion", result.ActionName);
            Assert.AreEqual(1, await context.Reservas.CountAsync());
        }

        [TestMethod]
        public async Task Post_Crear_CreatesNewPersona_WhenUserIsNotAuthenticated()
        {
            var context = GetDbContext();
            var controller = new ReservasController(context);
            controller.ControllerContext.HttpContext = new DefaultHttpContext(); // No autenticado

            var model = new ReservaFormViewModel
            {
                Nombre = "Lucia",
                Telefono = "123",
                Email = "lucia@test.com",
                Fecha = DateTime.Today,
                Hora = "20:00",
                Personas = "2",
                Ocasion = "Cumpleaños",
                Comentarios = "Mesa al fondo"
            };

            var result = await controller.Crear(model) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Confirmacion", result.ActionName);
            Assert.AreEqual(1, await context.Personas.CountAsync());
            Assert.AreEqual(1, await context.Reservas.CountAsync());
        }

        [TestMethod]
        public void Confirmacion_RedirectsToHomeIndex()
        {
            var context = GetDbContext();
            var controller = new ReservasController(context);

            var result = controller.Confirmacion() as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual("Home", result.ControllerName);
        }

        // ================== ESCENARIOS DE ERROR ==================

        [TestMethod]
        public async Task Post_Crear_ReturnsView_WhenModelIsInvalid()
        {
            var context = GetDbContext();
            var controller = new ReservasController(context);
            controller.ModelState.AddModelError("Nombre", "El nombre es obligatorio");

            var model = new ReservaFormViewModel
            {
                Telefono = "000",
                Email = "error@test.com"
                // Nombre faltante → Error
            };

            var result = await controller.Crear(model) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(model, result.Model); // Retorna el mismo modelo
            Assert.AreEqual(0, await context.Reservas.CountAsync()); // Nada guardado
        }

        [TestMethod]
        public async Task Post_Crear_ReturnsView_WhenPersonaNotFound()
        {
            var context = GetDbContext();
            var controller = new ReservasController(context);
            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = GetUser(99) }; // Persona inexistente

            var model = new ReservaFormViewModel
            {
                Nombre = "Error",
                Telefono = "000",
                Email = "error@test.com",
                Fecha = DateTime.Today,
                Hora = "18:00",
                Personas = "2"
            };

            var result = await controller.Crear(model) as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsFalse(controller.ModelState.IsValid);
            Assert.AreEqual(0, await context.Reservas.CountAsync());
        }

        [TestMethod]
        public async Task Post_Crear_ReturnsView_WhenPersonaIdClaimIsInvalid()
        {
            var context = GetDbContext();
            var controller = new ReservasController(context);

            // Usuario autenticado pero sin claim PersonaId
            controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "user") }, "TestAuth"))
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

            var result = await controller.Crear(model) as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsFalse(controller.ModelState.IsValid);
            Assert.AreEqual(0, await context.Reservas.CountAsync());
        }
    }
}
