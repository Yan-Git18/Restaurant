using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;
using Restaurant.ViewModels;
using RESTAURANT.Data;
using System.Security.Claims;

namespace Restaurant.Controllers
{    
    public class CuentaController : Controller
    {
        private readonly AppDbContext _context;

        public CuentaController(AppDbContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Cliente)
                .FirstOrDefaultAsync(u => u.Correo == model.Email && u.Activo);

            if (usuario == null)
            {
                TempData["Mensaje"] = "Usuario no encontrado";
                TempData["Tipo"] = "error";
                return View(model);
            }

            if (!usuario.Activo)
            {
                TempData["Mensaje"] = "Tu cuenta está desactivada. Contacta al administrador";
                TempData["Tipo"] = "error";
                return View(model);
            }

            bool passwordValido = false;

            // Verificar si es hash de BCrypt o texto plano
            if (usuario.PasswordHash.StartsWith("$2a$") || usuario.PasswordHash.StartsWith("$2b$"))
            {
                // Es un hash de BCrypt (usuarios registrados)
                passwordValido = BCrypt.Net.BCrypt.Verify(model.Password, usuario.PasswordHash);
            }
            else
            {
                // Es texto plano (usuarios iniciales de la BD)
                passwordValido = usuario.PasswordHash == model.Password;
            }

            if (!passwordValido)
            {
                TempData["Mensaje"] = "Correo o contraseña incorrectos";
                TempData["Tipo"] = "error";
                return View(model);
            }

            // Actualizar último acceso
            usuario.UltimoAcceso = DateTime.Now;
            await _context.SaveChangesAsync();

            // Obtener nombre para mostrar
            string nombreUsuario = usuario.Cliente?.Nombre ?? usuario.Correo;

            // Configurar claims
            var claims = new List<Claim>
            {
                new Claim("UserId", usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol.Nombre),
                new Claim("PersonaId", usuario.Cliente?.PersonaId.ToString() ?? "0"),
                new Claim(ClaimTypes.Name, nombreUsuario)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var properties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                AllowRefresh = true,
                ExpiresUtc = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : DateTimeOffset.UtcNow.AddMinutes(10)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                properties);

            TempData["Mensaje"] = $"Bienvenido, {nombreUsuario}";
            TempData["Tipo"] = "success";

            // Redirigir según rol
            return usuario.Rol.Nombre switch
            {
                "Administrador" => RedirectToAction("Index", "Admin"),
                //"Cliente" => RedirectToAction("Index", "Home"),
                _ => RedirectToAction("Index", "Admin")
            };
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Admin");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verificar si el email ya existe
            var existeUsuario = await _context.Usuarios
                .AnyAsync(u => u.Correo == model.Email);

            if (existeUsuario)
            {
                ModelState.AddModelError("Email", "Este correo ya está registrado");
                TempData["Mensaje"] = "Este correo ya está registrado";
                TempData["Tipo"] = "error";
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Crear usuario con contraseña encriptada
                var usuario = new Rest_Usuario
                {
                    Correo = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    RolId = 2, // Rol Cliente por defecto
                    FechaCreacion = DateTime.Now,
                    Activo = true
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // Crear persona asociada al usuario
                var persona = new Rest_Persona
                {
                    Nombre = model.Nombre,
                    Telefono = model.Telefono,
                    Apellidos = model.Apellidos,
                    Correo = model.Email,
                    UsuarioId = usuario.UsuarioId,
                    FechaRegistro = DateTime.Now
                };

                _context.Personas.Add(persona);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Mensaje"] = "Cuenta creada exitosamente. Ya puedes iniciar sesión";
                TempData["Tipo"] = "success";

                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Error al crear la cuenta. Intenta nuevamente");
                TempData["Mensaje"] = "Error al crear la cuenta. Intenta nuevamente";
                TempData["Tipo"] = "error";
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("DeliziosoAuth");

            TempData["Mensaje"] = "Sesión cerrada exitosamente";
            TempData["Tipo"] = "success";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            TempData["Mensaje"] = "Acceso denegado. No tienes permisos para esta página";
            TempData["Tipo"] = "error";
            return RedirectToAction("Index", "Admin");
            
        }

        public IActionResult Perfil()
        {
            return View();
        }
    }
}