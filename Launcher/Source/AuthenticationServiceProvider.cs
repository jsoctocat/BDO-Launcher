using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.OffScreen;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Launcher
{
    // https://github.com/cefsharp/CefSharp/wiki/General-Usage#request-interception
    public class CustomResourceRequestHandler : CefSharp.Handler.ResourceRequestHandler
    {
        public static string ResponseData;

        private readonly System.IO.MemoryStream _memoryStream = new System.IO.MemoryStream();
        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            // intercept the followings to grab resultMsg
            if (request.Url == "https://account.pearlabyss.com/en-US/Launcher/Login/LoginProcess" ||
                request.Url == "https://account.pearlabyss.com/Member/Login/LoginOtpAuth")
            {
                return CefReturnValue.Continue;
            }
            
            if (request.Url == "https://launcher.naeu.playblackdesert.com/Default/AuthenticateAccount")
            {
                var postData = new PostData();

                postData.AddData($"macAddr={null}&serverKey={AuthenticationServiceProvider._region}");

                request.Method = "POST";
                request.PostData = postData;
                //Set the Content-Type header to whatever suites your requirement
                request.SetHeaderByName("Content-Type", "application/x-www-form-urlencoded", true);
                //Set additional Request headers as required.

                return CefReturnValue.Continue;
            }

            return CefReturnValue.Cancel;
        }
        
        protected override IResponseFilter GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame,
            IRequest request, IResponse response)
        {
            return new CefSharp.ResponseFilter.StreamResponseFilter(_memoryStream);
        }

        protected override void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request,
            IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            //You can now get the data from the stream
            var bytes = _memoryStream.ToArray();

            if (response.Charset == "utf-8")
            {
                ResponseData = System.Text.Encoding.UTF8.GetString(bytes);
                
                // Check resultMsg
                var resultJObject = JsonConvert.DeserializeObject<JObject>(ResponseData);
                if (resultJObject.ContainsKey("resultMsg"))
                {
                    var resultMsg = resultJObject["resultMsg"].Value<string>();
                    if(resultMsg.Length > 0)
                        MessageBox.Show(resultMsg);
                }
            }
            else
            {
                //Deal with different encoding here
            }
        }
    }
    public class CustomRequestHandler : CefSharp.Handler.RequestHandler
    {
        protected override bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            // For security reasons you should perform validation on the url to confirm that it's safe before proceeding.
            if (request.Url.StartsWith("https://launcher.naeu.playblackdesert.com") || 
                request.Url.StartsWith("https://account.pearlabyss.com"))
            {
                return base.OnBeforeBrowse(chromiumWebBrowser, browser, frame, request, userGesture, isRedirect);
            }
            return true;
        }
        
        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            // intercept the followings to grab resultMsg
            if (request.Url == "https://account.pearlabyss.com/en-US/Launcher/Login/LoginProcess" ||
                request.Url == "https://account.pearlabyss.com/Member/Login/LoginOtpAuth" ||
                request.Url == "https://launcher.naeu.playblackdesert.com/Default/AuthenticateAccount")
            {
                return new CustomResourceRequestHandler();
            }
            
            // intercept any Url that's not specified, used to block tracking scripts/sites for faster performance
            if (request.Url.StartsWith("https://launcher.naeu.playblackdesert.com") || 
                request.Url.StartsWith("https://account.pearlabyss.com") ||
                request.Url.Contains("https://s1.pearlcdn.com/account/contents/js"))
            {
                // Default behaviour, url will be loaded normally.
                return null;
            }

            return new CustomResourceRequestHandler();
        }
    }
    
    public class AuthenticationServiceProvider
    {
        private const string _launcherReturnUrl = "https://launcher.naeu.playblackdesert.com/Login/Index";
        private const string _authenticationEndPoint = "https://launcher.naeu.playblackdesert.com/Default/AuthenticateAccount";
        public static string _region = null;
        
        public AuthenticationServiceProvider()
        {
            CefSharpSettings.SubprocessExitIfParentProcessClosed = true;
            var settings = new CefSettings()
            {
                UserAgent = "BLACKDESERT",
                AcceptLanguageList = "en-US"
            };
            
            // Only initialize Cef once, this is a framework limitation
            if(!Cef.IsInitialized)
                Cef.Initialize(settings,true, browserProcessHandler: null);
        }
        
        public async Task<string> AuthenticateAsync(string username, string password, string region, int otp)
        {
            _region = region;
            string otpString = otp.ToString("D6");
            
            var browserSettings = new BrowserSettings()
            {
                WindowlessFrameRate = 1,
                ImageLoading = CefState.Disabled,
                JavascriptAccessClipboard = CefState.Disabled,
                JavascriptCloseWindows = CefState.Disabled,
                JavascriptDomPaste = CefState.Disabled
            };
            
            using (var browser = new ChromiumWebBrowser(_launcherReturnUrl, browserSettings))
            {
                browser.RequestHandler = new CustomRequestHandler();
                
                await LoadPageAsync(browser);
                var loginScript = $@"
                            document.querySelector('#_email').value = '{username}';
                            document.querySelector('#_password').value = '{password}';
                            document.querySelector('#btnLogin').click();";
                await browser.EvaluateScriptAsync(loginScript);

                await LoadPageAsync(browser);
                var otpScript = $@"
                        document.querySelector('#otpInput1').value = '{otpString[0]}';
                        document.querySelector('#otpInput2').value = '{otpString[1]}';
                        document.querySelector('#otpInput3').value = '{otpString[2]}';
                        document.querySelector('#otpInput4').value = '{otpString[3]}'; 
                        document.querySelector('#otpInput5').value = '{otpString[4]}';
                        document.querySelector('#otpInput6').value = '{otpString[5]}';
                        document.querySelector('.btn.btn_big.btn_blue.btnCheckOtp').click();";
                await browser.EvaluateScriptAsync(otpScript).ContinueWith(u =>
                {
                    browser.EvaluateScriptAsync(otpScript);
                });
                
                await LoadPageAsync(browser);
                await LoadPageAsync(browser, _authenticationEndPoint);

                var resultJObject = JsonConvert.DeserializeObject<JObject>(CustomResourceRequestHandler.ResponseData);
                if (resultJObject["_result"]["resultMsg"] == null)
                    throw new AuthenticationException("Failed to get PlayToken");
                
                var resultMsgJson = resultJObject["_result"]["resultMsg"].Value<string>();

                Cef.Shutdown();
                return resultMsgJson;
            }
        }

        private Task LoadPageAsync(IWebBrowser browser, string address = null)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            EventHandler<LoadingStateChangedEventArgs> handler = null;
            handler = (sender, args) =>
            {
                // Wait for while page to finish loading not just the first frame
                if (!args.IsLoading)
                {
                    browser.LoadingStateChanged -= handler;
                    // Important that the continuation runs async using TaskCreationOptions.RunContinuationsAsynchronously
                    tcs.TrySetResult(true);
                }
            };

            browser.LoadingStateChanged += handler;

            if (!string.IsNullOrEmpty(address))
            {
                browser.Load(address);
            }
            return tcs.Task;
        }
    }
}
