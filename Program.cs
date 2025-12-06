using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using restapi.inventarios.Data;

namespace restapi.inventarios
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configuración JWT (idealmente mover a appsettings.json)
            var jwtKey = builder.Configuration["Jwt:Key"] ?? "ClaveSuperSecreta_development_cambia_esto";
            var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "restapi.inventarios";
            var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "restapi.inventarios.clients";
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            // EF Core DbContext (configurar cadena de conexión)
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                   ?? "Server=(localdb)\\MSSQLLocalDB;Database=InventariosDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Autenticación JWT
            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = signingKey,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Aplicar migraciones automáticamente en el arranque
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            // Orden correcto
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
