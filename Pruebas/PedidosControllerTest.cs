using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Restaurant.Controllers;
using Restaurant.Models;
using Restaurant.ViewModels;
using RESTAURANT.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Restaurant.Tests
{
    [TestClass]
    public class PedidosControllerTest
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private ClaimsPrincipal GetUser(string rol, int? personaId = null)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, rol) };
            if (personaId.HasValue)
                claims.Add(new Claim("PersonaId", personaId.Value.ToString()));

            var identity = new ClaimsIdentity(claims, "TestAuth");
            return new ClaimsPrincipal(identity);
        }

        private PedidosController GetController(AppDbContext context, string rol = "Administrador")
        {
            var controller = new PedidosController(context);
            var user = GetUser(rol, 1);
            var httpContext = new DefaultHttpContext { User = user };
            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            return controller;
        }


        [TestMethod]
        public async Task Post_Crear_SinDetalles_RetornaVistaConError()
        {
            var context = GetDbContext();
            var controller = new PedidosController(context);

            var cliente = new Rest_Persona
            {
                PersonaId = 1,
                Nombre = "Juan",
                Apellidos = "Pérez",
                Correo = "juan@example.com",
                Telefono = "987654321"
            };
            context.Personas.Add(cliente);
            await context.SaveChangesAsync();

            var vm = new PedidoCreateEditVm
            {
                ClienteId = cliente.PersonaId,
                MesaId = null,
                TipoPedido = "Mesa",
                Detalles = new List<DetallePedidoVm>()
            };

            var result = await controller.Crear(vm) as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsFalse(controller.ModelState.IsValid);
            Assert.IsTrue(controller.ModelState.ErrorCount > 0);
            Assert.AreEqual("", controller.ModelState.First().Key);
        }


        [TestMethod]
        public async Task Post_Crear_ModeloNulo_RetornaBadRequest()
        {
            var context = GetDbContext();
            var controller = GetController(context);

            var result = await controller.Crear(null);

            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }


        [TestMethod]
        public async Task Post_Editar_PedidoPendiente_ActualizaCorrectamente()
        {
            var context = GetDbContext();

            var pedido = new Rest_Pedido
            {
                Id = 1,
                ClienteId = 1,
                Estado = "Pendiente",
                Fecha = DateTime.Now,
                DetallesPedido = new List<Rest_DetallePedido>
                {
                    new Rest_DetallePedido { ProductoId = 10, Cantidad = 1, Subtotal = 10 }
                }
            };

            context.Pedidos.Add(pedido);
            context.Productos.Add(new Rest_Producto { Id = 10, Nombre = "Pizza", Precio = 10, Disponible = true, Activo = true });
            await context.SaveChangesAsync();

            var controller = GetController(context);

            var vm = new PedidoCreateEditVm
            {
                Id = 1,
                ClienteId = 1,
                Estado = "Pendiente",
                TipoPedido = "Mesa",
                Detalles = new List<DetallePedidoVm>
                {
                    new DetallePedidoVm { ProductoId = 10, Cantidad = 3, Precio = 10 }
                },
                Observaciones = "Extra queso"
            };

            var result = await controller.Editar(1, vm) as RedirectToActionResult;

            var actualizado = await context.Pedidos.FirstAsync();
            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual(30, actualizado.Total);
        }

        [TestMethod]
        public async Task Post_Editar_PedidoAtendido_NoPermiteEdicion()
        {
            var context = GetDbContext();
            var pedido = new Rest_Pedido { Id = 5, ClienteId = 1, Estado = "Atendido" };
            context.Pedidos.Add(pedido);
            await context.SaveChangesAsync();

            var controller = GetController(context);
            var vm = new PedidoCreateEditVm { Id = 5, ClienteId = 1, Estado = "Atendido" };

            var result = await controller.Editar(5, vm) as RedirectToActionResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.ActionName);
        }

        [TestMethod]
        public async Task Post_Editar_PedidoNoExiste_RetornaNotFound()
        {
            var context = GetDbContext();
            var controller = GetController(context);

            var vm = new PedidoCreateEditVm { Id = 100, ClienteId = 1 };
            var result = await controller.Editar(100, vm);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }


        [TestMethod]
        public async Task Get_Detalles_PedidoExiste_DevuelveVista()
        {
            // Arrange
            var context = GetDbContext();
            var controller = new PedidosController(context);

            // Crear cliente
            var cliente = new Rest_Persona
            {
                PersonaId = 1,
                Nombre = "Carlos",
                Apellidos = "Lopez",
                Correo = "carlos@example.com",
                Telefono = "999888777"
            };
            context.Personas.Add(cliente);

            // Crear producto
            var producto = new Rest_Producto
            {
                Id = 1,
                Nombre = "Pizza",
                Precio = 25,
                Disponible = true,
                Activo = true
            };
            context.Productos.Add(producto);

            // Crear pedido
            var pedido = new Rest_Pedido
            {
                Id = 1,
                ClienteId = cliente.PersonaId,
                Fecha = DateTime.Now,
                Estado = "Pendiente",
                TipoPedido = "Mesa",
                DetallesPedido = new List<Rest_DetallePedido>
        {
            new Rest_DetallePedido
            {
                ProductoId = producto.Id,
                Cantidad = 2,
                Subtotal = 50
            }
        }
            };
            context.Pedidos.Add(pedido);
            await context.SaveChangesAsync();

            var result = await controller.Detalles(1) as ViewResult;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(Rest_Pedido));
            var pedidoResult = result.Model as Rest_Pedido;
            Assert.AreEqual(1, pedidoResult.Id);
        }


        [TestMethod]
        public async Task Get_Detalles_PedidoNoExiste_RetornaNotFound()
        {
            var context = GetDbContext();
            var controller = GetController(context);

            var result = await controller.Detalles(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }


        [TestMethod]
        public async Task Post_EliminarConfirmar_PedidoSinVenta_EliminaYRedirige()
        {
            var context = GetDbContext();
            var pedido = new Rest_Pedido { Id = 20, ClienteId = 1, Estado = "Pendiente" };
            context.Pedidos.Add(pedido);
            await context.SaveChangesAsync();

            var controller = GetController(context);
            var result = await controller.EliminarConfirmar(20) as RedirectToActionResult;

            Assert.AreEqual("Index", result.ActionName);
            Assert.AreEqual(0, await context.Pedidos.CountAsync());
        }

        [TestMethod]
        public async Task Post_EliminarConfirmar_PedidoNoExiste_RetornaNotFound()
        {
            var context = GetDbContext();
            var controller = GetController(context);

            var result = await controller.EliminarConfirmar(999);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

    }
}