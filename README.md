# Custom Online Game Launcher
The generate of One-Time Password (OTP) implementation is based on https://github.com/LTruijens/google-auth-csharp
This Launcher is Based on/Forked https://github.com/bdoscientist/Launcher

<p align="center">
  <img width="363" height="237" src="https://user-images.githubusercontent.com/42134925/112936280-5b105780-90da-11eb-868d-d9a193c62f57.png">
</p>

- A custom game launcher (NA/EU, non-steam) made to replace the official game launcher. 
- Supports automatic login even if the account has set up for OTP system (The password and OTP are both stored locally and encrypted)
- Enjoy the 5% drop rate buff without the annoyance of having to input OTP every single time on log in and having to painfully click the Confirm button because you can't just copy/paste and hit the enter key

## Build Manually
1. Visual Studio
2. Grab Newtonsoft.Json version 11.0.1 Package from NuGet (Requires NuGet 2.12 or higher)
3. Choose Build Solution from the Build menu. The Output window shows the results of the build process.

## Features
- Stripped any unnecessary connections/assets that are not needed to start the game
- Support for one-time password (OTP)
- Credential Saving (username, password, OTP)
- Password and OTP are encrypted with PBKDF2 (see rfc2898)
- Automatic Login
- Removed the need for Admin Privilege to start the game
- (OPTIONAL) To disable the automatic-login function, either edit the settings file (%AppData%\bdoscientist_Launcher\settings.json) or start the Launcher with the command-line argument "--disable-automatic-login"

## How to use the OTP feature (2 ways to login if you have OTP)
#### Option A: Automatic | ***DO NOT STORAGE YOUR MASTER PASSWORD ANYWHERE _DIGITALLY_, YOU HAVE BEEN WARNED***
<p align="center">
  <img width="880" height="272" src="https://user-images.githubusercontent.com/42134925/110019971-e2d29400-7cdd-11eb-937e-e8ec6cd23dbb.png">
</p>

1. Grab the Master Password when sign up for OTP refer to the image above
2. **this should be the only time you ever see your master password**
3. Finish signing up for the OTP feature normally
Using automatic feature does not mean you don't need OTP, weekly updates must be done through official launcher

#### Option B: Manual

1. Check the OTP box and leave the text field empty
2. The launcher will prompt the user to enter a one-time password
3. Hit the ENTER key or click Login

## FAQ
*Why was this created?*
Since the official launcher takes a very long time to start and sometimes won't load at all. This custom launcher solves that problem.
This launcher also completely removes the need of having to manually input password or OTP every single time when trying to login.

*DO I still need the official launcher?*
Yes, the official launcher is still required for the weekly game update, this has to be done manually via the official launcher.

*So how does it work exactly?*
The launcher will fetch a handshake from PA (https://launcher.naeu.playblackdesert.com/Login/Index) then sends the necessary credentials (email and password) to PA's authentication server-endpoint (https://account.pearlabyss.com/en-US/Launcher/Login/LoginProcess), in return, gets an authentication token. This authentication token is then send to PA's second authentication server-endpoint (https://launcher.naeu.playblackdesert.com/Default/AuthenticateAccount) which generates a play token. The launcher then starts the game by creating a process (BlackDesertEAC.exe) with the play token as startup command-line argument.

*I get an error message/launcher doesn't work at all!*
Make sure .NET Framework 4.7 is installed.

