using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

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
    public record LoginResponse(string access_token, string token_type, int expires_in, string username, List<string> roles);

    public async Task<bool> LoginAsync(string username, string password)
    {
        var client = _httpClientFactory.CreateClient("InventariosApi");

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(username, password));
        if (!response.IsSuccessStatusCode)
            return false;

        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (payload is null || string.IsNullOrWhiteSpace(payload.access_token))
            return false;

        await _localStorage.SetAsync("auth.token", payload.access_token);
        await _localStorage.SetAsync("auth.tokenType", payload.token_type);
        await _localStorage.SetAsync("auth.expiresIn", payload.expires_in);
        await _localStorage.SetAsync("auth.username", payload.username);
        await _localStorage.SetAsync("auth.roles", payload.roles);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(payload.token_type, payload.access_token);
        _state.SetAuthenticated(true, payload.username, payload.roles);
        return true;
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
