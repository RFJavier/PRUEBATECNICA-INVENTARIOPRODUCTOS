using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using WEBAPP.Models;

namespace WEBAPP.Services;

public class AuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ProtectedLocalStorage _localStorage;
    private readonly AuthState _state;

    public AuthService(IHttpClientFactory httpClientFactory, ProtectedLocalStorage localStorage, AuthState state)
    {
        _httpClientFactory = httpClientFactory;
        _localStorage = localStorage;
        _state = state;
    }

    public record LoginRequest(string username, string password);
    public record LoginData(string access_token, string token_type, int expires_in, string username, List<string> roles);

    public async Task<(bool Success, string Message)> LoginAsync(string username, string password)
    {
        var client = _httpClientFactory.CreateClient("InventariosApi");

        var httpResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(username, password));
        var response = await httpResponse.Content.ReadFromJsonAsync<ApiResponse<LoginData>>();
        if (response is null || !response.Success || response.Data is null || string.IsNullOrWhiteSpace(response.Data.access_token))
        {
            return (false, response?.Message ?? "Credenciales inválidas");
        }

        var payload = response.Data;

        await _localStorage.SetAsync("auth.token", payload.access_token);
        await _localStorage.SetAsync("auth.tokenType", payload.token_type);
        await _localStorage.SetAsync("auth.expiresIn", payload.expires_in);
        await _localStorage.SetAsync("auth.username", payload.username);
        await _localStorage.SetAsync("auth.roles", payload.roles);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(payload.token_type, payload.access_token);
        _state.SetAuthenticated(true, payload.username, payload.roles);
        return (true, response.Message);
    }

    public async Task<string?> GetTokenAsync()
    {
        var result = await _localStorage.GetAsync<string>("auth.token");
        var token = result.Success ? result.Value : null;

        if (!string.IsNullOrWhiteSpace(token))
        {
            var usernameResult = await _localStorage.GetAsync<string>("auth.username");
            var rolesResult = await _localStorage.GetAsync<List<string>>("auth.roles");
            _state.SetAuthenticated(true, usernameResult.Value, rolesResult.Value);
        }
        else
        {
            _state.SetAuthenticated(false);
        }

        return token;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.DeleteAsync("auth.token");
        await _localStorage.DeleteAsync("auth.tokenType");
        await _localStorage.DeleteAsync("auth.expiresIn");
        await _localStorage.DeleteAsync("auth.username");
        await _localStorage.DeleteAsync("auth.roles");
        _state.SetAuthenticated(false);
    }
}
