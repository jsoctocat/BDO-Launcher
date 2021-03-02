using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;

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

        public async Task<string> AuthenticateAsync(string username, string password, string region, int otp)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));
            
            var request = (HttpWebRequest)WebRequest.Create(_launcherReturnUrl);
            
            //Set an account language (en-US) cookie to avoid duplicate request from fetching
            _cookieContainer.Add(new Cookie("Account_lang", "en-US") { Domain = _authenticationToken.Host });
            
            //Pearl Abyss launcher need a cookie container to storage security cookies.
            request.CookieContainer = _cookieContainer;
            request.UserAgent = "BLACKDESERT";
            request.Method = "GET";
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseUri = response.ResponseUri.ToString();

                //construct the returnUrl from response url
                var startPos = responseUri.LastIndexOf("=https", StringComparison.Ordinal) + 1;
                var length = responseUri.Length - startPos;
                var returnUrl = responseUri.Substring(startPos, length);

                var playTokenEndpoint = await RequestAuthenticationTokenAsync(returnUrl, username, password, otp);
            
                return await RequestPlayTokenAsync(playTokenEndpoint, region);
            }
        }

        private async Task<string> RequestAuthenticationTokenAsync(string returnUrl, string username, string password, int otp)
        {
            if (string.IsNullOrEmpty(returnUrl))
                throw new ArgumentNullException(nameof(returnUrl));
            
            using (var handler = new HttpClientHandler { CookieContainer = _cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = _authenticationToken })
            {
                //required parameter(s): _returnUrl, joinType, username, password
                var data = $"_returnUrl={returnUrl}" +
                           "&_joinType=1" +
                           "&_email=" + username +
                           "&_password=" + password;
                
                var message = new HttpRequestMessage(HttpMethod.Post, _authenticationToken);
                message.Headers.Add("User-Agent", "BLACKDESERT");
                message.Content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");

                using (var result = await client.SendAsync(message))
                {
                    if (!result.IsSuccessStatusCode)
                        return null;

                    var resultJson = JsonConvert.DeserializeObject<JObject>(await result.Content.ReadAsStringAsync());

                    if (resultJson["returnUrl"] == null)
                        return null;

                    var request = (HttpWebRequest) WebRequest.Create(resultJson["returnUrl"].Value<string>());
                    
                    if(otp != 0)
                        request = await VerifyOTPAsync(otp, returnUrl, request);

                    request.CookieContainer = _cookieContainer;
                    request.Method = "GET";
                    request.UserAgent = "BLACKDESERT";
                    
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
            }
        }

        private async Task<HttpWebRequest> VerifyOTPAsync(int otp, string returnUrl, HttpWebRequest request)
        {
            if (string.IsNullOrEmpty(returnUrl))
                throw new ArgumentNullException(nameof(returnUrl));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            request.CookieContainer = _cookieContainer;
            request.Method = "GET";
            request.UserAgent = "BLACKDESERT";
            
            using (var response = (HttpWebResponse) request.GetResponse())
            {
                var responseUri = response.ResponseUri.ToString();

                //construct the loginEncrypt from response url
                var startPos = responseUri.LastIndexOf("_loginEncrypt=", StringComparison.Ordinal) +
                               "_loginEncrypt=".Length;
                var length = responseUri.Length - startPos;
                var loginEncrypt = responseUri.Substring(startPos, length);
                
                using (var handler = new HttpClientHandler { CookieContainer = _cookieContainer })
                using (var client = new HttpClient(handler) { BaseAddress = _authenticationEndPointOTP })
                {
                    var data = $"_code={otp}&_returnUrl={returnUrl}&_isBackUpCodeAuth=false&_loginEncrypt={loginEncrypt}";
                
                    var message = new HttpRequestMessage(HttpMethod.Post, _authenticationEndPointOTP);
                    message.Headers.Add("User-Agent", "BLACKDESERT");
                    message.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    message.Content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");

                    using (var result = await client.SendAsync(message))
                    {
                        if (!result.IsSuccessStatusCode)
                            return null;

                        var resultJson = JsonConvert.DeserializeObject<JObject>(await result.Content.ReadAsStringAsync());
                        
                        if (resultJson["okUrl"] == null)
                            return null;

                        return (HttpWebRequest) WebRequest.Create(resultJson["okUrl"].Value<string>());
                    }
                }
            }
        }
        
        private async Task<string> RequestPlayTokenAsync(string authenticationToken, string region)
        {
            if (string.IsNullOrEmpty(authenticationToken))
                throw new ArgumentNullException(nameof(authenticationToken));

            //Disable UseCookies, only authentication token is required to start the game from here
            using (var handler = new HttpClientHandler { UseCookies = false })
            using (var client = new HttpClient(handler) { BaseAddress = _authenticationEndPoint })
            {
                var message = new HttpRequestMessage(HttpMethod.Post, _authenticationEndPoint);
                message.Headers.Add("User-Agent", "BLACKDESERT");
                message.Headers.Add("Cookie", authenticationToken);
                message.Content = new StringContent("serverKey=" + region, Encoding.UTF8, "application/x-www-form-urlencoded");
                
                using (var result = await client.SendAsync(message))
                {
                    if (!result.IsSuccessStatusCode)
                        return null;
            
                    var responseJObject = JsonConvert.DeserializeObject<JObject>(await result.Content.ReadAsStringAsync());
            
                    if (responseJObject["_result"]["resultMsg"] == null)
                        return null;

                    return responseJObject["_result"]["resultMsg"].Value<string>();
                }
            }
        }
    }
}
