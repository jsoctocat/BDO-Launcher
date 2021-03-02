using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{

    public class Configuration
    {

        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        public byte[] EncryptedPassword
        {
            get { return encryptedPassword; }
            set { encryptedPassword = value; }
        }
        
        public int RegionComboBox
        {
            get { return regionComboBox; }
            set { regionComboBox = value; }
        }

        public bool OTP
        {
            get { return otp; }
            set { otp = value; }
        }

        public bool RememberData
        {
            get { return rememberData; }
            set { rememberData = value; }
        }

        public bool LoginAutomatically
        {
            get { return loginAutomatically; }
            set { loginAutomatically = value; }
        }

        public string GameDirectoryPath
        {
            get { return gameDirectoryPath; }
            set { gameDirectoryPath = value; }
        }

        private string username;
        private byte[] encryptedPassword;
        private int regionComboBox;
        private bool otp;
        private bool rememberData;
        private bool loginAutomatically;
        private string gameDirectoryPath;

        public Configuration()
        {
            this.username = null;
            this.encryptedPassword = null;
            this.regionComboBox = 0;
            this.otp = false;
            this.rememberData = false;
            this.loginAutomatically = false;
            this.gameDirectoryPath = null;
        }

        public string GetPassword()
        {
            if (encryptedPassword == null || !encryptedPassword.Any())
                return null;

            return CryptographyManager.Decrypt(encryptedPassword);
        }

        public void SetPassword(string password)
        {
            if (String.IsNullOrEmpty(password))
                encryptedPassword = null;
            else
                encryptedPassword = CryptographyManager.Encrypt(password);
        }

    }

}
