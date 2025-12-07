using System.Net.Http.Headers;
using System.Net.Http.Json;
using WEBAPP.Models;

namespace WEBAPP.Services;

public class ProductoService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthService _authService;

    public ProductoService(IHttpClientFactory httpClientFactory, AuthService authService)
    {
        _httpClientFactory = httpClientFactory;
        _authService = authService;
    }

    private async Task<HttpClient> GetAuthenticatedClientAsync()
    {
        var client = _httpClientFactory.CreateClient("InventariosApi");
        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }

    public async Task<(List<Producto> Data, string Message)> GetAllAsync()
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.GetFromJsonAsync<ApiResponse<List<Producto>>>("/api/productos");
        return (response?.Data ?? new(), response?.Message ?? string.Empty);
    }

    public async Task<(Producto? Data, string Message)> GetByIdAsync(int id)
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.GetFromJsonAsync<ApiResponse<Producto>>($"/api/productos/{id}");
        return (response?.Data, response?.Message ?? string.Empty);
    }

    public async Task<(List<Producto> Data, string Message)> BuscarPorCodigoAsync(string codigo)
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.GetFromJsonAsync<ApiResponse<List<Producto>>>($"/api/productos/buscar?codigo={Uri.EscapeDataString(codigo)}");
        return (response?.Data ?? new(), response?.Message ?? string.Empty);
    }

    public async Task<(Producto? Data, string Message)> BuscarExactoPorCodigoAsync(string codigo)
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.GetFromJsonAsync<ApiResponse<Producto>>($"/api/productos/buscar-exacto?codigo={Uri.EscapeDataString(codigo)}");
        return (response?.Data, response?.Message ?? string.Empty);
    }

    public async Task<(bool Success, string Message, Producto? Data)> CreateAsync(ProductoRequest producto)
    {
        var client = await GetAuthenticatedClientAsync();
        var httpResponse = await client.PostAsJsonAsync("/api/productos", producto);
        var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<Producto>>();
        return (response?.Success ?? false, response?.Message ?? "Error desconocido", response?.Data);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, ProductoRequest producto)
    {
        var client = await GetAuthenticatedClientAsync();
        var httpResponse = await client.PutAsJsonAsync($"/api/productos/{id}", producto);
        var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse>();
        return (response?.Success ?? false, response?.Message ?? "Error desconocido");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var client = await GetAuthenticatedClientAsync();
        var httpResponse = await client.DeleteAsync($"/api/productos/{id}");
        var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse>();
        return (response?.Success ?? false, response?.Message ?? "Error desconocido");
    }
}
