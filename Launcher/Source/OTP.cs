using System;
using System.Linq;
using System.Security.Cryptography;
using Timer = System.Timers.Timer;

namespace Launcher.Source
{
    public class Otp
    {
        private static long GetUnixTimestamp() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public string GetOneTimePassword(string MasterOTP)
        {
            // https://tools.ietf.org/html/rfc4226
            // https://tools.ietf.org/html/rfc6238
            // https://security.stackexchange.com/questions/194782/is-using-hotp-only-authorization-considered-weak
            // https://security.stackexchange.com/questions/178746/how-can-authy-use-google-authenticator-qr
            // https://github.com/kspearrin/Otp.NET/
            long timestamp = Convert.ToInt64(GetUnixTimestamp() / 30); 
            var data = BitConverter.GetBytes(timestamp).Reverse().ToArray();
            var masterOTPinBytes = Base32Converter.ToBytes(MasterOTP);
            var Hmac = new HMACSHA1(masterOTPinBytes).ComputeHash(data);
            // same as Hmac[Hmac.Length - 1]
            int offset = Hmac[^1] & 0x0F;
            var calculatedOTP = (Hmac[offset] & 0x7f) << 24 |
                              (Hmac[offset + 1] & 0xff) << 16 |
                              (Hmac[offset + 2] & 0xff) << 8 |
                              (Hmac[offset + 3] & 0xff);
            var truncatedValue = calculatedOTP % (int)Math.Pow(10, 6);
            return truncatedValue.ToString("D6");
        }
    }
}