using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using restapi.inventarios.Data;
using restapi.inventarios.Data.Repositories;
using restapi.inventarios.Services;
using Microsoft.OpenApi.Models;

namespace restapi.inventarios
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configuración JWT
            var jwtKey = builder.Configuration["Jwt:Key"] ?? "ClaveSuperSecreta_development_cambia_esto";
            var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "restapi.inventarios";
            var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "restapi.inventarios.clients";
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            // EF Core DbContext
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                   ?? "Server=(localdb)\\MSSQLLocalDB;Database=InventariosDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Repositorios
            builder.Services.AddScoped<ProductoRepository>();
            builder.Services.AddScoped<VentasRepository>();

            // Servicios
            builder.Services.AddScoped<ReportesService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Inventarios API", Version = "v1" });
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Ingrese el token JWT: Bearer {token}",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };
                c.AddSecurityDefinition("Bearer", securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { securityScheme, Array.Empty<string>() }
                });
            });

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

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            // Middleware de permisos por endpoint
            app.Use(async (context, next) =>
            {
                // Permitir endpoints anónimos (como login/register) sin validar permisos
                var endpoint = context.GetEndpoint();
                var allowAnonymous = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() != null;
                if (allowAnonymous)
                {
                    await next();
                    return;
                }

                var route = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
                var method = context.Request.Method.ToUpperInvariant();
                
                // Usar el DbContext del scope de la request (no crear uno nuevo)
                var db = context.RequestServices.GetRequiredService<AppDbContext>();

                // Buscar permiso: primero coincidencia exacta, luego por prefijo (ruta base)
                var perm = await db.EndpointPermissions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Route.ToLower() == route && e.HttpMethod == method);

                // Si no hay coincidencia exacta, buscar por ruta base (ej: /api/ventas para /api/ventas/123)
                if (perm == null)
                {
                    var segments = route.TrimEnd('/').Split('/');
                    if (segments.Length > 1 && int.TryParse(segments[^1], out _))
                    {
                        var baseRoute = string.Join("/", segments[..^1]);
                        perm = await db.EndpointPermissions
                            .AsNoTracking()
                            .FirstOrDefaultAsync(e => e.Route.ToLower() == baseRoute && e.HttpMethod == method);
                    }
                }

                if (perm == null || !perm.IsEnabled)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Endpoint deshabilitado o no configurado");
                    return;
                }
                if (perm.AllowedRolesCsv.Trim() == "*")
                {
                    await next();
                    return;
                }
                var rolesAllowed = perm.AllowedRolesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                                        .Select(r => r.ToLowerInvariant()).ToHashSet();
                var userRoles = context.User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                                                   .Select(c => c.Value.ToLowerInvariant());
                if (!userRoles.Any(r => rolesAllowed.Contains(r)))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Acceso denegado por roles");
                    return;
                }
                await next();
            });

            app.MapControllers();

            app.Run();
        }
    }
}
