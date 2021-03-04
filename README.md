# Custom Online Game Launcher

Based on/Forked from https://github.com/bdoscientist/Launcher

A custom game launcher made to replace the official game launcher.

## Build Manually
#### Visual Studio
Grab Newtonsoft.Json version 11.0.1 Package from NuGet (Requires NuGet 2.12 or higher)

Choose Build Solution from the Build menu. The Output window shows the results of the build process.

## Features
1. Stripped any unnecessary connections/assets that are not needed to start the game
2. Support for one-time password (OTP)
3. credential Saving (username, password, OTP)
4. Password and OTP are encrypted with PBKDF2 (rfc2898)
5. Automatic Login
6. Removed the need for Admin Privilege to start the game

## How to use the OTP feature (version 1.0.5+)
### ***DO NOT STORAGE YOUR MASTER PASSWORD ANYWHERE _DIGITALLY_, YOU HAVE BEEN WARNED***
1. Grab the Master Password when you sign up for OTP refer to the image below, **this should be the only time you ever see your master password**
![Screenshot](https://user-images.githubusercontent.com/42134925/110019971-e2d29400-7cdd-11eb-937e-e8ec6cd23dbb.png)
2. Finish signing up for the OTP feature normally

### How to use the OTP feature (version 1.0.3 & 1.0.4)
Version 1.0.3 & 1.0.4 will prompt the user to enter the OTP upon login