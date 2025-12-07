using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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

    public async Task<(List<Venta> Data, string Message)> GetAllAsync()
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.GetFromJsonAsync<ApiResponse<List<Venta>>>("/api/ventas");
        return (response?.Data ?? new(), response?.Message ?? string.Empty);
    }

    public async Task<(List<VentaConDetalles> Data, string Message)> GetAllConDetallesAsync()
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.GetFromJsonAsync<ApiResponse<List<VentaConDetalles>>>("/api/ventas/con-detalles");
        return (response?.Data ?? new(), response?.Message ?? string.Empty);
    }

    public async Task<(VentaDetalle? Data, string Message)> GetByIdAsync(int id)
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.GetFromJsonAsync<ApiResponse<VentaDetalle>>($"/api/ventas/{id}");
        return (response?.Data, response?.Message ?? string.Empty);
    }

    public async Task<(bool Success, string Message, int? Id)> CreateAsync(VentaRequest venta)
    {
        var client = await GetAuthenticatedClientAsync();
        var httpResponse = await client.PostAsJsonAsync("/api/ventas", venta);
        
        var content = await httpResponse.Content.ReadAsStringAsync();
        
        // Handle empty response
        if (string.IsNullOrWhiteSpace(content))
        {
            return (httpResponse.IsSuccessStatusCode, 
                    httpResponse.IsSuccessStatusCode ? "Venta creada exitosamente" : "Error al crear la venta", 
                    null);
        }

        try
        {
            var response = JsonSerializer.Deserialize<ApiResponse<VentaCreateResponse>>(content, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            return (response?.Success ?? httpResponse.IsSuccessStatusCode, 
                    response?.Message ?? (httpResponse.IsSuccessStatusCode ? "Venta creada exitosamente" : "Error al crear la venta"), 
                    response?.Data?.Id);
        }
        catch (JsonException)
        {
            // If JSON parsing fails, use HTTP status
            return (httpResponse.IsSuccessStatusCode, 
                    httpResponse.IsSuccessStatusCode ? "Venta creada exitosamente" : $"Error: {content}", 
                    null);
        }
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, VentaRequest venta)
    {
        var client = await GetAuthenticatedClientAsync();
        var httpResponse = await client.PutAsJsonAsync($"/api/ventas/{id}", venta);
        
        var content = await httpResponse.Content.ReadAsStringAsync();
        
        if (string.IsNullOrWhiteSpace(content))
        {
            return (httpResponse.IsSuccessStatusCode, 
                    httpResponse.IsSuccessStatusCode ? "Venta actualizada exitosamente" : "Error al actualizar la venta");
        }

        try
        {
            var response = JsonSerializer.Deserialize<ApiResponse>(content, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            return (response?.Success ?? httpResponse.IsSuccessStatusCode, 
                    response?.Message ?? (httpResponse.IsSuccessStatusCode ? "Venta actualizada exitosamente" : "Error al actualizar la venta"));
        }
        catch (JsonException)
        {
            return (httpResponse.IsSuccessStatusCode, 
                    httpResponse.IsSuccessStatusCode ? "Venta actualizada exitosamente" : $"Error: {content}");
        }
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var client = await GetAuthenticatedClientAsync();
        var httpResponse = await client.DeleteAsync($"/api/ventas/{id}");
        
        var content = await httpResponse.Content.ReadAsStringAsync();
        
        if (string.IsNullOrWhiteSpace(content))
        {
            return (httpResponse.IsSuccessStatusCode, 
                    httpResponse.IsSuccessStatusCode ? "Venta eliminada exitosamente" : "Error al eliminar la venta");
        }

        try
        {
            var response = JsonSerializer.Deserialize<ApiResponse>(content, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            return (response?.Success ?? httpResponse.IsSuccessStatusCode, 
                    response?.Message ?? (httpResponse.IsSuccessStatusCode ? "Venta eliminada exitosamente" : "Error al eliminar la venta"));
        }
        catch (JsonException)
        {
            return (httpResponse.IsSuccessStatusCode, 
                    httpResponse.IsSuccessStatusCode ? "Venta eliminada exitosamente" : $"Error: {content}");
        }
    }
}
