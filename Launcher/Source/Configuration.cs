using System;
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

        public bool LaunchOption { get; set; }

        public string LaunchOptions { get; set; }
        
        public bool CoreAffinity { get; set; }
        
        public string AffinityBitmask { get; set; }

        public bool GameMode32Bit { get; set; }

        public bool RememberData { get; set; }

        public bool LoginAutomatically { get; set; }

        public bool LauncherUpdate { get; set; }

        public bool GameUpdate { get; set; }

        public bool RunAsAdmin { get; set; }
        
        public bool DebugMode { get; set; }

        public string GameDirectoryPath { get; set; }

        public Configuration()
        {
            Username = String.Empty;
            EncryptedPassword = Array.Empty<byte>();
            Otp = false;
            EncryptedOtp = Array.Empty<byte>();
            RegionComboBox = 0;
            LaunchOption = false;
            LaunchOptions = String.Empty;
            CoreAffinity = false;
            AffinityBitmask = String.Empty;
            GameMode32Bit = false;
            RememberData = false;
            LoginAutomatically = false;
            LauncherUpdate = false;
            GameUpdate = false;
            RunAsAdmin = false;
            DebugMode = false;
            GameDirectoryPath = String.Empty;
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

        public void SetOtp(string otp)
        {
            EncryptedOtp = string.IsNullOrEmpty(otp) ? null : CryptographyManager.Encrypt(otp);
        }
    }
}
