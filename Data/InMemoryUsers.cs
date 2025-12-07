using restapi.inventarios.Models;
using restapi.inventarios.Security;

namespace restapi.inventarios.Data
{
    // Store simple en memoria para demo. Reemplaza por EF Core/SQL.
    public static class InMemoryUsers
    {
        private static readonly Dictionary<string, User> _users = new(StringComparer.OrdinalIgnoreCase);

        static InMemoryUsers()
        {
            // Usuario demo: demo / P@ssw0rd!
            var salt = PasswordHasher.GenerateSalt();
            var hash = PasswordHasher.HashPassword("P@ssw0rd!", salt);

            _users["demo"] = new User
            {
                Username = "demo",
                PasswordSaltBase64 = Convert.ToBase64String(salt),
                PasswordHashBase64 = Convert.ToBase64String(hash),
                Roles = new[] { "User" }
            };
        }

        public static User? Find(string username) => _users.TryGetValue(username, out var u) ? u : null;

        public static void Add(User user) => _users[user.Username] = user;
    }
}
