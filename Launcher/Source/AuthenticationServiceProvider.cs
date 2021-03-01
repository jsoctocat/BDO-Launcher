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
        private readonly Uri _launcherStateToken = 
            new Uri("https://launcher.naeu.playblackdesert.com/Login/Index");
        
        private readonly Uri _authenticationToken = 
            new Uri("https://account.pearlabyss.com/en-US/Launcher/Login/LoginProcess");
        
        private readonly Uri _authenticationTokenEndPoint =
            new Uri("https://launcher.naeu.playblackdesert.com/Default/AuthenticateAccount"); 
        
        private readonly HttpClient _httpClient;
        private readonly CookieContainer _cookieContainer = new CookieContainer();

        public AuthenticationServiceProvider()
        {
            this._httpClient = new HttpClient();
        }

        public void Dispose()
        {
            this._httpClient?.Dispose();
        }

        public async Task<string> AuthenticateAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));
            
            var request = (HttpWebRequest)WebRequest.Create(_launcherStateToken);
            
            //Set an account language (en-US) cookie to avoid duplicate request from fetching
            _cookieContainer.Add(new Cookie("Account_lang", "en-US") { Domain = _authenticationToken.Host });
            
            //Pearl Abyss launcher need a cookie container to storage security cookies.
            request.CookieContainer = _cookieContainer;
            request.UserAgent = "BLACKDESERT";
            request.Method = "GET";
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseUri = response.ResponseUri.ToString();

                //construct the state token from response url
                var startPos = responseUri.LastIndexOf("%26state", StringComparison.Ordinal) + "%26state%3d".Length;
                var length = responseUri.IndexOf("%26client_id", StringComparison.Ordinal) - startPos;
                var stateToken = responseUri.Substring(startPos, length);

                var playTokenEndpoint = await this.RequestAuthenticationTokenAsync(stateToken, username, password);
            
                return await this.RequestPlayTokenAsync(playTokenEndpoint);
            }
        }

        private async Task<string> RequestAuthenticationTokenAsync(string stateToken, string username, string password)
        {
            if (string.IsNullOrEmpty(stateToken))
                throw new ArgumentNullException(nameof(stateToken));
            
            using (var handler = new HttpClientHandler { CookieContainer = _cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = _authenticationToken })
            {
                //required parameter(s): _returnUrl, joinType, username, password
                var data = "_returnUrl=https://account.pearlabyss.com/en-US/Member/Login/AuthorizeOauth?response_type=code%26scope=profile%26state=" + stateToken + "%26client_id=client_id%26redirect_uri=https://launcher.naeu.playblackdesert.com/Login/Oauth2CallBack" +
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
            
                    var request = (HttpWebRequest)WebRequest.Create(resultJson["returnUrl"].Value<string>());
            
                    request.CookieContainer = _cookieContainer;
                    request.Method = "GET";
                    request.UserAgent = "BLACKDESERT";
            
                    var response = (HttpWebResponse)request.GetResponse();

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

        private async Task<string> RequestPlayTokenAsync(string authenticationToken)
        {
            if (string.IsNullOrEmpty(authenticationToken))
                throw new ArgumentNullException(nameof(authenticationToken));

            //Disable UseCookies, only authentication token is required to start the game from here
            using (var handler = new HttpClientHandler { UseCookies = false })
            using (var client = new HttpClient(handler) { BaseAddress = _authenticationTokenEndPoint })
            {
                var message = new HttpRequestMessage(HttpMethod.Post, _authenticationTokenEndPoint);
                message.Headers.Add("User-Agent", "BLACKDESERT");
                message.Headers.Add("Cookie", authenticationToken);
                message.Content = new StringContent("serverKey=NA", Encoding.UTF8, "application/x-www-form-urlencoded");
                
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
