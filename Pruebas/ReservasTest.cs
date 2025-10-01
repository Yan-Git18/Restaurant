//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Restaurant.Controllers;
//using RESTAURANT.Data;
//using Restaurant.ViewModels;
//using System;

//namespace Restaurant.Tests.Controllers
//{
//    [TestClass]
//    public class ReservasControllerTests
//    {
//        private AppDbContext _context;
//        private ReservasController _controller;

//        [TestInitialize]
//        public void Setup()
//        {
//            var options = new DbContextOptionsBuilder<AppDbContext>()
//                .UseInMemoryDatabase(databaseName: "ReservasTestDb")
//                .Options;

//            _context = new AppDbContext(options);
//            _controller = new ReservasController(_context);
//        }

//        [TestCleanup]
//        public void Cleanup()
//        {
//            _context.Database.EnsureDeleted();
//            _context.Dispose();
//        }

//        [TestMethod]
//        public void Crear_Get_DeberiaRetornarVista()
//        {
//            // Act
//            var result = _controller.Crear() as ViewResult;

//            // Assert
//            Assert.IsNotNull(result);
//            Assert.IsNull(result.Model);
//        }

//        [TestMethod]
//        public void Crear_Post_ModeloInvalido_DeberiaRetornarVistaConModelo()
//        {
//            // Arrange
//            var model = new ReservaFormViewModel();
//            _controller.ModelState.AddModelError("Nombre", "El nombre es requerido");

//            // Act
//            var result = _controller.Crear(model) as ViewResult;

//            // Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual(model, result.Model);
//        }

//        [TestMethod]
//        public void Crear_Post_ModeloValido_DeberiaGuardarReservaYRedirigir()
//        {
//            // Arrange
//            var model = new ReservaFormViewModel
//            {
//                Nombre = "Juan Pérez",
//                Telefono = "987654321",
//                Fecha = DateTime.Today,
//                Hora = "19:30",
//                Personas = "4",
//                Ocasion = "Cumpleaños",
//                Comentarios = "Mesa cerca a la ventana"
//            };

//            // Act
//            var result = _controller.Crear(model) as RedirectToActionResult;

//            // Assert
//            Assert.IsNotNull(result);
//            Assert.AreEqual("Index", result.ActionName);
//            Assert.AreEqual("Home", result.ControllerName);

//            var reserva = _context.Reservas.Include(r => r.Cliente).FirstOrDefault();
//            Assert.IsNotNull(reserva);
//            Assert.AreEqual("Juan Pérez", reserva.Cliente.Nombre);
//            Assert.AreEqual("987654321", reserva.Cliente.Telefono);
//            Assert.AreEqual(4, reserva.NumeroPersonas);
//            StringAssert.Contains(reserva.Observaciones, "Cumpleaños");
//            StringAssert.Contains(reserva.Observaciones, "Mesa cerca a la ventana");
//            Assert.AreEqual("Pendiente", reserva.Estado);
//        }

//        [TestMethod]
//        public void Confirmacion_DeberiaRetornarVista()
//        {
//            // Act
//            var result = _controller.Confirmacion() as ViewResult;

//            // Assert
//            Assert.IsNotNull(result);
//        }
//    }
//}