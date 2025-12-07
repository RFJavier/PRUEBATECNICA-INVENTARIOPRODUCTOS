namespace WEBAPP.Services;

// Holds auth status without touching browser storage during prerender
public class AuthState
{
    public bool IsAuthenticated { get; private set; }
    public string? Username { get; private set; }
    public List<string> Roles { get; private set; } = new();

    public void SetAuthenticated(bool value, string? username = null, List<string>? roles = null)
    {
        IsAuthenticated = value;
        Username = value ? username : null;
        Roles = value && roles is not null ? roles : new();
    }

    public bool IsInRole(params string[] roles) =>
        IsAuthenticated && Roles.Any(r => roles.Contains(r, StringComparer.OrdinalIgnoreCase));
}
