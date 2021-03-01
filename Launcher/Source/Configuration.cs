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
            get { return this.username; }
            set { this.username = value; }
        }

        public byte[] EncryptedPassword
        {
            get { return this.encryptedPassword; }
            set { this.encryptedPassword = value; }
        }

        public bool RememberData
        {
            get { return this.rememberData; }
            set { this.rememberData = value; }
        }

        public bool LoginAutomatically
        {
            get { return this.loginAutomatically; }
            set { this.loginAutomatically = value; }
        }

        public string GameDirectoryPath
        {
            get { return this.gameDirectoryPath; }
            set { this.gameDirectoryPath = value; }
        }

        private string username;
        private byte[] encryptedPassword;
        private bool rememberData;
        private bool loginAutomatically;
        private string gameDirectoryPath;

        public Configuration()
        {
            this.username = null;
            this.encryptedPassword = null;
            this.rememberData = false;
            this.loginAutomatically = false;
            this.gameDirectoryPath = null;
        }

        public string GetPassword()
        {
            if ((this.encryptedPassword == null) || !this.encryptedPassword.Any())
                return null;

            return CryptographyManager.Decrypt(this.encryptedPassword);
        }

        public void SetPassword(string password)
        {
            if (String.IsNullOrEmpty(password))
                this.encryptedPassword = null;
            else
                this.encryptedPassword = CryptographyManager.Encrypt(password);
        }

    }

}
