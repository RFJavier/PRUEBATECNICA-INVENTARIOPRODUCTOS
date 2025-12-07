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

    public async Task<List<Producto>> GetAllAsync()
    {
        var client = await GetAuthenticatedClientAsync();
        var result = await client.GetFromJsonAsync<List<Producto>>("/api/productos");
        return result ?? new();
    }

    public async Task<Producto?> GetByIdAsync(int id)
    {
        var client = await GetAuthenticatedClientAsync();
        return await client.GetFromJsonAsync<Producto>($"/api/productos/{id}");
    }

    public async Task<bool> CreateAsync(ProductoRequest producto)
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.PostAsJsonAsync("/api/productos", producto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int id, ProductoRequest producto)
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.PutAsJsonAsync($"/api/productos/{id}", producto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.DeleteAsync($"/api/productos/{id}");
        return response.IsSuccessStatusCode;
    }
}
