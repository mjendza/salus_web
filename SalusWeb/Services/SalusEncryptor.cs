using System.Security.Cryptography;
using System.Text;

namespace SalusWeb.Services;

/// <summary>
/// Encryptor for Salus iT600 local mode communication.
/// Based on the Python pyit600 library implementation.
/// </summary>
public class SalusEncryptor
{
    private static readonly byte[] IV = new byte[] 
    { 
        0x88, 0xa6, 0xb0, 0x79, 0x5d, 0x85, 0xdb, 0xfc, 
        0xe6, 0xe0, 0xb3, 0xe9, 0xa6, 0x29, 0x65, 0x4b 
    };

    private readonly byte[] _key;

    public SalusEncryptor(string euid)
    {
        // Generate key from EUID: MD5("Salus-{euid_lowercase}") + 16 zero bytes
        var keyString = $"Salus-{euid.ToLower()}";
        using var md5 = MD5.Create();
        var md5Hash = md5.ComputeHash(Encoding.UTF8.GetBytes(keyString));
        
        // Key is MD5 hash (16 bytes) + 16 zero bytes = 32 bytes for AES-256
        _key = new byte[32];
        Array.Copy(md5Hash, 0, _key, 0, 16);
        // Remaining 16 bytes are already zero (default value)
    }

    /// <summary>
    /// Encrypts plain text using AES-256-CBC with PKCS7 padding
    /// </summary>
    public byte[] Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = IV;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
    }

    /// <summary>
    /// Decrypts cipher bytes using AES-256-CBC with PKCS7 padding
    /// </summary>
    public string Decrypt(byte[] cipherBytes)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = IV;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
