using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Launcher.Source
{
    public static class CryptographyManager
    {
        private static readonly int _keySize = 128 / 8;
        private static Aes _aes;

        static CryptographyManager()
        {
            _aes = Aes.Create();
            _aes.BlockSize = 128;
            _aes.Mode = CipherMode.CBC;
            _aes.Padding = PaddingMode.PKCS7;
        }

        public static byte[] Encrypt(string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            byte[] salt = RandomNumberGenerator.GetBytes(_keySize);
            byte[] vector = RandomNumberGenerator.GetBytes(_keySize);
            byte[] valueRaw = Encoding.UTF8.GetBytes(value);

            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(GetAssemblyGuid(), salt, 1000, HashAlgorithmName.SHA256))
            {
                byte[] derivedData = rfc2898DeriveBytes.GetBytes(_keySize);

                using (ICryptoTransform cryptoTransform = _aes.CreateEncryptor(derivedData, vector))
                using (MemoryStream memoryStream = new MemoryStream())
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(valueRaw, 0, valueRaw.Length);
                    cryptoStream.FlushFinalBlock();

                    byte[] encryptedValue = new byte[0];

                    encryptedValue = encryptedValue.Concat(salt).ToArray();
                    encryptedValue = encryptedValue.Concat(vector).ToArray();
                    encryptedValue = encryptedValue.Concat(memoryStream.ToArray()).ToArray();

                    return encryptedValue;
                }
            }
        }

        public static string Decrypt(byte[] value)
        {
            if ((value == null) || !value.Any())
                throw new ArgumentNullException(nameof(value));

            try
            {
                byte[] salt = value.Take(_keySize).ToArray();
                byte[] vector = value.Skip(_keySize).Take(_keySize).ToArray();
                byte[] encryptedValue = value.Skip(_keySize * 2).Take(value.Length - _keySize * 2).ToArray();

                using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(GetAssemblyGuid(), salt, 1000, HashAlgorithmName.SHA256))
                {
                    byte[] derivedData = rfc2898DeriveBytes.GetBytes(_keySize);

                    using (ICryptoTransform cryptoTransform = _aes.CreateDecryptor(derivedData, vector))
                    using (MemoryStream memoryStream = new MemoryStream(encryptedValue))
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read))
                    using (var plainTextReader = new StreamReader(cryptoStream))
                        return plainTextReader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                return String.Empty;
            }
        }

        private static string GetAssemblyGuid() => (Assembly.GetExecutingAssembly().GetCustomAttribute(typeof(GuidAttribute)) as GuidAttribute).Value.Replace("-", String.Empty);
    }

}
