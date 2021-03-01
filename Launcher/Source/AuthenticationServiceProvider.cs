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
        private readonly Uri _launcherStateToken =  new Uri("https://launcher.naeu.playblackdesert.com/Login/Index");
        private readonly Uri _authenticationToken = new Uri("https://account.pearlabyss.com/en-US/Launcher/Login/LoginProcess");
        private readonly Uri _authenticationTokenEndPoint =
            new Uri("https://launcher.naeu.playblackdesert.com/Default/AuthenticateAccount"); 
        
        private HttpClient httpClient;
        private CookieContainer _cookieContainer = new CookieContainer();

        public AuthenticationServiceProvider()
        {
            this.httpClient = new HttpClient();
        }

        public void Dispose()
        {
            this.httpClient?.Dispose();
        }

        public async Task<string> AuthenticateAsync(string username, string password)
        {
            if (String.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));

            if (String.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));
            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_launcherStateToken);
            
            //PA launcher need a cookie container to storage additional security info.
            _cookieContainer.Add(new Cookie("Account_lang", "en-US") { Domain = _authenticationToken.Host });
            request.CookieContainer = _cookieContainer;
            request.UserAgent = "BLACKDESERT";
            request.Method = "GET";
            var response = (HttpWebResponse)request.GetResponse();
            var responseUri = response.ResponseUri.ToString();

            var startPos = responseUri.LastIndexOf("%26state", StringComparison.Ordinal) + "%26state%3d".Length;
            var length = responseUri.IndexOf("%26client_id", StringComparison.Ordinal) - startPos;
            var stateToken = responseUri.Substring(startPos, length);

            var returnUrl =
                "https://account.pearlabyss.com/en-US/Member/Login/AuthorizeOauth?response_type=code%26scope=profile%26state=" + stateToken + "%26client_id=client_id%26redirect_uri=https://launcher.naeu.playblackdesert.com/Login/Oauth2CallBack";

            var playTokenEndpoint = await this.RequestAuthenticationTokenAsync(returnUrl, username, password);
            
            return await this.RequestPlayTokenAsync(playTokenEndpoint);
        }

        private async Task<string> RequestAuthenticationTokenAsync(string returnUrl, string username, string password)
        {
            using (var handler = new HttpClientHandler { CookieContainer = _cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = _authenticationToken })
            {
                var data = "_returnUrl=" + returnUrl + "&_joinType=1&_email=" + username + "&_password=" + password;
                var message = new HttpRequestMessage(HttpMethod.Post, _authenticationToken);
                message.Headers.Add("User-Agent", "BLACKDESERT");
                message.Content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");
                
                using (var result = await client.SendAsync(message))
                {
                    if (!result.IsSuccessStatusCode)
                        return null;
            
                    JObject responseJObject = JsonConvert.DeserializeObject<JObject>(await result.Content.ReadAsStringAsync());
            
                    if (responseJObject["returnUrl"] == null)
                        return null;
            
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(responseJObject["returnUrl"].Value<String>());
            
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
            if (String.IsNullOrEmpty(authenticationToken))
                throw new ArgumentNullException(nameof(authenticationToken));

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
            
                    JObject responseJObject = JsonConvert.DeserializeObject<JObject>(await result.Content.ReadAsStringAsync());
            
                    if (responseJObject["_result"]["resultMsg"] == null)
                        return null;

                    return responseJObject["_result"]["resultMsg"].Value<String>();
                }
            }
        }
    }
}
