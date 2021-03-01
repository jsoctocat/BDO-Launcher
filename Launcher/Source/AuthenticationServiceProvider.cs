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
        private static readonly string _authenticationTokenEndpoint = "https://www.blackdesertonline.com/launcher/ll/api/Login.json";
        private static readonly string _playTokenEndpoint = "https://www.blackdesertonline.com/launcher/I/api/CreatePlayToken.json";

        private HttpClient httpClient;

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

            string authenticationToken = await this.RequestAuthenticationTokenAsync(username, password);

            if (authenticationToken == null)
                return null;

            return await this.RequestPlayTokenAsync(authenticationToken);
        }

        private async Task<string> RequestAuthenticationTokenAsync(string username, string password)
        {
            using (HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _authenticationTokenEndpoint))
            {
                httpRequestMessage.Content = new StringContent($"email={username}&password={password}", Encoding.UTF8, "application/x-www-form-urlencoded");
            
                using (HttpResponseMessage httpResponseMessage = await this.httpClient.SendAsync(httpRequestMessage))
                {
                    if (!httpResponseMessage.IsSuccessStatusCode)
                        return null;
            
                    JObject responseJObject = JsonConvert.DeserializeObject<JObject>(await httpResponseMessage.Content.ReadAsStringAsync());
            
                    if ((responseJObject["result"] == null) || (responseJObject["result"]["token"] == null))
                        return null;
            
                    return responseJObject["result"]["token"].Value<String>();
                }
            }
        }

        private async Task<string> RequestPlayTokenAsync(string authenticationToken)
        {
            if (String.IsNullOrEmpty(authenticationToken))
                throw new ArgumentNullException(nameof(authenticationToken));
            
            using (HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _playTokenEndpoint))
            {
                httpRequestMessage.Content = new StringContent($"token={authenticationToken}&lang=EN&region=NA&tou_date=&tou_try_count=1", Encoding.UTF8, "application/x-www-form-urlencoded");
            
                using (HttpResponseMessage httpResponseMessage = await this.httpClient.SendAsync(httpRequestMessage))
                {
                    if (!httpResponseMessage.IsSuccessStatusCode)
                        return null;
            
                    JObject responseJObject = JsonConvert.DeserializeObject<JObject>(await httpResponseMessage.Content.ReadAsStringAsync());
            
                    if ((responseJObject["resultMsg"] == null))
                        return null;
            
                    return responseJObject["resultMsg"].Value<String>();
                }
            }
        }
    }
}
