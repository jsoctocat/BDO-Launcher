using System;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Launcher
{
    public class AuthenticationServiceProvider : IDisposable
    {
        private readonly Uri _launcherReturnUrl = 
            new Uri("https://launcher.naeu.playblackdesert.com/Login/Index");
        
        private readonly Uri _authenticationToken = 
            new Uri("https://account.pearlabyss.com/en-US/Launcher/Login/LoginProcess");
        
        private readonly Uri _authenticationEndPointOTP = 
            new Uri("https://account.pearlabyss.com/Member/Login/LoginOtpAuth");
        
        private readonly Uri _authenticationEndPoint =
            new Uri("https://launcher.naeu.playblackdesert.com/Default/AuthenticateAccount"); 
        
        private readonly HttpClient _httpClient;
        private readonly CookieContainer _cookieContainer = new CookieContainer();

        public AuthenticationServiceProvider()
        {
            _httpClient = new HttpClient();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        private bool ValidateJSON(string testString)
        {
            try
            {
                JToken.Parse(testString);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> AuthenticateAsync(string username, string password, string region, int otp)
        {
            var request = (HttpWebRequest)WebRequest.Create(_launcherReturnUrl);

            // Pearl Abyss launcher need a cookie container to storage security cookies.
            request.CookieContainer = _cookieContainer;
            request.UserAgent = "BLACKDESERT";
            request.Headers.Add("Accept-Language", "en-US");
            request.Accept = "text/html,application/xhtml+xml,*/*";
            request.Method = "GET";
            
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseUri = response.ResponseUri.ToString();

                //construct the returnUrl from response url
                var startPos = responseUri.LastIndexOf("returnUrl=", StringComparison.Ordinal) + "returnUrl=".Length;
                var length = responseUri.Length - startPos;
                var returnUrl = responseUri.Substring(startPos, length);

                var playTokenEndpoint = await RequestAuthenticationTokenAsync(returnUrl, username, password, otp);
            
                return await RequestPlayTokenAsync(playTokenEndpoint, region);
            }
        }

        private async Task<string> RequestAuthenticationTokenAsync(string returnUrl, string username, string password, int otp)
        {
            if (string.IsNullOrEmpty(returnUrl))
                throw new AuthenticationException("Failed to to get (string)returnUrl within function RequestAuthenticationTokenAsync");
            
            using (var handler = new HttpClientHandler { CookieContainer = _cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = _authenticationToken })
            {
                //required parameter(s): _returnUrl, joinType, username, password
                var data = $"_returnUrl={returnUrl}" +
                           "&_joinType=1" +
                           $"&_email={username}" +
                           $"&_password={password}";
                
                var message = new HttpRequestMessage(HttpMethod.Post, _authenticationToken);
                message.Headers.Add("User-Agent", "BLACKDESERT");
                message.Content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");

                using (var result = await client.SendAsync(message))
                {
                    if (!result.IsSuccessStatusCode)
                        return null;

                    var resultContent = await result.Content.ReadAsStringAsync();
                    if (ValidateJSON(resultContent))
                    {
                        var resultJson = JsonConvert.DeserializeObject<JObject>(await result.Content.ReadAsStringAsync());
                        var returnUrlJson = resultJson["returnUrl"].Value<string>();

                        if (string.IsNullOrEmpty(returnUrlJson))
                            throw new AuthenticationException("Failed to to get (string)returnUrlJson within function RequestAuthenticationTokenAsync");

                        var request = (HttpWebRequest) WebRequest.Create(returnUrlJson);
                    
                        if(otp != 0)
                            request = await VerifyOtpAsync(otp.ToString("D6"), returnUrl, request);
                    
                        if(request == null)
                            throw new AuthenticationException("Failed to to get (HttpWebRequest)request within function RequestAuthenticationTokenAsync");

                        request.CookieContainer = _cookieContainer;
                        request.UserAgent = "BLACKDESERT";
                        request.Method = "GET";
                    
                        using (var response = (HttpWebResponse) request.GetResponse())
                        {
                            foreach (var cookie in _cookieContainer.GetCookies(response.ResponseUri))
                            {
                                var cookieStr = cookie.ToString();
                                if (cookieStr.Contains("naeu.Session="))
                                {
                                    return cookieStr;
                                }
                            }

                            return null;
                        }
                    }
                    else
                    {
                        throw new AuthenticationException("Failed to validate JSON within function RequestAuthenticationTokenAsync");
                    }
                }
            }
        }

        private async Task<HttpWebRequest> VerifyOtpAsync(string otp, string returnUrl, HttpWebRequest request)
        {
            if (string.IsNullOrEmpty(returnUrl))
                throw new AuthenticationException("Failed to to get (string)returnUrl within function VerifyOtpAsync");
            if (request == null)
                throw new AuthenticationException("Failed to to get (HttpWebRequest)request within function VerifyOtpAsync");

            request.CookieContainer = _cookieContainer;
            request.UserAgent = "BLACKDESERT";
            request.Method = "GET";
            
            using (var response = (HttpWebResponse) request.GetResponse())
            {
                var responseUri = response.ResponseUri.ToString();

                //construct the loginEncrypt from response url
                var startPos = responseUri.LastIndexOf("loginEncrypt=", StringComparison.Ordinal) +
                               "loginEncrypt=".Length;
                var length = responseUri.Length - startPos - "&_isChannelling=0".Length;
                var loginEncrypt = responseUri.Substring(startPos, length);
                
                using (var handler = new HttpClientHandler { CookieContainer = _cookieContainer })
                using (var client = new HttpClient(handler) { BaseAddress = _authenticationEndPointOTP })
                {
                    var data = $"_code={otp}&" +
                               $"_returnUrl={returnUrl}&" +
                               $"_isBackUpCodeAuth=false&" +
                               $"_loginEncrypt={loginEncrypt}&" +
                               $"_isLauncehr=true";
                
                    var message = new HttpRequestMessage(HttpMethod.Post, _authenticationEndPointOTP);
                    message.Headers.Add("User-Agent", "BLACKDESERT");
                    message.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    message.Content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");

                    using (var result = await client.SendAsync(message))
                    {
                        if (!result.IsSuccessStatusCode)
                            return null;

                        var resultContent = await result.Content.ReadAsStringAsync();
                        if (ValidateJSON(resultContent))
                        {
                            var resultJson = JsonConvert.DeserializeObject<JObject>(resultContent);
                            var returnUrlJson = resultJson["returnUrl"].Value<string>();
                            
                            //a message will return if encounters an OTP authentication error
                            if (string.IsNullOrEmpty(returnUrlJson))
                                throw new AuthenticationException( $"OTP authentication error, resultMsg: { resultJson["resultMsg"] }");
                        
                            return (HttpWebRequest) WebRequest.Create(returnUrlJson);
                        }
                        else
                        {
                            throw new AuthenticationException("Failed to validate JSON within function VerifyOtpAsync");
                        }
                    }
                }
            }
        }
        
        private async Task<string> RequestPlayTokenAsync(string authenticationToken, string region)
        {
            if (string.IsNullOrEmpty(authenticationToken))
                throw new AuthenticationException("Failed to retrieve authentication token, please check your username and password. (Could also be an issue of an invalid OTP)");

            //Disable UseCookies, only authentication token is required to start the game from here
            using (var handler = new HttpClientHandler { UseCookies = false })
            using (var client = new HttpClient(handler) { BaseAddress = _authenticationEndPoint })
            {
                var message = new HttpRequestMessage(HttpMethod.Post, _authenticationEndPoint);
                message.Headers.Add("User-Agent", "BLACKDESERT");
                message.Headers.Add("Cookie", authenticationToken);
                
                // supports registered PC feature, fully implemented but disabled for now, uncomment the code to use
                // will grab the current PC's mac address
                // var mac = 
                // (
                //     from nic in NetworkInterface.GetAllNetworkInterfaces()
                //     where nic.OperationalStatus == OperationalStatus.Up
                //     select nic.GetPhysicalAddress().ToString()
                // ).FirstOrDefault();
                //
                // if (string.IsNullOrEmpty(mac))
                //     throw new SecurityException(nameof(mac));
                //
                // var macAddr = string.Join ("-", Enumerable.Range(0, 6)
                //     .Select(i => mac.Substring(i * 2, 2)));
    
                message.Content = new StringContent($"macAddr={null}&serverKey={region}", Encoding.UTF8, "application/x-www-form-urlencoded");
                
                using (var result = await client.SendAsync(message))
                {
                    if (!result.IsSuccessStatusCode)
                        return null;
            
                    var resultContent = await result.Content.ReadAsStringAsync();
                    if (ValidateJSON(resultContent))
                    {
                        var resultJson = JsonConvert.DeserializeObject<JObject>(resultContent);
                        var resultMsgJson = resultJson["_result"]["resultMsg"].Value<string>();

                        if (string.IsNullOrEmpty(resultMsgJson))
                            throw new AuthenticationException("Failed to get (string)resultMsgJson within function RequestPlayTokenAsync");

                        return resultMsgJson;
                    }
                    else
                    {
                        throw new AuthenticationException("Failed to validate JSON within function RequestPlayTokenAsync");
                    }
                }
            }
        }
    }
}
