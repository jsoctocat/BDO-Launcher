using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{

    public class Configuration
    {
        private string _username;
        private byte[] _encryptedPassword;
        private bool _otp;
        private byte[] _encryptedOtp;
        private int _regionComboBox;
        private bool _gamemode;
        private bool _rememberData;
        private bool _loginAutomatically;
        private string _gameDirectoryPath;
        
        public string Username
        {
            get => _username;
            set => _username = value;
        }
        public byte[] EncryptedPassword
        {
            get => _encryptedPassword;
            set => _encryptedPassword = value;
        }
        public bool Otp
        {
            get => _otp;
            set => _otp = value;
        }
        public byte[] EncryptedOtp
        {
            get => _encryptedOtp;
            set => _encryptedOtp = value;
        }
        public int RegionComboBox
        {
            get => _regionComboBox;
            set => _regionComboBox = value;
        }
        public bool GameMode
        {
            get => _gamemode;
            set => _gamemode = value;
        }
        public bool RememberData
        {
            get => _rememberData;
            set => _rememberData = value;
        }
        public bool LoginAutomatically
        {
            get => _loginAutomatically;
            set => _loginAutomatically = value;
        }
        public string GameDirectoryPath
        {
            get => _gameDirectoryPath;
            set => _gameDirectoryPath = value;
        }

        public Configuration()
        {
            _username = null;
            _encryptedPassword = null;
            _otp = false;
            _encryptedOtp = null;
            _regionComboBox = 0;
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
            if (string.IsNullOrEmpty(password))
                _encryptedPassword = null;
            else
                _encryptedPassword = CryptographyManager.Encrypt(password);
        }

        public string GetOtp()
        {
            if (_encryptedOtp == null || !_encryptedOtp.Any())
                return null;

            return CryptographyManager.Decrypt(_encryptedOtp);
        }

        public void SetOtp(string password)
        {
            if (string.IsNullOrEmpty(password))
                _encryptedOtp = null;
            else
                _encryptedOtp = CryptographyManager.Encrypt(password);
        }

    }

}
