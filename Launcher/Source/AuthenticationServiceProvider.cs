using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Launcher.Source;

public class AuthenticationServiceProvider
{
    public async Task<string> AuthenticateAsync(IBrowserContext browser, string username, string password,
        string region,
        bool useOtp, bool useMasterOtp, string otp)
    {
        string launcherRegion = (region == "NA" || region == "EU") ? "naeu" : region.ToLower();
        string LauncherReturnUrl = "https://launcher." + launcherRegion + ".playblackdesert.com/Login/Index";
        string AuthenticationEndPoint = "https://launcher." + launcherRegion + ".playblackdesert.com/Default/AuthenticateAccount";
        
        try
        {
            /* since the removal of pc registration, mac address is no longer needed
            if (macAddress == "?")
            {
                // will grab the current PC's mac address
                macAddress =
                (
                    from nic in NetworkInterface.GetAllNetworkInterfaces()
                    where nic.OperationalStatus == OperationalStatus.Up
                    select nic.GetPhysicalAddress().ToString()
                ).FirstOrDefault();

                if (string.IsNullOrEmpty(macAddress))
                    throw new SecurityException(nameof(macAddress));

                macAddress = string.Join ("-", Enumerable.Range(0, 6)
                    .Select(i => macAddress.Substring(i * 2, 2)));
            }
            */

            var page = await browser.NewPageAsync();

            // intercept any Url that's not specified, used to block tracking scripts/sites for faster performance
            await page.RouteAsync("**/*", async route =>
            {
                if (route.Request.Url.StartsWith("https://launcher." + launcherRegion + ".playblackdesert.com") ||
                    route.Request.Url.StartsWith("https://account.pearlabyss.com") ||
                    route.Request.Url.StartsWith("https://s1.pearlcdn.com/account/contents/js") ||
                    route.Request.Url.Contains("hcaptcha.com"))
                {
                    await route.ContinueAsync();
                }
                else
                {
                    await route.AbortAsync();
                }
            });
            
            await page.GotoAsync(LauncherReturnUrl);
            
            // Check for maintenance
            string errorMsg = await AdditionalErrorsCheck(page, "maintenance");
            if (!string.IsNullOrEmpty(errorMsg))
                return errorMsg;

            // Wait for login page to load
            await page.WaitForSelectorAsync("input[id=_email]");

            await page.FillAsync("#_email", $"{username}");
            await page.FillAsync("#_password", $"{password}");
            await page.SetCheckedAsync("#isIpCheck", false);
            await page.ClickAsync("#btnLogin");

            errorMsg = await CheckErrorMsg(page, "loginScript");
            if (!string.IsNullOrEmpty(errorMsg))
                return errorMsg;

            if (useOtp)
            {
                // Wait for the OTP page to load first
                // This checks for whether or not OTP input boxes are loaded
                await page.WaitForSelectorAsync(".input_otp_wrap.js-inputNumWrap");
                
                Otp _otp = new Otp();
                string otpString = null;
                // if OTP input is not null or empty then use it instead of master OTP
                if (useMasterOtp)
                {
                    otpString = _otp.GetOneTimePassword(otp);
                    // \s is white space + is more than 1
                    // https://stackoverflow.com/questions/6219454/efficient-way-to-remove-all-whitespace-from-string
                    Regex.Replace(otpString, @"\s+", "0");
                }
                else
                {
                    otpString = otp;
                }
                
                // Fill the OTP
                await page.EvaluateAsync<int>($@"async () => {{
                    document.querySelector('#otpInput1').value = '{otpString[0]}';
                    document.querySelector('#otpInput2').value = '{otpString[1]}';
                    document.querySelector('#otpInput3').value = '{otpString[2]}';
                    document.querySelector('#otpInput4').value = '{otpString[3]}';
                    document.querySelector('#otpInput5').value = '{otpString[4]}';
                    document.querySelector('#otpInput6').value = '{otpString[5]}';
                }}");
                
                // Check for OTP Confirm button disable state :not([disabled])
                await page.WaitForSelectorAsync(".btn.btn_big.btn_blue.btnCheckOtp:not([disabled])");
                
                await page.ClickAsync(".btn.btn_big.btn_blue.btnCheckOtp:not([disabled])");

                errorMsg = await CheckErrorMsg(page, "otpScript");
                if (!string.IsNullOrEmpty(errorMsg))
                    return errorMsg;
            }

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Check for password change notification
            errorMsg = await AdditionalErrorsCheck(page, "password change");
            if (!string.IsNullOrEmpty(errorMsg))
            {
                if (errorMsg.Contains("Change Password"))
                {
                    var msgBox =  MsgBoxManager.GetMessageBox("Password Too Old Error",
                        $"Your password is too old, please login using the official launcher to change your password\n\nThis error is from the game server, it will come up every 3 months",
                        true, false, false,
                        async void () => { 
                            await page.ClickAsync("#btnPasswordChangeLater"); });
                    
                    await msgBox.ShowAsync();
                }
            }
            
            // Wait for Start Game btn to load
            await page.WaitForSelectorAsync("a[id=btnGamePlay]");
            
            // Push a POST msg to fetch login code
            var data = new Dictionary<string, string>();
            data.Add("macAddr", String.Empty);
            data.Add("serverKey", region);
            var request = await page.APIRequest.PostAsync(AuthenticationEndPoint, new() { DataObject = data });
            
            // Check if there are any errors from login process
            var xhrPayload = await request.JsonAsync();
            
            if (xhrPayload.Value.ValueKind == JsonValueKind.Object)
            {
                var _result = xhrPayload.Value.GetProperty("_result");
                var resultData = _result.GetProperty("resultData").GetString();
                var resultCode = _result.GetProperty("resultCode").GetInt32();
                var resultMsg = _result.GetProperty("resultMsg").GetString();
                var returnUrl = _result.GetProperty("returnUrl").GetString();
                
                var _policyList = xhrPayload.Value.GetProperty("_policyList").ToString();
                
                if (resultCode == 0)
                    return resultMsg;
                
                return $"Failed to get PlayToken, resultMsg was empty, resultCode was {resultCode}{resultMsg}";
            }

            return "Failed to get PlayToken, _result or resultMsg does not exit";
        }
        catch (Exception e)
        {
            MsgBoxManager.GetMessageBox("AuthenticateAsync Failed", e.Message);
            throw;
        }
    }

    private async Task<string> CheckErrorMsg(IPage page, string step)
    {
        // This needs to be before login btn is clicked, it will get the response of login process
        var errorCatcher = page.WaitForResponseAsync(r => r.Request.Url.Contains("LoginProcess") ||
                                                           r.Request.Url.Contains("LoginOtpAuth") && r.Request.Method == "POST");
        
        // Check if Captcha is requested
        if (step == "loginScript")
        {
            // The following script checks for captcha ('.layer_launcher.inner_layer.active[data-type="alert"]')
            var checkForCaptchaError = @"
                () => {
                    var query = document.querySelector('.layer_launcher.inner_layer.active[data-type=""alert""]');
                    var result = null;
                    if(query != null)
                        result = query.innerText;
                    return result;
                }";

            var captchaDetected = await page.EvaluateAsync<string>(checkForCaptchaError);

            if (!string.IsNullOrEmpty(captchaDetected))
            {
                var msgBox = MsgBoxManager.GetMessageBox("Captcha Detected", "Please complete the captcha verification.", true);
                await msgBox.ShowAsync();
                //await page.ClickAsync(".btnDone.btn.btn_blue.btn_mid"); //captcha alert pop up confirm btn, doesn't do anything
                await page.ClickAsync("#btnLogin");
            }
        }
        
        // Check for Login process response
        var xhrResponse = await errorCatcher;

        // Check if there are any errors from login process
        var xhrPayload = await xhrResponse.JsonAsync();
        if (xhrPayload.Value.ValueKind == JsonValueKind.Object)
        {
            bool isJoinProcLogin = xhrPayload.Value.GetProperty("_isJoinProcLogin").GetBoolean();
            string _failReturnUrl = xhrPayload.Value.GetProperty("_failReturnUrl").GetString();
            int resultCode = xhrPayload.Value.GetProperty("resultCode").GetInt32();
            string resultMsg = xhrPayload.Value.GetProperty("resultMsg").GetString();
            string returnUrl = xhrPayload.Value.GetProperty("returnUrl").GetString();
            string resultData = xhrPayload.Value.GetProperty("resultData").GetString();
            bool isAutoLoginSuccess = xhrPayload.Value.GetProperty("isAutoLoginSuccess").GetBoolean();
            
            // No error found, returning null
            // resultCode is 0 if successful and account does not have OTP
            // resultCode is -1 if successful and account have OTP
            // resultCode is -2 if successful with OTP
            if (resultCode >= -2 && resultCode <= 0)
                return null;
            else
                return $"Step: {step} returned an error code {resultCode}: {resultMsg}";
        }
        else
        {
            return $"Step: {step} returned result -> The JSON is not an object.";
        }
    }
    
    private async Task<string> AdditionalErrorsCheck(IPage page, string step)
    {
        // The following script checks for maintenance (.box_error) and
        // password change notification (.container.error.closetime)
        var additionalErrorsCheckScript = @"
                (function(){
                    var query1 = document.querySelector('.box_error');
                    var query2 = document.querySelector('.container.error.closetime');
                    var result = null;
                    if(query1 != null)
                        result = query1.innerText;
                    else if(query2 != null)
                        result = query2.innerText;
                    return result;
                })()";
        
        string errorMsg = null;
        try
        {
            errorMsg = await page.EvaluateAsync<string>(additionalErrorsCheckScript);
        }
        catch (Exception e)
        {
            MsgBoxManager.GetMessageBox($"{step} Fail", e.Message);
            throw;
        }
        return !string.IsNullOrEmpty(errorMsg) ? errorMsg : null;
    }
}