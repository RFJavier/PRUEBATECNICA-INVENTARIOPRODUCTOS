namespace restapi.inventarios.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        public List<UserRole> UserRoles { get; set; } = new();
        public List<Session> Sessions { get; set; } = new();
    }

    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<UserRole> UserRoles { get; set; } = new();
    }

    public class UserRole
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public User? User { get; set; }
        public Role? Role { get; set; }
    }

    public class Session
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public string? JwtId { get; set; }
        public string? UserAgent { get; set; }
        public string? IP { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public User? User { get; set; }
    }
}
