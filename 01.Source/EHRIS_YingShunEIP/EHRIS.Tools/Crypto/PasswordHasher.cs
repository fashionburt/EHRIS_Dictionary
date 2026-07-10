using Isopoh.Cryptography.Argon2;
using Isopoh.Cryptography.SecureArray;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static Azure.Core.HttpHeader;

namespace EHRIS.Tools.Crypto
{
    /// <summary>
    /// 安全儲存密碼（不可逆 Argon2id）
    /// </summary>
    public class PasswordHasher
    {
        private static byte[] ComputeHmac(byte[] data, byte[] key)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(data);
        }

        /// <summary>
        /// Argon2id 雜湊模組 
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        private static byte[] DeriveArgon2idHashAddSalt(string paintText, byte[] salt)
        {
            //var argon2 = new Argon2(Encoding.UTF8.GetBytes(password))
            //{
            //    Salt = salt,
            //    DegreeOfParallelism = 8,
            //    MemorySize = 65536,
            //    Iterations = 4
            //};
            //return argon2.GetBytes(32);

            if (salt == null || salt.Length < 8)
                throw new ArgumentException("Salt 必須至少 8 bytes");

            var config = new Argon2Config
            {
                Type = Argon2Type.DataIndependentAddressing,
                Version = Argon2Version.Nineteen,
                Password = Encoding.UTF8.GetBytes(paintText),
                Salt = salt,
                TimeCost = 4,
                MemoryCost = 65536,
                Lanes = 8,
                Threads = Environment.ProcessorCount,
                HashLength = 32
            };

            using var argon2 = new Argon2(config);
            using var hash = argon2.Hash();
            return hash.Buffer.ToArray();
 

        }

        /// <summary>
        /// Argon2id 雜湊模組 - LEVEL 1
        /// 存資料庫用
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string HashPasswordAddSalt(string paintText, byte[] salt)
        {
            var hash = DeriveArgon2idHashAddSalt(paintText, salt);

            return Convert.ToBase64String(hash);

        }

        public static bool VerifyPassword(string password, byte[] salt, string expectedHash)
        {
            var hash = HashPasswordAddSalt(password, salt);
            return CryptographicOperations.FixedTimeEquals(
                            Convert.FromBase64String(hash),
                            Convert.FromBase64String(expectedHash)
                        );
        }



        /// <summary>
        /// Argon2id 雜湊模組 +HMAC - LEVEL 2
        /// 存資料庫用
        /// </summary>
        /// <param name="paintText"></param>
        /// <param name="salt"></param>
        /// <param name="hmacKey"></param>
        /// <returns></returns>
        public static string HashPasswordAddSalt(string paintText, byte[] salt, byte[] hmacKey)
        {
            var hash = DeriveArgon2idHashAddSalt(paintText, salt);
            var tag = ComputeHmac(hash, hmacKey);
            var combined = hash.Concat(tag).ToArray();
            return Convert.ToBase64String(combined);
        }

        /// <summary>
        /// Argon2id 雜湊模組 +HMAC - LEVEL 2
        /// 存資料庫用
        /// </summary>
        /// <param name="password">原始密碼</param> 
        /// <param name="hmacKey"></param>
        /// <returns></returns>
        public static (string Hash, string Salt) HashPassword(string paintText, byte[] hmacKey)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            var hash = DeriveArgon2idHashAddSalt(paintText, salt);
            var tag = ComputeHmac(hash, hmacKey);
            var combined = hash.Concat(tag).ToArray();

            return (Convert.ToBase64String(combined), Convert.ToBase64String(salt));
        }

        /// <summary>
        /// 驗證密碼Argon2id 雜湊模組 +HMAC - LEVEL 2
        /// </summary>
        /// <param name="password">加密密碼</param>
        /// <param name="salt">Salt</param>
        /// <param name="hmacKey">驗證Key</param>
        /// <param name="expectedHash">加密Hash密碼</param>
        /// <returns></returns>
        public static bool VerifyPassword(string password, byte[] salt, byte[] hmacKey, string expectedHash)
        {
            var hash = DeriveArgon2idHashAddSalt(password, salt);
            var tag = ComputeHmac(hash, hmacKey);
            var combined = hash.Concat(tag).ToArray();
            var recomputed = Convert.ToBase64String(combined);

            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(recomputed),
                Convert.FromBase64String(expectedHash)
            );
        }


        /// <summary>
        /// 驗證密碼Argon2id 雜湊模組 +HMAC - LEVEL 2
        /// </summary>
        /// <param name="password">加密密碼</param>
        /// <param name="storedSalt">Salt</param>
        /// <param name="hmacKeyStr">驗證Key</param>
        /// <param name="expectedHash">加密Hash密碼</param>
        /// <returns></returns>
        public static bool VerifyPassword(string password, string storedSalt, string hmacKeyStr, string expectedHash)
        {
            if (string.IsNullOrWhiteSpace(storedSalt))
            {
                //空字串直接拒絕
                return false;
            }

            // 嘗試轉換 Base64，避免 FormatException
            byte[] saltBytes;
            if (!Convert.TryFromBase64String(storedSalt, new Span<byte>(new byte[storedSalt.Length]), out int bytesWritten))
            {
                //不是合法 Base64，直接拒絕
                return false;
            }
            else
            {
                saltBytes = new byte[bytesWritten];
                Convert.TryFromBase64String(storedSalt, saltBytes, out _);
            }

            //HMAC key 不可為空
            if (string.IsNullOrEmpty(hmacKeyStr))
            {
                return false;
            }

            byte[] hmacKey = Encoding.UTF8.GetBytes(hmacKeyStr);
             
            return VerifyPassword(password, saltBytes, hmacKey, expectedHash);
        }


        /// <summary>
        /// 驗證密碼是否重覆
        /// var recentHashes = GetLastThreePasswordHashesFromDb(userId); // 取出最近三筆
        /// if (IsPasswordReused(newPassword, hmacKeyStr, recentHashes))
        /// {
        ///     throw new InvalidOperationException("密碼不可與最近三次相同");
        /// }
        /// </summary>
        /// <param name="newPaintPassword"></param>
        /// <param name="hmacKeyStr"></param>
        /// <param name="recentHashes"></param>
        /// <returns></returns>
        public static bool IsPasswordReused(string newPaintPassword, string hmacKeyStr, List<(string Hash, string Salt)> recentHashes)
        {
            byte[] hmacKey = Encoding.UTF8.GetBytes(hmacKeyStr);

            foreach (var (storedHash, storedSalt) in recentHashes)
            {
                byte[] saltBytes = Convert.FromBase64String(storedSalt);
                if (PasswordHasher.VerifyPassword(newPaintPassword, saltBytes, hmacKey, storedHash))
                {
                    return true; // 密碼重複
                }
            }

            return false; // 沒有重複
        }
    }
}