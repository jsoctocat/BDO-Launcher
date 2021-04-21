using System;
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

                postData.AddData($"macAddr={null}&serverKey={AuthenticationServiceProvider.Region}");

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
        private const string LauncherReturnUrl = "https://launcher.naeu.playblackdesert.com/Login/Index";
        private const string AuthenticationEndPoint = "https://launcher.naeu.playblackdesert.com/Default/AuthenticateAccount";
        public static string Region = null;

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
            Region = region;
            string otpString = otp.ToString("D6");
            
            var browserSettings = new BrowserSettings()
            {
                WindowlessFrameRate = 1,
                ImageLoading = CefState.Disabled,
                JavascriptAccessClipboard = CefState.Disabled,
                JavascriptCloseWindows = CefState.Disabled,
                JavascriptDomPaste = CefState.Disabled
            };
            using (var browser = new ChromiumWebBrowser(LauncherReturnUrl, browserSettings))
            {
                browser.RequestHandler = new CustomRequestHandler();
                string errorMsg = null;

                await LoadPageAsync(browser);
                var loginScript = $@"
                    document.querySelector('#_email').value = '{username}';
                    document.querySelector('#_password').value = '{password}';
                    document.querySelector('#btnLogin').click();";

                errorMsg = await CheckErrorMsg(browser, loginScript, "loginScript");
                if (!string.IsNullOrEmpty(errorMsg))
                    return errorMsg;

                var otpScript = $@"
                    document.querySelector('#otpInput1').value = '{otpString[0]}';
                    document.querySelector('#otpInput2').value = '{otpString[1]}';
                    document.querySelector('#otpInput3').value = '{otpString[2]}';
                    document.querySelector('#otpInput4').value = '{otpString[3]}'; 
                    document.querySelector('#otpInput5').value = '{otpString[4]}';
                    document.querySelector('#otpInput6').value = '{otpString[5]}';
                    document.querySelector('.btn.btn_big.btn_blue.btnCheckOtp').click();";
                
                errorMsg = await CheckErrorMsg(browser, otpScript, "otpScript");
                if (!string.IsNullOrEmpty(errorMsg))
                    return errorMsg;

                await LoadPageAsync(browser);
                var maintenanceCheckScript = @"
                    (function(){
                        var query = document.querySelector('.box_error');
                        var result = null;
                        if(query != null)
                            result = query.innerText;
                        return result; 
                    })()";
                await browser.EvaluateScriptAsync(maintenanceCheckScript).ContinueWith(tsk =>
                {
                    if (tsk.Result.Success && tsk.Result.Result != null)
                    {
                        errorMsg = tsk.Result.Result.ToString();
                    }
                });
                if (!string.IsNullOrEmpty(errorMsg))
                    return errorMsg;
                
                await LoadPageAsync(browser, AuthenticationEndPoint);

                var responseData = CustomResourceRequestHandler.ResponseData;
                if (!ValidateJSON(responseData))
                {
                    return "Failed to get PlayToken, ResponseData returned an invalidate JSON";
                }

                var responseJObject = JsonConvert.DeserializeObject<JObject>(responseData);
                // https://stackoverflow.com/questions/28352072/what-does-question-mark-and-dot-operator-mean-in-c-sharp-6-0
                if (responseJObject["_result"]?["resultMsg"] == null)
                    return "Failed to get PlayToken, _result or resultMsg does not exit";
                
                var resultMsg = responseJObject["_result"]["resultMsg"].Value<string>();
                
                if (string.IsNullOrEmpty(resultMsg))
                    return "Failed to get PlayToken, resultMsg was empty";
                
                Cef.Shutdown();
                return resultMsg;
            }
        }

        private async Task<string> CheckErrorMsg(IWebBrowser browser, string javascript, string step)
        {
            // wait for any previous js scripts to finish, might needs a better implementation
            // Callback might be better than time based, since users might have slower internet/computer
            int delayCounter = 250;
            do
            {
                await browser.EvaluateScriptAsync(javascript);
                await Task.Delay(delayCounter);
                delayCounter += 50;
            } while (CustomResourceRequestHandler.ResponseData == null && delayCounter < 1000);
            
            if (string.IsNullOrEmpty(CustomResourceRequestHandler.ResponseData))
                return "Failed to get a reply at step (" + step + ") from Pearl Abyss after waiting for 9000(ms)";

            // Check resultMsg
            var responseJObject = JsonConvert.DeserializeObject<JObject>(CustomResourceRequestHandler.ResponseData);
            CustomResourceRequestHandler.ResponseData = null;
            if (responseJObject.ContainsKey("resultMsg"))
            {
                var errorMsg = responseJObject["resultMsg"].Value<string>();
                if (!string.IsNullOrEmpty(errorMsg))
                    return errorMsg;
            }
            
            // No error found, returning null
            return null;
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
    }
}