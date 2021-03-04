using System;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using Timer = System.Timers.Timer;

namespace Launcher
{
    public class Otp : INotifyPropertyChanged
    {
        private byte[] _password;
        private long _timestamp;
        private byte[] _hmac;
        private int _offset;
        private int _oneTimePassword;
        private int _secondsToGo;

        public byte[] Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged("Password");
                CalculateOneTimePassword();
            }
        }
        private long Timestamp
        {
            get => _timestamp;
            set
            {
                _timestamp = value;
                OnPropertyChanged("Timestamp");
            }
        }
        private byte[] Hmac
        {
            get => _hmac;
            set
            {
                _hmac = value;
                OnPropertyChanged("Hmac");
                OnPropertyChanged("HmacPart1");
                OnPropertyChanged("HmacPart2");
                OnPropertyChanged("HmacPart3");
            }
        }
        private int Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                OnPropertyChanged("Offset");
            }
        }
        public int OneTimePassword
        {
            get => _oneTimePassword;
            private set
            {
                _oneTimePassword = value;
                OnPropertyChanged("OneTimePassword");
            }
        }
        private int SecondsToGo
        {
            get => _secondsToGo;
            set
            {
                _secondsToGo = value;
                OnPropertyChanged("SecondsToGo");
                if (SecondsToGo == 30)
                {
                    CalculateOneTimePassword();
                }
            }
        }

        public Otp()
        {
            var timer = new Timer();
            timer.Elapsed +=  (s, e) => SecondsToGo = 30 - Convert.ToInt32(GetUnixTimestamp() % 30);
            timer.Enabled = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
        public byte[] HmacPart1 => _hmac.Take(Offset).ToArray();
        public byte[] HmacPart2 => _hmac.Skip(Offset).Take(4).ToArray();
        public byte[] HmacPart3 => _hmac.Skip(Offset + 4).ToArray();

        private static long GetUnixTimestamp() =>
            Convert.ToInt64(Math.Round((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds));

        private void CalculateOneTimePassword()
        {
            // https://tools.ietf.org/html/rfc4226
            // https://tools.ietf.org/html/rfc6238
            // https://security.stackexchange.com/questions/194782/is-using-hotp-only-authorization-considered-weak
            // https://security.stackexchange.com/questions/178746/how-can-authy-use-google-authenticator-qr
            Timestamp = Convert.ToInt64(GetUnixTimestamp() / 30); 
            var data = BitConverter.GetBytes(Timestamp).Reverse().ToArray();
            Hmac = new HMACSHA1(Password).ComputeHash(data);
            Offset = Hmac.Last() & 0x0F;
            OneTimePassword = (
                ((Hmac[Offset + 0] & 0x7f) << 24) |
                ((Hmac[Offset + 1] & 0xff) << 16) |
                ((Hmac[Offset + 2] & 0xff) << 8) |
                (Hmac[Offset + 3] & 0xff)) % 1000000;
        }
    }
}