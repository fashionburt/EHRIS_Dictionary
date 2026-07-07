using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static Azure.Core.HttpHeader;

namespace EHRIS.Tools.Crypto
{
    /// <summary>
    /// 密碼/憑證管理器
    /// </summary>
    public class CredentialManager
    {
        private readonly string _masterKey;
        public CredentialManager(string masterKey)
        {
            _masterKey = masterKey ?? throw new ArgumentNullException(nameof(masterKey));
        }

        public class AccountCredential
        {
            /// <summary>
            /// Account
            /// </summary>
            public required string Account { get; set; }
            /// <summary>
            /// 密碼加密後的密文
            /// </summary>
            public required string EncryptedPassword { get; set; }
        }

        /// <summary>
        /// 帳號加密 (如email, emailpwd)
        /// </summary>
        /// <param name="account">帳號</param>
        /// <param name="plainPassword">密碼</param>
        /// <returns></returns>
        public AccountCredential EncryptCredential(string account, string plainPassword)
        {
            string encryptedPassword = SecureEncryptor.Encrypt(plainPassword, _masterKey, account);
            return new AccountCredential
            {
                Account = account,
                EncryptedPassword = encryptedPassword
            };
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="credential"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        public string DecryptPassword(AccountCredential credential, string account)
        {
            return SecureEncryptor.Decrypt(credential.EncryptedPassword, _masterKey, account);
        }

        /// <summary>
        /// 驗證密碼
        /// </summary>
        /// <param name="credential"></param>
        /// <param name="inputPassword"></param>
        /// <returns></returns>
        public bool VerifyPassword(AccountCredential credential, string inputPassword)
        {
            try
            {
                string decrypted = SecureEncryptor.Decrypt(credential.EncryptedPassword, _masterKey, credential.Account);

                return CryptographicOperations.FixedTimeEquals(
                                   Encoding.UTF8.GetBytes(decrypted),
                                   Encoding.UTF8.GetBytes(inputPassword)
                               );
            }
            catch (CryptographicException)
            {
                // 解密失敗（可能是 AAD 不一致或密文遭竄改）
                return false;
            }

        }
    }
}