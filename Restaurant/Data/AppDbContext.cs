using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace RESTAURANT.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Rest_Categoria> Categorias { get; set; }
        public DbSet<Rest_Persona> Personas { get; set; }
        public DbSet<Rest_Comprobante> Comprobantes { get; set; }
        public DbSet<Rest_DetallePedido> DetallesPedido { get; set; }
        public DbSet<Rest_Inventario> Inventarios { get; set; }
        public DbSet<Rest_Mesa> Mesas { get; set; }
        public DbSet<Rest_Pago> Pagos { get; set; }
        public DbSet<Rest_Pedido> Pedidos { get; set; }
        public DbSet<Rest_Producto> Productos { get; set; }
        public DbSet<Rest_Reserva> Reservas { get; set; }
        public DbSet<Rest_Rol> Roles { get; set; }
        public DbSet<Rest_Usuario> Usuarios { get; set; }
        public DbSet<Rest_Venta> Ventas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== CONFIGURACIÓN DE PROPIEDADES =====

            // Fechas por defecto - SOLO para registros nuevos
            modelBuilder.Entity<Rest_Usuario>()
                .Property(u => u.FechaCreacion)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Rest_Persona>()
                .Property(c => c.FechaRegistro)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Rest_Pedido>()
                .Property(p => p.Fecha)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Rest_Venta>()
                .Property(v => v.Fecha)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Rest_Reserva>()
                .Property(r => r.FechaCreacion)
                .HasDefaultValueSql("GETDATE()");

            // Configuración de decimales para precios
            modelBuilder.Entity<Rest_Producto>()
                .Property(p => p.Precio)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Rest_Venta>()
                .Property(v => v.Total)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Rest_Venta>()
                .Property(v => v.Descuento)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Rest_Venta>()
                .Property(v => v.Impuesto)
                .HasColumnType("decimal(5,2)");

            modelBuilder.Entity<Rest_DetallePedido>()
                .Property(d => d.Subtotal)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Rest_Pago>()
                .Property(p => p.Monto)
                .HasColumnType("decimal(10,2)");

            // Configuración de longitudes máximas
            modelBuilder.Entity<Rest_Persona>()
                .Property(c => c.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Rest_Persona>()
                .Property(c => c.Correo)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Rest_Producto>()
                .Property(p => p.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Rest_Categoria>()
                .Property(c => c.Nombre)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Rest_Usuario>()
                .Property(u => u.Correo)
                .HasMaxLength(100)
                .IsRequired();

            // ===== ÍNDICES =====

            // Índices únicos
            modelBuilder.Entity<Rest_Usuario>()
                .HasIndex(u => u.Correo)
                .IsUnique();

            modelBuilder.Entity<Rest_Mesa>()
                .HasIndex(m => m.Numero)
                .IsUnique();

            // Índices compuestos para optimizar consultas
            modelBuilder.Entity<Rest_Pedido>()
                .HasIndex(p => new { p.ClienteId, p.Fecha });

            modelBuilder.Entity<Rest_Reserva>()
                .HasIndex(r => new { r.FechaHora, r.MesaId });

            // Para búsquedas de productos por categoría
            modelBuilder.Entity<Rest_Producto>()
                .HasIndex(p => new { p.CategoriaId, p.Disponible });

            // Para consultas de ventas por fecha
            modelBuilder.Entity<Rest_Venta>()
                .HasIndex(v => v.Fecha);

            // Para búsqueda de mesas por estado
            modelBuilder.Entity<Rest_Mesa>()
                .HasIndex(m => m.Estado);

            // Para control de stock (ahora en productos, no inventarios)
            modelBuilder.Entity<Rest_Producto>()
                .HasIndex(p => p.Stock);

            // ===== RELACIONES =====

            // Cliente - Reserva (1:N)
            modelBuilder.Entity<Rest_Persona>()
                .HasMany(c => c.Reservas)
                .WithOne(r => r.Cliente)
                .HasForeignKey(r => r.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Mesa - Reserva (1:N)
            modelBuilder.Entity<Rest_Mesa>()
                .HasMany(m => m.Reservas)
                .WithOne(r => r.Mesa)
                .HasForeignKey(r => r.MesaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Usuario - Cliente (1:1 opcional)
            modelBuilder.Entity<Rest_Usuario>()
                .HasOne(u => u.Cliente)
                .WithOne(c => c.Usuario)
                .HasForeignKey<Rest_Persona>(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Categoria - Producto (1:N)
            modelBuilder.Entity<Rest_Categoria>()
                .HasMany(c => c.Productos)
                .WithOne(p => p.Categoria)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Inventario - Productos (1:N)
            modelBuilder.Entity<Rest_Inventario>()
                .HasMany(i => i.Productos)
                .WithOne(p => p.Inventario)
                .HasForeignKey(p => p.InventarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Rol - Usuario (1:N)
            modelBuilder.Entity<Rest_Rol>()
                .HasMany(r => r.Usuarios)
                .WithOne(u => u.Rol)
                .HasForeignKey(u => u.RolId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cliente - Pedido (1:N)
            modelBuilder.Entity<Rest_Persona>()
                .HasMany(c => c.Pedidos)
                .WithOne(p => p.Cliente)
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Mesa - Pedido (1:N) - Puede ser null para pedidos para llevar
            modelBuilder.Entity<Rest_Mesa>()
                .HasMany(m => m.Pedidos)
                .WithOne(p => p.Mesa)
                .HasForeignKey(p => p.MesaId)
                .OnDelete(DeleteBehavior.SetNull);

            // Pedido - DetallePedido (1:N)
            modelBuilder.Entity<Rest_Pedido>()
                .HasMany(p => p.DetallesPedido)
                .WithOne(d => d.Pedido)
                .HasForeignKey(d => d.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Producto - DetallePedido (1:N)
            modelBuilder.Entity<Rest_Producto>()
                .HasMany(p => p.DetallesPedido)
                .WithOne(d => d.Producto)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Pedido - Venta (1:1)
            modelBuilder.Entity<Rest_Pedido>()
                .HasOne(p => p.Venta)
                .WithOne(v => v.Pedido)
                .HasForeignKey<Rest_Venta>(v => v.PedidoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Venta - Pago (1:N) - Una venta puede tener múltiples pagos parciales
            modelBuilder.Entity<Rest_Venta>()
                .HasMany(v => v.Pagos)
                .WithOne(p => p.Venta)
                .HasForeignKey(p => p.VentaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Venta - Comprobante (1:1)
            modelBuilder.Entity<Rest_Venta>()
                .HasOne(v => v.Comprobante)
                .WithOne(c => c.Venta)
                .HasForeignKey<Rest_Comprobante>(c => c.VentaId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== DATOS INICIALES =====

            // Roles
            modelBuilder.Entity<Rest_Rol>().HasData(
                new Rest_Rol { RolId = 1, Nombre = "Administrador" },
                new Rest_Rol { RolId = 2, Nombre = "Cliente" }
            );

            // Usuarios
            modelBuilder.Entity<Rest_Usuario>().HasData(
                new Rest_Usuario
                {
                    UsuarioId = 1,
                    Correo = "admin@delizioso.com",
                    PasswordHash = "admin123",
                    RolId = 1,
                    FechaCreacion = new DateTime(2025, 09, 27, 16, 41, 0),
                    Activo = true
                },
                new Rest_Usuario
                {
                    UsuarioId = 2,
                    Correo = "cliente@gmail.com",
                    PasswordHash = "cliente123",
                    RolId = 2,
                    FechaCreacion = new DateTime(2025, 09, 27, 16, 41, 0),
                    Activo = true
                }
            );

            // Mesas
            modelBuilder.Entity<Rest_Mesa>().HasData(
                new Rest_Mesa { MesaId = 1, Numero = 1, Capacidad = 4, Estado = "Libre" },
                new Rest_Mesa { MesaId = 2, Numero = 2, Capacidad = 2, Estado = "Libre" },
                new Rest_Mesa { MesaId = 3, Numero = 3, Capacidad = 6, Estado = "Libre" },
                new Rest_Mesa { MesaId = 4, Numero = 4, Capacidad = 4, Estado = "Libre" },
                new Rest_Mesa { MesaId = 5, Numero = 5, Capacidad = 8, Estado = "Libre" }
            );

            // Categorías
            modelBuilder.Entity<Rest_Categoria>().HasData(
                new Rest_Categoria { Id = 1, Nombre = "Bebidas" }
            );

            // Inventarios (ya sin Stock)
            modelBuilder.Entity<Rest_Inventario>().HasData(
                new Rest_Inventario
                {
                    Id = 1,
                    Nombre = "Bebidas",
                    UnidadMedida = "Litro",
                    StockMinimo = 5,
                    FechaActualizacion = new DateTime(2025, 09, 27, 12, 0, 0)
                }
            );

            // Productos con stock inicial
            modelBuilder.Entity<Rest_Producto>().HasData(
                new Rest_Producto
                {
                    Id = 1,
                    Nombre = "Agua Mineral",
                    Precio = 2.50m,
                    CategoriaId = 1,
                    InventarioId = 1,
                    Disponible = true,
                    Stock = 5,
                    FechaCreacion = new DateTime(2025, 09, 27, 12, 0, 0)
                },
                new Rest_Producto
                {
                    Id = 2,
                    Nombre = "Refresco",
                    Precio = 4.00m,
                    CategoriaId = 1,
                    InventarioId = 1,
                    Disponible = true,
                    Stock = 7,
                    FechaCreacion = new DateTime(2025, 09, 27, 12, 0, 0)
                },
                new Rest_Producto
                {
                    Id = 3,
                    Nombre = "Jugo Natural",
                    Precio = 6.00m,
                    CategoriaId = 1,
                    InventarioId = 1,
                    Disponible = true,
                    Stock = 8,
                    FechaCreacion = new DateTime(2025, 09, 27, 12, 0, 0)
                }
            );
        }
    }
}
