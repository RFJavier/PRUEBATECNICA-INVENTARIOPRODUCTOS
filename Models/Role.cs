namespace WEBAPP.Models;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class RoleRequest
{
    public string Name { get; set; } = string.Empty;
}
