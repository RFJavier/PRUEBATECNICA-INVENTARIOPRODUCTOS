using System.Text.Json.Serialization;

namespace restapi.inventarios.Models
{
    public class User
    {
        public string Username { get; set; } = string.Empty;

        // Guardamos salt y hash como Base64 para facilidad de almacenamiento
        public string PasswordSaltBase64 { get; set; } = string.Empty;
        public string PasswordHashBase64 { get; set; } = string.Empty;

        public string[] Roles { get; set; } = new[] { "User" };

        [JsonIgnore]
        public byte[] Salt => Convert.FromBase64String(PasswordSaltBase64);

        [JsonIgnore]
        public byte[] Hash => Convert.FromBase64String(PasswordHashBase64);
    }
}
