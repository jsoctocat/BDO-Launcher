using System;
using System.Linq;
using System.Security.Cryptography;
using Timer = System.Timers.Timer;

namespace Launcher.Source
{
    public class Otp
    {
        public string GetOneTimePassword(string MasterOTP)
        {
            // https://tools.ietf.org/html/rfc4226
            // https://tools.ietf.org/html/rfc6238
            // https://security.stackexchange.com/questions/194782/is-using-hotp-only-authorization-considered-weak
            // https://security.stackexchange.com/questions/178746/how-can-authy-use-google-authenticator-qr
            // https://github.com/kspearrin/Otp.NET/
            
            long timestampUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
            
            byte[] timestampBytes = new byte[8];
            for (int i = 7; i >= 0; i--)
            {
                timestampBytes[i] = (byte)(timestampUnix & 0xFF);
                timestampUnix >>= 8;
            }

            var masterOTPinBytes = Base32Converter.ToBytes(MasterOTP);
            using var hmac = new HMACSHA1(masterOTPinBytes);
            var hash = hmac.ComputeHash(timestampBytes);

            // same as Hmac[Hmac.Length - 1]
            int offset = hash[^1] & 0x0F;
            int calculatedOTP = 
                ((hash[offset] & 0x7f) << 24) |
                ((hash[offset + 1] & 0xff) << 16) |
                ((hash[offset + 2] & 0xff) << 8) |
                (hash[offset + 3] & 0xff);
            
            int otp = calculatedOTP % 1_000_000;
            return otp.ToString("D6");
        }
    }
}