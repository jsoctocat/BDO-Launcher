using System.Linq;

namespace Launcher.Source
{

    public class Configuration
    {
        public string Username { get; set; }

        // this field needs to be public for the configuration to be saved properly
        public byte[] EncryptedPassword { get; set; }

        public bool Otp { get; set; }

        // this field needs to be public for the configuration to be saved properly
        public byte[] EncryptedOtp { get; set; }

        public int RegionComboBox { get; set; }

        public bool PcRegistration { get; set; }

        public string MacAddress { get; set; }

        public bool GameMode32Bit { get; set; }

        public bool RememberData { get; set; }

        public bool LoginAutomatically { get; set; }

        public bool LauncherUpdate { get; set; }

        public bool GameUpdate { get; set; }

        public bool RunAsAdmin { get; set; }

        public string GameDirectoryPath { get; set; }

        public Configuration()
        {
            Username = null;
            EncryptedPassword = null;
            Otp = false;
            EncryptedOtp = null;
            RegionComboBox = 0;
            PcRegistration = false;
            MacAddress = null;
            GameMode32Bit = false;
            RememberData = false;
            LoginAutomatically = false;
            LauncherUpdate = false;
            GameUpdate = false;
            RunAsAdmin = false;
            GameDirectoryPath = null;
        }

        public string GetPassword()
        {
            if (EncryptedPassword == null || !EncryptedPassword.Any())
                return null;

            return CryptographyManager.Decrypt(EncryptedPassword);
        }

        public void SetPassword(string password)
        {
            EncryptedPassword = string.IsNullOrEmpty(password) ? null : CryptographyManager.Encrypt(password);
        }

        public string GetOtp()
        {
            if (EncryptedOtp == null || !EncryptedOtp.Any())
                return null;

            return CryptographyManager.Decrypt(EncryptedOtp);
        }

        public void SetOtp(string password)
        {
            EncryptedOtp = string.IsNullOrEmpty(password) ? null : CryptographyManager.Encrypt(password);
        }
    }
}
