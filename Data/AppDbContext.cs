using Microsoft.EntityFrameworkCore;
using restapi.inventarios.Entities;

namespace restapi.inventarios.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Producto> Productos => Set<Producto>();
        public DbSet<EncabezadoVenta> EncabezadoVentas => Set<EncabezadoVenta>();
        public DbSet<DetalleVenta> DetalleVentas => Set<DetalleVenta>();
        public DbSet<LogProducto> LogsProductos => Set<LogProducto>();

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<EndpointPermission> EndpointPermissions => Set<EndpointPermission>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Productos
            modelBuilder.Entity<Producto>(b =>
            {
                b.ToTable("Productos");
                b.HasKey(p => p.Id);
                b.Property(p => p.Id).HasColumnName("idpro");
                b.Property(p => p.Nombre).HasColumnName("producto").HasMaxLength(200).IsRequired();
                b.Property(p => p.Precio).HasColumnName("precio").HasColumnType("decimal(18,2)").IsRequired();
            });

            // EncabezadoVentas
            modelBuilder.Entity<EncabezadoVenta>(b =>
            {
                b.ToTable("EncabezadoVentas");
                b.HasKey(e => e.Id);
                b.Property(e => e.Id).HasColumnName("idventa");
                b.Property(e => e.Fecha).HasColumnName("fecha");
                b.Property(e => e.Vendedor).HasColumnName("vendedor").HasMaxLength(150).IsRequired();
                b.Property(e => e.Total).HasColumnName("total").HasColumnType("decimal(18,2)").IsRequired();
                b.HasMany(e => e.Detalles).WithOne(d => d.Encabezado).HasForeignKey(d => d.IdVenta).OnDelete(DeleteBehavior.Cascade);
            });

            // DetalleVentas
            modelBuilder.Entity<DetalleVenta>(b =>
            {
                b.ToTable("DetalleVentas");
                b.HasKey(d => d.Id);
                b.Property(d => d.Id).HasColumnName("idde");
                b.Property(d => d.Fecha).HasColumnName("fecha");
                b.Property(d => d.IdVenta).HasColumnName("idventa");
                b.Property(d => d.IdProducto).HasColumnName("idpro");
                b.Property(d => d.Cantidad).HasColumnName("cantidad");
                b.Property(d => d.Precio).HasColumnName("precio").HasColumnType("decimal(18,2)");
                b.Property(d => d.Iva).HasColumnName("iva").HasColumnType("decimal(18,2)");
                b.Property(d => d.Total).HasColumnName("total").HasColumnType("decimal(18,2)");
                b.HasOne(d => d.Producto).WithMany().HasForeignKey(d => d.IdProducto).OnDelete(DeleteBehavior.NoAction);
            });

            // LogsProductos
            modelBuilder.Entity<LogProducto>(b =>
            {
                b.ToTable("LogsProductos");
                b.HasKey(l => l.Id);
                b.Property(l => l.Id).HasColumnName("idlog");
                b.Property(l => l.Operacion).HasColumnName("operacion").HasMaxLength(20);
                b.Property(l => l.IdProducto).HasColumnName("idpro");
                b.Property(l => l.ProductoOld).HasColumnName("producto_old").HasMaxLength(200);
                b.Property(l => l.PrecioOld).HasColumnName("precio_old").HasColumnType("decimal(18,2)");
                b.Property(l => l.ProductoNew).HasColumnName("producto_new").HasMaxLength(200);
                b.Property(l => l.PrecioNew).HasColumnName("precio_new").HasColumnType("decimal(18,2)");
                b.Property(l => l.Fecha).HasColumnName("fecha").HasColumnType("datetime2");
                b.Property(l => l.Fecha)
                    .HasDefaultValueSql("GETDATE()");
            });

            // Users
            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("Users");
                b.HasKey(u => u.Id);
                b.Property(u => u.Username).HasMaxLength(100).IsRequired();
                b.HasIndex(u => u.Username).IsUnique();
                b.Property(u => u.PasswordHash).HasColumnType("varbinary(64)").IsRequired();
                b.Property(u => u.PasswordSalt).HasColumnType("varbinary(32)").IsRequired();
                b.Property(u => u.IsActive).HasDefaultValue(true);
                b.Property(u => u.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
            });

            // Roles
            modelBuilder.Entity<Role>(b =>
            {
                b.ToTable("Roles");
                b.HasKey(r => r.Id);
                b.Property(r => r.Name).HasMaxLength(100).IsRequired();
                b.HasIndex(r => r.Name).IsUnique();
            });

            // UserRoles
            modelBuilder.Entity<UserRole>(b =>
            {
                b.ToTable("UserRoles");
                b.HasKey(ur => new { ur.UserId, ur.RoleId });
                b.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId).OnDelete(DeleteBehavior.Cascade);
            });

            // Sessions
            modelBuilder.Entity<Session>(b =>
            {
                b.ToTable("Sessions");
                b.HasKey(s => s.Id);
                b.Property(s => s.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
                b.Property(s => s.JwtId).HasMaxLength(64);
                b.Property(s => s.UserAgent).HasMaxLength(200);
                b.Property(s => s.IP).HasMaxLength(45);
                b.Property(s => s.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
                b.Property(s => s.ExpiresAt).HasColumnType("datetime2");
                b.Property(s => s.RevokedAt).HasColumnType("datetime2");
                b.HasOne(s => s.User).WithMany(u => u.Sessions).HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            // EndpointPermissions
            modelBuilder.Entity<EndpointPermission>(b =>
            {
                b.ToTable("EndpointPermissions");
                b.HasKey(e => e.Id);
                b.Property(e => e.Route).HasMaxLength(200).IsRequired();
                b.Property(e => e.HttpMethod).HasMaxLength(10).IsRequired();
                b.Property(e => e.AllowedRolesCsv).HasMaxLength(500).IsRequired(); // "*" para cualquiera
                b.Property(e => e.IsEnabled).HasDefaultValue(true);
                b.HasIndex(e => new { e.Route, e.HttpMethod }).IsUnique();
            });
        }
    }
}
