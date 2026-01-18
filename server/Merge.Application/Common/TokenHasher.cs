using System.Security.Cryptography;
using System.Text;

namespace Merge.Application.Common;

public static class TokenHasher
{
    /// <summary>
    /// Token'ı SHA256 ile hash'ler
    /// </summary>
    public static string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Token'ın hash'ini kontrol eder
    /// </summary>
    public static bool VerifyToken(string token, string tokenHash)
    {
        var hash = HashToken(token);
        return hash == tokenHash;
    }
}

