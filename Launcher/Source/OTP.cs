using System;
using System.Linq;
using System.Security.Cryptography;
using Timer = System.Timers.Timer;

namespace Launcher.Source
{
    public class Otp
    {
        private byte[] _password;
        private int _offset;
        private int _secondsToGo;

        public byte[] Password
        {
            get => _password;
            set
            {
                _password = value;
                CalculateOneTimePassword();
            }
        }
        private byte[] Hmac { get; set; }
        public int OneTimePassword { get; set; }

        private int SecondsToGo
        {
            get => _secondsToGo;
            set
            {
                _secondsToGo = value;
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

        public byte[] HmacPart1 => Hmac.Take(_offset).ToArray();
        public byte[] HmacPart2 => Hmac.Skip(_offset).Take(4).ToArray();
        public byte[] HmacPart3 => Hmac.Skip(_offset + 4).ToArray();

        private static long GetUnixTimestamp() =>
            Convert.ToInt64(Math.Round((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds));

        private void CalculateOneTimePassword()
        {
            // https://tools.ietf.org/html/rfc4226
            // https://tools.ietf.org/html/rfc6238
            // https://security.stackexchange.com/questions/194782/is-using-hotp-only-authorization-considered-weak
            // https://security.stackexchange.com/questions/178746/how-can-authy-use-google-authenticator-qr
            long timestamp = Convert.ToInt64(GetUnixTimestamp() / 30); 
            var data = BitConverter.GetBytes(timestamp).Reverse().ToArray();
            
            if (Password == null)
                return;
            
            Hmac = new HMACSHA1(Password).ComputeHash(data);
            _offset = Hmac.Last() & 0x0F;
            OneTimePassword = (
                ((Hmac[_offset + 0] & 0x7f) << 24) |
                ((Hmac[_offset + 1] & 0xff) << 16) |
                ((Hmac[_offset + 2] & 0xff) << 8) |
                (Hmac[_offset + 3] & 0xff)) % 1000000;
        }
    }
}