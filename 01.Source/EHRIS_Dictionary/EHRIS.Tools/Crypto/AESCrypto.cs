using System.Security.Cryptography;
using System.Text;

namespace EHRIS.Tools.Crypto;

public static class AESCrypto
{
    public static string Encrypt(string plainText, string key)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;

        using Aes aesAlg = Aes.Create();
        aesAlg.Key = DeriveKey(key);
        aesAlg.IV = new byte[16];

        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msEncrypt = new MemoryStream();
        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    public static string Decrypt(string cipherText, string key)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;

        using Aes aesAlg = Aes.Create();
        aesAlg.Key = DeriveKey(key);
        aesAlg.IV = new byte[16];

        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText));
        using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using StreamReader srDecrypt = new StreamReader(csDecrypt);

        return srDecrypt.ReadToEnd();
    }

    private static byte[] DeriveKey(string key)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(key));
    }
}