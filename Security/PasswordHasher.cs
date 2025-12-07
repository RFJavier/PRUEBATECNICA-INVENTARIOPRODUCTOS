using System.Security.Cryptography;

namespace restapi.inventarios.Security
{
    public static class PasswordHasher
    {
        // Genera un salt aleatorio seguro
        public static byte[] GenerateSalt(int size = 16)
        {
            var salt = new byte[size];
            RandomNumberGenerator.Fill(salt);
            return salt;
        }

        // Deriva un hash usando PBKDF2
        public static byte[] HashPassword(string password, byte[] salt, int iterations = 100_000, int keySize = 32)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(keySize);
        }

        // Verifica contraseña comparando hashes en tiempo constante
        public static bool Verify(string password, byte[] salt, byte[] expectedHash, int iterations = 100_000, int keySize = 32)
        {
            var hash = HashPassword(password, salt, iterations, keySize);
            return CryptographicOperations.FixedTimeEquals(hash, expectedHash);
        }
    }
}
