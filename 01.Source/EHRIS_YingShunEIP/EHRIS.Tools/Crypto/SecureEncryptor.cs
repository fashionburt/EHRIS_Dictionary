extern alias CryptoLegacy;
extern alias CryptoNew;

using LegacySecureRandom = CryptoLegacy::Org.BouncyCastle.Security.SecureRandom;
using NewSecureRandom = CryptoNew::Org.BouncyCastle.Security.SecureRandom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CryptoLegacy::Org.BouncyCastle.Security;
using CryptoLegacy::Org.BouncyCastle.Crypto.Modes;
using CryptoLegacy::Org.BouncyCastle.Crypto.Parameters;
using CryptoLegacy::Org.BouncyCastle.Crypto.Engines;

namespace EHRIS.Tools.Crypto
{
    /// <summary>
    /// 驗證密碼（可逆）
    /// 加密 : string encrypted = SecureEncryptor.Encrypt("機密資料", "StrongPassword!");
    /// 解密 : string decrypted = SecureEncryptor.Decrypt(encrypted, "StrongPassword!"); 
    /// 加密+AAD : string encrypted = SecureEncryptor.Encrypt("機密資料", "StrongPassword!", "驗證文字");
    /// 解密+AAD : string decrypted = SecureEncryptor.Decrypt(encrypted, "StrongPassword!", "驗證文字"); 
    /// </summary>
    public static class SecureEncryptor
    {
        private const int SaltSize = 16;
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const int Iterations = 100_000;
        private const int KeySize = 32; // AES-256
        private const int HmacSize = 32;

        #region AES-GCM 模式 + HMAC 雙重驗證
        /// <summary>
        /// 加密流程總結
        /// Salt : 用於 PBKDF2 派生金鑰
        /// Nonce (IV) : AES-GCM 的初始化向量
        /// CipherText : AES-GCM 加密後的密文 + tag
        /// HMAC : 對整個 payload 做完整性驗證
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string Encrypt(string plainText, string password)
        {
            byte[] salt = new byte[SaltSize];
            new SecureRandom().NextBytes(salt);

            var keyGen = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] key = keyGen.GetBytes(KeySize);

            byte[] nonce = new byte[NonceSize];
            new SecureRandom().NextBytes(nonce);

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), TagSize * 8, nonce, null);
            cipher.Init(true, parameters);

            byte[] cipherBytes = new byte[cipher.GetOutputSize(plainBytes.Length)];
            int len = cipher.ProcessBytes(plainBytes, 0, plainBytes.Length, cipherBytes, 0);
            cipher.DoFinal(cipherBytes, len);

            // 組合 payload: salt + nonce + cipherText
            byte[] payload;
            using (var ms = new MemoryStream())
            {
                ms.Write(salt, 0, salt.Length);
                ms.Write(nonce, 0, nonce.Length);
                ms.Write(cipherBytes, 0, cipherBytes.Length);
                payload = ms.ToArray();
            }

            // 加入 HMAC-SHA256 驗證
            byte[] hmac;
            using (var hmacSha = new HMACSHA256(key))
            {
                hmac = hmacSha.ComputeHash(payload);
            }

            // 最終輸出: payload + hmac
            byte[] final = payload.Concat(hmac).ToArray();
            return Convert.ToBase64String(final);

        }

        // 解密：Base64 → salt + Nonce (IV) + Key + CipherText → AES 解密
        public static string Decrypt(string base64CipherText, string password)
        {
            byte[] fullData = Convert.FromBase64String(base64CipherText);

            int payloadLength = SaltSize + NonceSize + (fullData.Length - SaltSize - NonceSize - 32); // 32 bytes HMAC
            byte[] payload = fullData.Take(fullData.Length - 32).ToArray();
            byte[] hmac = fullData.Skip(fullData.Length - 32).ToArray();

            byte[] salt = payload.Take(SaltSize).ToArray();
            byte[] nonce = payload.Skip(SaltSize).Take(NonceSize).ToArray();
            byte[] cipherBytes = payload.Skip(SaltSize + NonceSize).ToArray();

            var keyGen = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] key = keyGen.GetBytes(KeySize);

            // 驗證 HMAC
            using (var hmacSha = new HMACSHA256(key))
            {
                byte[] expectedHmac = hmacSha.ComputeHash(payload);
                if (!expectedHmac.SequenceEqual(hmac))
                    throw new CryptographicException("HMAC 驗證失敗，資料可能已被竄改。");
            }

            // 解密
            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), TagSize * 8, nonce, null);
            cipher.Init(false, parameters);

            byte[] plainBytes = new byte[cipher.GetOutputSize(cipherBytes.Length)];
            int len = cipher.ProcessBytes(cipherBytes, 0, cipherBytes.Length, plainBytes, 0);
            cipher.DoFinal(plainBytes, len);

            return Encoding.UTF8.GetString(plainBytes);

        }
        #endregion

        #region AES-GCM 模式 + HMAC 雙重驗證 + AAD(驗證)
        /// <summary>
        /// string email = "user@example.com";
        /// string password = "EmailPassword123!";
        /// string masterKey = "SystemLevelMasterKey";

        /// string encrypted = Encryptor.Encrypt(password, masterKey, email);
        /// string decrypted = Encryptor.Decrypt(encrypted, masterKey, email);

        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="password"></param>
        /// <param name="aadText"></param>
        /// <returns></returns>
        //AES-GCM 模式 + HMAC 雙重驗證 + AAD(驗證)
        public static string Encrypt(string plainText, string password, string aadText)
        {
            byte[] salt = new byte[SaltSize];
            new SecureRandom().NextBytes(salt);

            var keyGen = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] key = keyGen.GetBytes(KeySize);

            byte[] nonce = new byte[NonceSize];
            new SecureRandom().NextBytes(nonce);

            byte[] aad = Encoding.UTF8.GetBytes(aadText);
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), TagSize * 8, nonce, aad);
            cipher.Init(true, parameters);

            byte[] cipherBytes = new byte[cipher.GetOutputSize(plainBytes.Length)];
            int len = cipher.ProcessBytes(plainBytes, 0, plainBytes.Length, cipherBytes, 0);
            cipher.DoFinal(cipherBytes, len);

            byte[] payload;
            using (var ms = new MemoryStream())
            {
                ms.Write(salt, 0, salt.Length);
                ms.Write(nonce, 0, nonce.Length);
                ms.Write(cipherBytes, 0, cipherBytes.Length);
                payload = ms.ToArray();
            }

            byte[] hmac;
            using (var hmacSha = new HMACSHA256(key))
            {
                hmac = hmacSha.ComputeHash(payload);
            }

            byte[] final = payload.Concat(hmac).ToArray();
            return Convert.ToBase64String(final);
        }

        //解密：AES-GCM 模式 + HMAC 雙重驗證 + AAD(驗證)
        public static string Decrypt(string base64CipherText, string password, string aadText)
        {
            byte[] fullData = Convert.FromBase64String(base64CipherText);

            if (fullData.Length < SaltSize + NonceSize + TagSize + HmacSize)
                throw new CryptographicException("密文格式錯誤或資料不完整");

            byte[] payload = fullData.Take(fullData.Length - HmacSize).ToArray();
            byte[] hmac = fullData.Skip(fullData.Length - HmacSize).ToArray();

            byte[] salt = payload.Take(SaltSize).ToArray();
            byte[] nonce = payload.Skip(SaltSize).Take(NonceSize).ToArray();
            byte[] cipherBytes = payload.Skip(SaltSize + NonceSize).ToArray();
            byte[] aad = Encoding.UTF8.GetBytes(aadText);

            var keyGen = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] key = keyGen.GetBytes(KeySize);

            using (var hmacSha = new HMACSHA256(key))
            {
                byte[] expectedHmac = hmacSha.ComputeHash(payload);
                if (!expectedHmac.SequenceEqual(hmac))
                    throw new CryptographicException("HMAC 驗證失敗，資料可能已被竄改");
            }

            var cipher = new GcmBlockCipher(new AesEngine());
            var parameters = new AeadParameters(new KeyParameter(key), TagSize * 8, nonce, aad);
            cipher.Init(false, parameters);

            byte[] plainBytes = new byte[cipher.GetOutputSize(cipherBytes.Length)];
            int len = cipher.ProcessBytes(cipherBytes, 0, cipherBytes.Length, plainBytes, 0);
            cipher.DoFinal(plainBytes, len);

            return Encoding.UTF8.GetString(plainBytes);
        }
        #endregion


    }
}