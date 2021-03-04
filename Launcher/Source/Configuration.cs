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
            get { return _username; }
            set { _username = value; }
        }

        public byte[] EncryptedPassword
        {
            get { return _encryptedPassword; }
            set { _encryptedPassword = value; }
        }
        
        public int RegionComboBox
        {
            get { return _regionComboBox; }
            set { _regionComboBox = value; }
        }

        public bool OTP
        {
            get { return _otp; }
            set { _otp = value; }
        }

        public bool GameMode
        {
            get { return _gamemode; }
            set { _gamemode = value; }
        }

        public bool RememberData
        {
            get { return _rememberData; }
            set { _rememberData = value; }
        }

        public bool LoginAutomatically
        {
            get { return _loginAutomatically; }
            set { _loginAutomatically = value; }
        }

        public string GameDirectoryPath
        {
            get { return _gameDirectoryPath; }
            set { _gameDirectoryPath = value; }
        }

        private string _username;
        private byte[] _encryptedPassword;
        private int _regionComboBox;
        private bool _otp;
        private bool _gamemode;
        private bool _rememberData;
        private bool _loginAutomatically;
        private string _gameDirectoryPath;

        public Configuration()
        {
            _username = null;
            _encryptedPassword = null;
            _regionComboBox = 0;
            _otp = false;
            _gamemode = false;
            _rememberData = false;
            _loginAutomatically = false;
            _gameDirectoryPath = null;
        }

        public string GetPassword()
        {
            if (_encryptedPassword == null || !_encryptedPassword.Any())
                return null;

            return CryptographyManager.Decrypt(_encryptedPassword);
        }

        public void SetPassword(string password)
        {
            if (String.IsNullOrEmpty(password))
                _encryptedPassword = null;
            else
                _encryptedPassword = CryptographyManager.Encrypt(password);
        }

    }

}
