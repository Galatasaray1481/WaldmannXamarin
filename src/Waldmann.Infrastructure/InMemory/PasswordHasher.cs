using System.Security.Cryptography;

namespace Waldmann.Infrastructure.InMemory;

/// <summary>
/// PBKDF2-SHA256 password hashes. Even the in-memory store keeps no plain-text passwords, so that a
/// later server-backed store does not inherit the habit.
/// </summary>
internal static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public static string Hash(string password)
    {
        var salt = new byte[SaltSize];
        using (var random = RandomNumberGenerator.Create())
        {
            random.GetBytes(salt);
        }

        return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(Derive(password, salt));
    }

    public static bool Verify(string password, string stored)
    {
        var parts = stored.Split('.');
        if (parts.Length != 2)
        {
            return false;
        }

        var expected = Convert.FromBase64String(parts[1]);
        return FixedTimeEquals(Derive(password, Convert.FromBase64String(parts[0])), expected);
    }

    // PBKDF2-HMAC-SHA256 (RFC 8018, section 5.2) for a single 32-byte block. netstandard2.0's
    // Rfc2898DeriveBytes only offers SHA-1. Verified against the RFC 7914 test vector in the tests.
    internal static byte[] Derive(string password, byte[] salt, int iterations = Iterations)
    {
        using var hmac = new HMACSHA256(System.Text.Encoding.UTF8.GetBytes(password));
        var block = new byte[salt.Length + 4];
        Buffer.BlockCopy(salt, 0, block, 0, salt.Length);
        block[block.Length - 1] = 1; // block index INT(1), big-endian

        var u = hmac.ComputeHash(block);
        var result = (byte[])u.Clone();
        for (var iteration = 1; iteration < iterations; iteration++)
        {
            u = hmac.ComputeHash(u);
            for (var i = 0; i < HashSize; i++)
            {
                result[i] ^= u[i];
            }
        }

        return result;
    }

    // CryptographicOperations.FixedTimeEquals is not available on netstandard2.0.
    private static bool FixedTimeEquals(byte[] left, byte[] right)
    {
        if (left.Length != right.Length)
        {
            return false;
        }

        var difference = 0;
        for (var i = 0; i < left.Length; i++)
        {
            difference |= left[i] ^ right[i];
        }

        return difference == 0;
    }
}
