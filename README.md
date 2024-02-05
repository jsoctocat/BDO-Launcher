# Custom Online Game Launcher
## Credits
- The generate of One-Time Password (OTP) implementation is based on https://github.com/LTruijens/google-auth-csharp
- This Launcher is Based on https://github.com/bdoscientist/Launcher

## Description
<p align="center">
  <img src="https://github.com/jsoctocat/BDO-Launcher/assets/42134925/73fe4d0e-9256-44ce-80da-9a193535cf4f">
</p>

- A custom game launcher (NA/EU, non-steam) made to replace the official game launcher. 
- Cross platform support (Windows and Linux, macOS not tested)
- Supports automatic login even if the account has set up for OTP system (The password and OTP are both stored locally and encrypted)
- Enjoy the 5% drop rate buff without the annoyance of having to input OTP every single time on log in

## Build Manually
1. Visual Studio
2. Grab .NET 8.0 SDK
2. Grab Newtonsoft.Json version 13.0.3 Package from NuGet
3. Grab Avalonia version 11.0.6 Package from NuGet
3. Grab CommunityToolkit.Mvvm version 8.2.2 Package from NuGet
3. Grab Microsoft.Playwright version 1.41.2 Package from NuGet
4. Choose Build Solution from the Build menu. The Output window shows the results of the build process.

## Features
- Stripped any unnecessary connections/assets that are not needed to start the game
- Support for one-time password (OTP)
- Credential Saving (username, password, OTP)
- Password and OTP are encrypted with PBKDF2 (see [rfc2898](https://tools.ietf.org/html/rfc2898) OR [rfc2898(wikipedia)](https://en.wikipedia.org/wiki/PBKDF2))
- ~~Support for PC Registration Service~~ Pearl Abyss removed the Register PC function on [January 31, 2024](https://www.naeu.playblackdesert.com/en-US/News/Detail?groupContentNo=6545&countryType=en-US#Web). It is enabled by default, so the transmission of the MAC address is now a mandatory feature.
- Removed the need for Admin Privilege to start the game
- CPU core affinity tweak (see [BDO Ultimate Performance Guide, "CPU Performance - Set Affinity"](https://docs.google.com/document/d/1cyLaDiPL_B6nOZw_qPE_wOGuoeRT-qddTjevTFoFBkg))
- (OPTIONAL) To disable the automatic-login function, either edit the settings file (%AppData%\bdoscientist_Launcher\settings.json) or start the Launcher with the command-line argument "--disable-automatic-login"

## How to use the OTP feature (2 ways to login if you have OTP)
#### Option A: Automatic | ***DO NOT STORAGE YOUR MASTER PASSWORD ANYWHERE _DIGITALLY_, YOU HAVE BEEN WARNED***
<p align="center">
  <img width="880" height="272" src="https://user-images.githubusercontent.com/42134925/110019971-e2d29400-7cdd-11eb-937e-e8ec6cd23dbb.png">
</p>

1. Grab the Master Password when sign up for OTP refer to the image above
2. **this should be the only time you ever see your master password**
3. Finish signing up for the OTP feature normally
4. Using the automatic feature does not mean you don't need OTP, weekly updates must be done through the official launcher

#### Option B: Manual
1. Check the OTP checkbox and leave the text field empty
2. The launcher will prompt the user to enter a one-time password
3. Hit the ENTER key or click Login

## I encountered a captcha. What should I do now?
1. Do not close the error pop up
2. Check if debug mode is on, if off then turn on debug mode then restart the launcher
3. If debug mode is on, complete the captcha from the browser pop up
4. Close the error pop up, everything else should be automatic

## Linux Setup
1. By default the following launch options are used to start the game on Linux, if your paths are different you have to edit the Launch Option
2. STEAM_COMPAT_CLIENT_INSTALL_PATH=~/.local/share/Steam/ STEAM_COMPAT_DATA_PATH=~/.local/share/Steam/steamapps/compatdata/ nohup ~/.local/share/Steam/compatibilitytools.d/GE-Proton8-30/proton run
2. If you need any additional launch options put them before nohup such as ../tdata/ DXVK_HUD=1 DRI_PRIME=1 MESA_VK_DEVICE_SELECT=1 nohup
3. CPU core affinity tweak is not supported on Linux
- NOTE: "nohup" must be included or the game will close with the launcher at start up, the above paths are default steam install locations, and is using GE-Proton8-30, **possible failure at launch if directories have white spaces**

## FAQ
### *Why was this created?*

Since the official launcher takes a very long time to start and sometimes won't load at all. This custom launcher solves that problem.
This launcher also completely removes the need of having to manually input password or OTP every single time when trying to login.

### *Do I still need the official launcher?*

Yes, the official launcher is still required for the weekly game update, this has to be done manually via the official launcher. However, you do not need to login.

### *So how does it work exactly?*

The launcher will fetch a handshake from PA (https://launcher.naeu.playblackdesert.com/Login/Index) then sends the necessary credentials (email and password) to PA's authentication server-endpoint (https://account.pearlabyss.com/en-US/Launcher/Login/LoginProcess), in return, gets an authentication token. This authentication token is then sent to PA's second authentication server-endpoint (https://launcher.naeu.playblackdesert.com/Default/AuthenticateAccount) which generates a play token. The launcher then starts the game by creating a process (BlackDesert64.exe/BlackDesert32.exe) with the play token as a startup command-line argument.

### *I get an error message/launcher doesn't work at all!*

If you are using version older than 2.0.0. Make sure [.NET Framework 4.7](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net47) and [MSVC runtime libraries](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170) are installed.

If you are on version 2.0.0 or newer. Make sure [.NET Runtime 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) is installed.

## If you are facing an issue, feel free to create an issue [here](https://github.com/jsoctocat/BDO-Launcher/issues), please describe the issue in as much detail as possible and/or paste/screenshot the error(s).
