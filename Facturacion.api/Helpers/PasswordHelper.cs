using System;
using System.Security.Cryptography;

namespace Facturacion.api.Helpers
{
    public static class PasswordHelper
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100_000;

        public static string Hash(string password)
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(salt);

            byte[] hash = Pbkdf2(password, salt, Iterations, HashSize);
            return $"pbkdf2:{Iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrEmpty(stored))
                return false;

            // Migración: si no es hash PBKDF2, comparar como texto plano
            if (!stored.StartsWith("pbkdf2:"))
                return string.Equals(password, stored, StringComparison.Ordinal);

            var parts = stored.Split(':');
            if (parts.Length != 4)
                return false;

            if (!int.TryParse(parts[1], out int iterations))
                return false;

            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expectedHash = Convert.FromBase64String(parts[3]);
            byte[] actualHash = Pbkdf2(password, salt, iterations, expectedHash.Length);
            return CryptographicEquals(expectedHash, actualHash);
        }

        private static byte[] Pbkdf2(string password, byte[] salt, int iterations, int length)
        {
            using (var d = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                return d.GetBytes(length);
        }

        private static bool CryptographicEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
