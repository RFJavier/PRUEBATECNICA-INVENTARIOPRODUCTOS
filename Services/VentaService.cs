using System.Net.Http.Headers;
using System.Net.Http.Json;
using WEBAPP.Models;

namespace WEBAPP.Services;

public class VentaService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthService _authService;

    public VentaService(IHttpClientFactory httpClientFactory, AuthService authService)
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

    public async Task<List<Venta>> GetAllAsync()
    {
        var client = await GetAuthenticatedClientAsync();
        var result = await client.GetFromJsonAsync<List<Venta>>("/api/ventas");
        return result ?? new();
    }

    public async Task<Venta?> GetByIdAsync(int id)
    {
        var client = await GetAuthenticatedClientAsync();
        return await client.GetFromJsonAsync<Venta>($"/api/ventas/{id}");
    }

    public async Task<bool> CreateAsync(VentaRequest venta)
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.PostAsJsonAsync("/api/ventas", venta);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int id, VentaRequest venta)
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.PutAsJsonAsync($"/api/ventas/{id}", venta);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.DeleteAsync($"/api/ventas/{id}");
        return response.IsSuccessStatusCode;
    }
}
