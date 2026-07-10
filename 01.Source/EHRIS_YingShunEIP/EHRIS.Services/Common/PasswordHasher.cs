using System.Security.Cryptography;

namespace EHRIS.Services.Common;

public class PasswordHasher
{
    public static (string Hash, string Salt) HashPassword(string password)
    {
        byte[] saltBytes = RandomNumberGenerator.GetBytes(16);

        var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100_000, HashAlgorithmName.SHA256);
        byte[] hashBytes = pbkdf2.GetBytes(32);

        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes)); 
         
    }

    public static bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        byte[] saltBytes = Convert.FromBase64String(storedSalt);
        var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100_000, HashAlgorithmName.SHA256);
        byte[] hashBytes = pbkdf2.GetBytes(32);

        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(storedHash),
            hashBytes
        );
    }
}
