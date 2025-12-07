using WEBAPP.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace WEBAPP
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Configure HttpClient with API base address from configuration
            var apiBaseUrl = builder.Configuration["RestApi:BaseUrl"] ?? "http://94.72.112.74:7777";
            builder.Services.AddHttpClient("InventariosApi", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            });

            // Register ProtectedBrowserStorage services (no AddProtectedBrowserStorage extension in .NET 8; use direct types)
            builder.Services.AddScoped<ProtectedLocalStorage>();
            builder.Services.AddScoped<ProtectedSessionStorage>();

            // Auth state + service
            builder.Services.AddScoped<Services.AuthState>();
            builder.Services.AddScoped<Services.AuthService>();

            // Product service
            builder.Services.AddScoped<Services.ProductoService>();

            // Venta service
            builder.Services.AddScoped<Services.VentaService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
