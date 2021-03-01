using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Launcher
{

    public static class CryptographyManager
    {

        private static readonly int _keySize = (256 / 8);
        private static readonly Encoding _defaultEncoding = Encoding.UTF8;

        private static RNGCryptoServiceProvider _rngCryptoServiceProvider;
        private static RijndaelManaged _rijndaelManaged;

        static CryptographyManager()
        {
            _rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            _rijndaelManaged = new RijndaelManaged()
            {
                BlockSize = 256,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };
        }

        public static byte[] Encrypt(string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));

            byte[] salt = GenerateRandomSeed();
            byte[] vector = GenerateRandomSeed();
            byte[] valueRaw = _defaultEncoding.GetBytes(value);

            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(GetAssemblyGuid(), salt))
            {
                byte[] derivedData = rfc2898DeriveBytes.GetBytes(_keySize);

                using (ICryptoTransform cryptoTransform = _rijndaelManaged.CreateEncryptor(derivedData, vector))
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

            byte[] salt = value.Take(_keySize).ToArray();
            byte[] vector = value.Skip(_keySize).Take(_keySize).ToArray();
            byte[] encryptedValue = value.Skip(_keySize * 2).Take(_keySize).ToArray();

            using (Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(GetAssemblyGuid(), salt))
            {
                byte[] derivedData = rfc2898DeriveBytes.GetBytes(_keySize);

                using (ICryptoTransform cryptoTransform = _rijndaelManaged.CreateDecryptor(derivedData, vector))
                using (MemoryStream memoryStream = new MemoryStream(encryptedValue))
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read))
                {
                    byte[] decryptedValue = new byte[encryptedValue.Length];

                    int decryptedValueLength = cryptoStream.Read(decryptedValue, 0, decryptedValue.Length);

                    return _defaultEncoding.GetString(decryptedValue, 0, decryptedValueLength);
                }
            }
        }

        private static byte[] GenerateRandomSeed()
        {
            byte[] randomSeed = new byte[32];

            _rngCryptoServiceProvider.GetBytes(randomSeed);

            return randomSeed;
        }

        private static string GetAssemblyGuid() => (Assembly.GetExecutingAssembly().GetCustomAttribute(typeof(GuidAttribute)) as GuidAttribute).Value.Replace("-", String.Empty);

    }

}
