using System.Net.Http.Headers;
using System.Net.Http.Json;
using WEBAPP.Models;

namespace WEBAPP.Services;

public class EndpointPermissionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthService _authService;

    public EndpointPermissionService(IHttpClientFactory httpClientFactory, AuthService authService)
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

    public async Task<(List<EndpointPermission> Data, string Message)> GetAllAsync()
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.GetFromJsonAsync<ApiResponse<List<EndpointPermission>>>("/api/endpointpermissions");
        return (response?.Data ?? new(), response?.Message ?? string.Empty);
    }

    public async Task<(bool Success, string Message, EndpointPermission? Data)> CreateAsync(EndpointPermissionRequest permission)
    {
        var client = await GetAuthenticatedClientAsync();
        var httpResponse = await client.PostAsJsonAsync("/api/endpointpermissions", permission);
        var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<EndpointPermission>>();
        return (response?.Success ?? false, response?.Message ?? "Error desconocido", response?.Data);
    }

    public async Task<(bool Success, string Message, EndpointPermission? Data)> UpdateAsync(int id, EndpointPermissionRequest permission)
    {
        var client = await GetAuthenticatedClientAsync();
        var httpResponse = await client.PutAsJsonAsync($"/api/endpointpermissions/{id}", permission);
        var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<EndpointPermission>>();
        return (response?.Success ?? false, response?.Message ?? "Error desconocido", response?.Data);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var client = await GetAuthenticatedClientAsync();
        var httpResponse = await client.DeleteAsync($"/api/endpointpermissions/{id}");
        var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse>();
        return (response?.Success ?? false, response?.Message ?? "Error desconocido");
    }
}
