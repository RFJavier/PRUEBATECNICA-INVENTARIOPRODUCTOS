using System.Net.Http.Headers;
using System.Net.Http.Json;
using WEBAPP.Models;

namespace WEBAPP.Services;

public class RoleService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthService _authService;

    public RoleService(IHttpClientFactory httpClientFactory, AuthService authService)
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

    public async Task<(List<Role> Data, string Message)> GetAllAsync()
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.GetFromJsonAsync<ApiResponse<List<Role>>>("/api/roles");
        return (response?.Data ?? new(), response?.Message ?? string.Empty);
    }

    public async Task<(bool Success, string Message, Role? Data)> CreateAsync(RoleRequest role)
    {
        var client = await GetAuthenticatedClientAsync();
        var httpResponse = await client.PostAsJsonAsync("/api/roles", role);
        var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<Role>>();
        return (response?.Success ?? false, response?.Message ?? "Error desconocido", response?.Data);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var client = await GetAuthenticatedClientAsync();
        var httpResponse = await client.DeleteAsync($"/api/roles/{id}");
        var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse>();
        return (response?.Success ?? false, response?.Message ?? "Error desconocido");
    }
}
