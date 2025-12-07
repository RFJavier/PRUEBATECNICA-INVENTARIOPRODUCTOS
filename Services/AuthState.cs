namespace WEBAPP.Services;

// Holds auth status without touching browser storage during prerender
public class AuthState
{
    public bool IsAuthenticated { get; private set; }

    public void SetAuthenticated(bool value) => IsAuthenticated = value;
}
