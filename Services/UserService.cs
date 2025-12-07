using System.Net.Http.Headers;
using System.Net.Http.Json;
using WEBAPP.Models;

namespace WEBAPP.Services;

public class UserService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthService _authService;

    public UserService(IHttpClientFactory httpClientFactory, AuthService authService)
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

    public async Task<(List<User> Data, string Message)> GetAllAsync()
    {
        var client = await GetAuthenticatedClientAsync();
        var response = await client.GetFromJsonAsync<ApiResponse<List<User>>>("/api/users");
        return (response?.Data ?? new(), response?.Message ?? string.Empty);
    }

    public async Task<(bool Success, string Message)> AssignRoleAsync(int userId, string roleName)
    {
        var client = await GetAuthenticatedClientAsync();
        var httpResponse = await client.PostAsJsonAsync($"/api/users/{userId}/roles", roleName);
        var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse>();
        return (response?.Success ?? false, response?.Message ?? "Error desconocido");
    }

    public async Task<(bool Success, string Message)> RemoveRoleAsync(int userId, string roleName)
    {
        var client = await GetAuthenticatedClientAsync();
        var httpResponse = await client.DeleteAsync($"/api/users/{userId}/roles/{roleName}");
        var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse>();
        return (response?.Success ?? false, response?.Message ?? "Error desconocido");
    }
}
