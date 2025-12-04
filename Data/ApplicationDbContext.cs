using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PRUEBATECNICA.STEFFANINIGROUP.INVENTARIO.Models;

namespace PRUEBATECNICA.STEFFANINIGROUP.INVENTARIO.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // API-PRODUCTOS - Inventario de Productos
        public DbSet<Producto> Productos { get; set; }

        // API-VENTAS - Encabezado y Detalle de Ventas
        public DbSet<EncabezadoVenta> EncabezadoVentas { get; set; }
        public DbSet<DetalleVenta> DetalleVentas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Producto
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(e => e.IdPro);
                entity.Property(e => e.ProductoNombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Precio).HasColumnType("decimal(18,2)");
            });

            // Configuración de EncabezadoVenta
            modelBuilder.Entity<EncabezadoVenta>(entity =>
            {
                entity.HasKey(e => e.IdVenta);
                entity.Property(e => e.Vendedor).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            });

            // Configuración de DetalleVenta
            modelBuilder.Entity<DetalleVenta>(entity =>
            {
                entity.HasKey(e => e.IdDe);
                entity.Property(e => e.Precio).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Iva).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Total).HasColumnType("decimal(18,2)");

                // Relación con EncabezadoVenta
                entity.HasOne(d => d.EncabezadoVenta)
                    .WithMany(e => e.DetallesVenta)
                    .HasForeignKey(d => d.IdVenta)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación con Producto
                entity.HasOne(d => d.Producto)
                    .WithMany(p => p.DetallesVenta)
                    .HasForeignKey(d => d.IdPro)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
