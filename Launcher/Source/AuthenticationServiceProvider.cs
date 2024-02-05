using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Launcher.Source;

public class AuthenticationServiceProvider
{
    private const string LauncherReturnUrl = "https://launcher.naeu.playblackdesert.com/Login/Index";
    private const string AuthenticationEndPoint = "https://launcher.naeu.playblackdesert.com/Default/AuthenticateAccount";
    
    public async Task<string> AuthenticateAsync(IBrowserContext browser, string username, string password,
        string region,
        bool useOtp, bool useMasterOtp, string otp)
    {
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
                if (route.Request.Url.StartsWith("https://launcher.naeu.playblackdesert.com") ||
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

            var loginScript = $@"
                document.querySelector('#_email').value = '{username}';
                document.querySelector('#_password').value = '{password}';
                document.querySelector('#isIpCheck').value = 'false';
                document.querySelector('#btnLogin').click();";

            errorMsg = await CheckErrorMsg(page, loginScript, "loginScript");
            if (!string.IsNullOrEmpty(errorMsg))
                return errorMsg;

            if (useOtp)
            {
                Otp _otp = new Otp();
                string otpString = null;
                // if OTP input is not null or empty then use it instead of master OTP
                if (useMasterOtp)
                {
                    _otp.Password = Base32Converter.ToBytes(otp);
                    otpString = _otp.OneTimePassword.ToString("D6");
                }
                else
                {
                    otpString = otp;
                }
                
                // Wait for the OTP page to load first
                await page.WaitForSelectorAsync(".input_otp_wrap.js-inputNumWrap");

                var otpScript = $@"
                document.querySelector('#otpInput1').value = '{otpString[0]}';
                document.querySelector('#otpInput2').value = '{otpString[1]}';
                document.querySelector('#otpInput3').value = '{otpString[2]}';
                document.querySelector('#otpInput4').value = '{otpString[3]}';
                document.querySelector('#otpInput5').value = '{otpString[4]}';
                document.querySelector('#otpInput6').value = '{otpString[5]}';
                document.querySelector('.btn.btn_big.btn_blue.btnCheckOtp').click();";

                errorMsg = await CheckErrorMsg(page, otpScript, "otpScript");
                if (!string.IsNullOrEmpty(errorMsg))
                    return errorMsg;
            }

            // Check for password change notification
            errorMsg = await AdditionalErrorsCheck(page, "password change");
            if (!string.IsNullOrEmpty(errorMsg))
                return errorMsg;
            
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

    private async Task<string> CheckErrorMsg(IPage page, string javascript, string step)
    {
        // This needs to be before login btn is clicked, it will get the response of login process
        var errorCatcher = page.WaitForResponseAsync(r => r.Request.Url.Contains("LoginProcess") ||
                                                           r.Request.Url.Contains("LoginOtpAuth") && r.Request.Method == "POST");
        
        // Check if Captcha is requested
        var checkForCaptchaError = @"
                (function(){
                    var query = document.querySelector('.recaptcha_wrap');
                    var result = false;
                    if(query != null)
                        result = true;
                    return result;
                })()";
        
        var errorDetected = await page.EvaluateAsync<bool>(checkForCaptchaError);

        if (errorDetected)
        {
            var msgBox = MsgBoxManager.GetMessageBox("Captcha Detected", "Please complete the captcha verification.", true);
            await msgBox.ShowAsync();
        }
        
        // Fill email and password textboxes and uncheck the IP checkbox
        await page.EvaluateAsync(javascript);
        
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
                    var query = document.querySelector('.box_error');
                    var query2 = document.querySelector('.container.error.closetime');
                    var result = null;
                    if(query != null)
                        result = query.innerText;
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