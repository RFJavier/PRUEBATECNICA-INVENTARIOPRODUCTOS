namespace WEBAPP.Models;

public class EndpointPermission
{
    public int Id { get; set; }
    public string Route { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string AllowedRolesCsv { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}

public class EndpointPermissionRequest
{
    public string Route { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string AllowedRolesCsv { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}
