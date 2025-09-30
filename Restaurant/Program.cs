using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RESTAURANT.Data;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Configurar la conexión a la base de datos
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CadenaSql")));


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login"; // Ruta personalizada para la página de inicio de sesión
        options.Cookie.Name = "DeliziosoAuth"; // Nombre personalizado para la cookie de autenticación
        options.AccessDeniedPath = "/Cuenta/AccessDenied"; // Ruta personalizada para la página de acceso denegado
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10); // Tiempo de expiración de la cookie
    });


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Autenticación

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
