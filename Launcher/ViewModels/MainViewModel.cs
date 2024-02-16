using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Launcher.Source;
using Launcher.Views;
using Microsoft.Playwright;
using Newtonsoft.Json.Linq;

namespace Launcher.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private IBrowserContext _browser;
    private Configuration _configuration;
    private Window _otpWindow;
    private const string _version = "2.0.2";

    public MainViewModel()
    {
        MainWindow.Instance.Loaded += MainForm_Load;
        MainWindow.Instance.Closing += MainForm_FormClosing;
    }
    
    [ObservableProperty] private string _usernameTextBox;
    [ObservableProperty] private string _passwordTextBox;
    [ObservableProperty] private string _otpTextBox;
    [ObservableProperty] private string _launchOptTextBox;
    [ObservableProperty] private string _affinityBitmaskTextBox;
    [ObservableProperty] private bool _otpCheckBox;
    [ObservableProperty] private int _regionComboBox;
    [ObservableProperty] private bool _launchOptCheckBox;
    [ObservableProperty] private bool _coreAffinityCheckBox;
    [ObservableProperty] private bool _gameMode32BitCheckBox;
    [ObservableProperty] private bool _rememberDataCheckBox;
    [ObservableProperty] private bool _loginAutomaticallyCheckBox;
    [ObservableProperty] private bool _launcherUpdateCheckBox;
    [ObservableProperty] private bool _gameUpdateCheckBox;
    [ObservableProperty] private bool _adminCheckBox;
    [ObservableProperty] private bool _debugModeCheckBox;
    [ObservableProperty] private string _gamePathLabel;
    [ObservableProperty] private bool _enableStartGameBtn;
    [ObservableProperty] private string _otpNotMasterTextBox;
    
    private async void MainForm_Load(object? sender, RoutedEventArgs e)
    {
        _configuration = ConfigurationManager.Load();

        if (_configuration == null)
        {
            _configuration = new Configuration();

            ConfigurationManager.Save(_configuration);
        }

        if (_configuration.LoginAutomatically && (Environment.GetCommandLineArgs().Length >= 2) && (Environment.GetCommandLineArgs()[1].ToLower() == "--disable-automatic-login"))
            _configuration.LoginAutomatically = false;

        if(await CheckGameDirectoryPathAndPrompt())
            GamePathLabel = $"{_configuration.GameDirectoryPath}";

        if (_configuration.RememberData)
        {
            UsernameTextBox = _configuration.Username;
            PasswordTextBox = _configuration.GetPassword();
            OtpTextBox = _configuration.GetOtp();
            AffinityBitmaskTextBox = _configuration.AffinityBitmask;
            LaunchOptTextBox = _configuration.LaunchOptions;
            
            OtpCheckBox = _configuration.Otp;
            RegionComboBox = _configuration.RegionComboBox;
            LaunchOptCheckBox = _configuration.LaunchOption;
            CoreAffinityCheckBox = _configuration.CoreAffinity;
            GameMode32BitCheckBox = _configuration.GameMode32Bit;
            RememberDataCheckBox = _configuration.RememberData;
            LoginAutomaticallyCheckBox = _configuration.LoginAutomatically;
            LauncherUpdateCheckBox = _configuration.LauncherUpdate;
            GameUpdateCheckBox = _configuration.GameUpdate;
            AdminCheckBox = _configuration.RunAsAdmin;
            DebugModeCheckBox = _configuration.DebugMode;
        }
        
        var basePath = AppContext.BaseDirectory;
        var cachePath = $"{basePath}\\Cache";
        
        // browser execution configs
        var browserLaunchOptions = new BrowserTypeLaunchPersistentContextOptions()
        {
            Headless = !DebugModeCheckBox, // set false to show browser, mostly for debugging purpose
            UserAgent = "BLACKDESERT",
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            browserLaunchOptions.ExecutablePath = basePath + "/.playwright/firefox-1438/firefox/firefox";
        else
            browserLaunchOptions.ExecutablePath = basePath + "\\.playwright\\firefox-1438\\firefox\\firefox.exe";

        if (!File.Exists(browserLaunchOptions.ExecutablePath))
        {
            var msgBox = MsgBoxManager.GetMessageBox("Error", "Playwright firefox 1438 is missing", true);
            await msgBox.ShowAsync();
        }
        
        // create a single browser
        var playwright = await Playwright.CreateAsync();
        _browser = await playwright.Firefox.LaunchPersistentContextAsync(cachePath, browserLaunchOptions);
        EnableStartGameBtn = true;

        // Check for new version on start up
        if (_configuration.LauncherUpdate || _configuration.GameUpdate)
        {
            await CheckVersion(_configuration.LauncherUpdate, _configuration.GameUpdate);
        }

        if (_configuration.LoginAutomatically)
        {
            GameStart();
        }
    }
    
    private void MainForm_FormClosing(object? sender, WindowClosingEventArgs e)
    {
        //clean up (shuts down browser, close all windows)
        if(_otpWindow != null)
            _otpWindow.Close();
        
        if (RememberDataCheckBox)
        {
            _configuration.Username = UsernameTextBox;
            _configuration.SetPassword(PasswordTextBox);
            _configuration.LaunchOptions = LaunchOptTextBox;
            _configuration.AffinityBitmask = AffinityBitmaskTextBox;
            _configuration.Otp = OtpCheckBox;
            _configuration.RegionComboBox = RegionComboBox;
            _configuration.LaunchOption = LaunchOptCheckBox;
            _configuration.CoreAffinity = CoreAffinityCheckBox;
            _configuration.GameMode32Bit = GameMode32BitCheckBox;
            _configuration.RememberData = RememberDataCheckBox;
            _configuration.LoginAutomatically = LoginAutomaticallyCheckBox;
            _configuration.LauncherUpdate = LauncherUpdateCheckBox;
            _configuration.GameUpdate = GameUpdateCheckBox;
            _configuration.RunAsAdmin = AdminCheckBox;
            _configuration.DebugMode = DebugModeCheckBox;
        }
        // Always save OTP
        _configuration.SetOtp(OtpTextBox);
        
        ConfigurationManager.Save(_configuration);
    }

    partial void OnRegionComboBoxChanged(int value)
    {
        string[] region = { "NA", "EU" };
        string[] regionInfo =
        {
            $"[SERVICE]\nTYPE=NA\nRES=_EN_\nnationType=0\n\n[NA]\nAUTHENTIC_DOMAIN=gameauth.na.playblackdesert.com\nAUTHENTIC_PORT=8888\nPATCH_URL=http://naeu-o-dn.playblackdesert.com/UploadData/\nviewTradeMarketUrl=https://na-trade.naeu.playblackdesert.com/\ngameTradeMarketUrl=https://na-game-trade.naeu.playblackdesert.com/",
            $"[SERVICE]\nTYPE=NA\nRES=_EN_\nnationType=1\n\n[NA]\nAUTHENTIC_DOMAIN=gameauth.eu.playblackdesert.com\nAUTHENTIC_PORT=8888\nPATCH_URL=http://naeu-o-dn.playblackdesert.com/UploadData/\nviewTradeMarketUrl=https://eu-trade.naeu.playblackdesert.com/\ngameTradeMarketUrl=https://eu-game-trade.naeu.playblackdesert.com/"
        };
            
        var regionFilePath = Path.Combine(_configuration.GameDirectoryPath, "region");

        if (!File.Exists(regionFilePath))
        {
            File.WriteAllText(regionFilePath, region[value]);
            File.WriteAllText(Path.Combine(_configuration.GameDirectoryPath, "service.ini"),
                regionInfo[value]);
            return;
        }
            
        var currentRegion = File.ReadAllText(regionFilePath);

        if (currentRegion == region[value]) return;
                
        File.WriteAllText(regionFilePath, region[value]);
        File.WriteAllText(Path.Combine(_configuration.GameDirectoryPath, "service.ini"),
            regionInfo[value]);
    }

    partial void OnLoginAutomaticallyCheckBoxChanged(bool value)
    {
        if (!RememberDataCheckBox && value) RememberDataCheckBox = true;
    }

    // This has async because await TestPuppeteer is commented out for testing purpose
    [RelayCommand]
    private async Task StartGameBtn()
    {
        GameStart();
        //await TestPuppeteer();
    }
    
    // Function is not used on release
    private async Task TestPuppeteer()
    {
        // https://pixeljets.com/blog/puppeteer-click-get-xhr-response/
        // http://ip.jsontest.com/
        // https://api.github.com/repos/jsoctocat/BDO-Launcher/tags

        var page = await _browser.NewPageAsync();
        await page.GotoAsync("https://apiroad.net/ajax-test.html");
        await page.ContentAsync();
    
        await page.WaitForSelectorAsync("form > input[type=text]");
        await page.FillAsync("form > input[type=text]", "toyota");
    
        //Request.Method is HTTP request methods
        var xhrCatcher = page.WaitForResponseAsync(r => r.Request.Url.Contains("sample-search.php") && r.Request.Method != "OPTIONS");
    
        await page.ClickAsync("#search-button");
    
        var xhrResponse = await xhrCatcher;

        // now get the JSON payload
        var xhrPayload = await xhrResponse.JsonAsync();
        Console.WriteLine(xhrPayload);
    }
    
    [RelayCommand]
    private async Task SetGamePathBtn()
    {
        string newGameDirectoryPath = await SelectGameDirectoryPath();

        if (!string.IsNullOrEmpty(newGameDirectoryPath))
        {
            _configuration.GameDirectoryPath = newGameDirectoryPath;

            ConfigurationManager.Save(_configuration);

            GamePathLabel = $"{_configuration.GameDirectoryPath}";
        }
    }
    
    [RelayCommand]
    private void ReportIssueBtn()
    {
        Process.Start(new ProcessStartInfo("https://github.com/jsoctocat/BDO-Launcher/issues") { UseShellExecute = true });
    }
    
    private async void GameStart()
    {
        EnableStartGameBtn = false;
            
        if (OtpCheckBox && string.IsNullOrEmpty(OtpTextBox))
            OneTimePasswordAsync();
        else if (await StartGameAsync(true, String.Empty))
        {
            MainWindow.Instance.Close();
            Environment.Exit(0);
        }
        else
            EnableStartGameBtn = true;
    }
    
    [RelayCommand]
    public async Task OtpLoginBtn()
    {
        EnableStartGameBtn = true;
        if (OtpNotMasterTextBox.Length != 6 || !OtpNotMasterTextBox.All(char.IsDigit))
        {
            EnableStartGameBtn = false;
            var msg = MsgBoxManager.GetMessageBox("Error", "Please enter a valid OTP.", true);
            await msg.ShowAsync();
        }
        else if(await StartGameAsync(false, OtpNotMasterTextBox))
        {
            MainWindow.Instance.Close();
            Environment.Exit(0);
        }
        else
        {
            EnableStartGameBtn = false;
        }
    }
    
    private void OneTimePasswordAsync()
    {
        _otpWindow = new SimpleInputBoxWindow()
        {
            Content = new SimpleInputBoxView(),
            DataContext = this
        };

        _otpWindow.Closing += Otp_FormClosing;

        _otpWindow.Show();

        // Put focus on textbox when otp window pops up
        foreach (var tBox in _otpWindow.GetVisualDescendants().OfType<TextBox>())
        {
            tBox.Focus();
        }
    }

    private void Otp_FormClosing(object? sender, WindowClosingEventArgs e)
    {
        EnableStartGameBtn = true;
    }
    
    private async Task<bool> StartGameAsync(bool useMasterOtp, string otpNotMaster)
    {
        var gameExecutableFilePath = GameMode32BitCheckBox ? 
            Path.Combine("bin","BlackDesert32.exe") : Path.Combine("bin64", "BlackDesert64.exe");
        
        var launchPath = Path.Combine(_configuration.GameDirectoryPath, gameExecutableFilePath);

        if (!File.Exists(launchPath))
        {
            var msgBox =  MsgBoxManager.GetMessageBox("Error",
                $"Failed to find `{launchPath}`.\nUsed path: `{_configuration.GameDirectoryPath}`.\nPlease set the correct path to the game's base directory where the bin/bin64 folder reside.",
                true);
                    
            await msgBox.ShowAsync();

            return false;
        }

        if (string.IsNullOrEmpty(UsernameTextBox) || string.IsNullOrEmpty(PasswordTextBox))
        {
            var msgBox =  MsgBoxManager.GetMessageBox("Error",
                "Please enter the valid credential(s).",
                true);
                    
            await msgBox.ShowAsync();

            return false;
        }

        var authenticationServiceProvider = new AuthenticationServiceProvider();
        string otp = useMasterOtp ? OtpTextBox : otpNotMaster;
        
        string[] gameRegion = ["NA", "EU"];
        var playToken = await authenticationServiceProvider.AuthenticateAsync(
            _browser,
            UsernameTextBox, 
            PasswordTextBox, 
            gameRegion[RegionComboBox],
            OtpCheckBox,
            useMasterOtp,
            otp);
        
        if (!playToken.StartsWith("0x"))
        {
            if (playToken.Contains("Change Password"))
            {
                var msgBox =  MsgBoxManager.GetMessageBox("Password Too Old Error",
                    $"Your password is too old, please login using the official launcher to change your password\n\nThis error is from the game server, it will come up every 3 months",
                    true, false, false,
                    () =>
                    {
                        MainWindow.Instance.Close();
                        Environment.Exit(0);
                    });
                    
                await msgBox.ShowAsync();
            }
            else
            {
                //might need fix, originally return only after ok button is clicked
                var msgBox =  MsgBoxManager.GetMessageBox("Authentication Error",
                    $"{playToken}\n\nPlease report the error if the error isn't your username/password/otp",
                    true);
                    
                await msgBox.ShowAsync();

                return false;
            }
        }

        string affinityBitmask = "";
        if (CoreAffinityCheckBox && !string.IsNullOrEmpty(AffinityBitmaskTextBox))
            affinityBitmask = " /affinity " + AffinityBitmaskTextBox.Trim();
        
        using (var process = new Process())
        {
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(launchPath);
            
            // Ignore run as admin when on linux
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string command = String.Empty;;
                
                if (LaunchOptCheckBox)
                    command = $"{LaunchOptTextBox} '{launchPath}' {playToken}";
                else
                    command = $"STEAM_COMPAT_CLIENT_INSTALL_PATH=~/.local/share/Steam/ STEAM_COMPAT_DATA_PATH=~/.local/share/Steam/steamapps/compatdata/ nohup ~/.local/share/Steam/steamapps/common/'Proton - Experimental'/proton run '{launchPath}' {playToken}";
                
                process.StartInfo.FileName = "/bin/bash";
                process.StartInfo.Arguments = "-c \" " + command + " \"";
            }
            else if (!AdminCheckBox) // RunAsInvoker
            {
                string command = $"{affinityBitmask} \"\" \"{launchPath}\" {playToken} ";

                if (LaunchOptCheckBox)
                    command += _launchOptTextBox;
                
                process.StartInfo.FileName = "CMD";
                process.StartInfo.Arguments = "/min /C set __COMPAT_LAYER=RUNASINVOKER && start" + command;
            }
            else // RunAsAdmin
            {
                process.StartInfo.Verb = "runas";
                process.StartInfo.FileName = launchPath;
                process.StartInfo.Arguments = playToken;
            }
            
            process.Start();
        }

        return true;
    }
    
    private async Task<string> InnerText(IPage response)
    {
        string value;
        try
        {
            value = await response.EvaluateAsync<string>(
                "() => { return document.querySelector(\"body\").innerText; }");
        }
        catch (Exception e)
        {
            var msgBox = MsgBoxManager.GetMessageBox("ValidateJson Fail", e.Message);
            await msgBox.ShowAsync();
            throw;
        }
        return value;
    }
    
    private async Task CheckVersion(bool launcherUpdate, bool gameUpdate)
    {
        string launcherVersionUrl = "https://api.github.com/repos/jsoctocat/BDO-Launcher/tags";

        string gameVersionUrl = "https://naeu-o-dn.playblackdesert.com/UploadData/client_version";
        
        // Check for launcher update
        if (launcherUpdate)
        {
            var page = await _browser.NewPageAsync();
            await page.GotoAsync(launcherVersionUrl); 
            var innerText = await InnerText(page);
            var json = JArray.Parse(innerText);
            var name = json[0]?["name"];
            
            if (name != null && name.Value<string>() != _version)
            {
                var msgBox =  MsgBoxManager.GetMessageBox("Custom Launcher Update Notice",
                    "New version is available for this launcher, would you like to update?",
                    false, true, true,
                    null,
                    () => { Process.Start(new ProcessStartInfo("https://github.com/jsoctocat/BDO-Launcher/releases")
                        { UseShellExecute = true }); });
                
                await msgBox.ShowAsync();
            }
        }

        // Check for game update
        if (gameUpdate)
        {
            var page = await _browser.NewPageAsync();
            await page.GotoAsync(gameVersionUrl);
            var innerText = await InnerText(page);
            string[] versions = innerText.Split('\n');
            
            var metaFilePath = Path.Combine(_configuration.GameDirectoryPath, "Paz", "pad00000.meta");
            FileStream metaFile = new FileStream(metaFilePath, FileMode.Open);
            
            var clientVersionBytes = 4;
            var buffer = new byte[clientVersionBytes];
            metaFile.Read(buffer, 0, clientVersionBytes);
            var clientVersion = BitConverter.ToInt32(buffer, 0);
            
            if (clientVersion < int.Parse(versions[0]))
            {
                var msgBox =  MsgBoxManager.GetMessageBox("Game Update Notice",
                    "Game version is lower than required to start\nWould you like to start the official launcher?",
                    false, true, true,
                    null,
                    () => { 
                        var gameExecutableFilePath = Path.Combine(_configuration.GameDirectoryPath, "BlackDesertLauncher.exe");
                        Process.Start(new ProcessStartInfo(gameExecutableFilePath)
                        { UseShellExecute = true }); });
                
                await msgBox.ShowAsync();
            }
            
        }
    }

    private async Task<bool> CheckGameDirectoryPathAndPrompt()
    {
        string messageBoxText = String.Empty;
        
        if (String.IsNullOrEmpty(_configuration.GameDirectoryPath))
            messageBoxText = "The path to the game is not set.\nDo you want to set it now?";
        else if (!Directory.Exists(_configuration.GameDirectoryPath) || !File.Exists(Path.Combine(_configuration.GameDirectoryPath, "BlackDesertLauncher.exe")))
            messageBoxText = "The path to the game is invalid.\nDo you want to set it now?";
        else
            return true;
        
        var msgBox =  MsgBoxManager.GetMessageBox("Current Game Directory Stored: " + _configuration.GameDirectoryPath,
            messageBoxText,
            false, true, true,
            null,
            async () => { 
                string newGameDirectoryPath = await SelectGameDirectoryPath();

                if (!string.IsNullOrEmpty(newGameDirectoryPath))
                {
                    _configuration.GameDirectoryPath = newGameDirectoryPath;

                    ConfigurationManager.Save(_configuration);

                    GamePathLabel = $"{_configuration.GameDirectoryPath}";
                } });
                    
        await msgBox.ShowAsync();

        return false;
    }

    private async Task<string> SelectGameDirectoryPath()
    {
        var dialog = MainWindow.Instance.StorageProvider;
        FolderPickerOpenOptions openOptions = new FolderPickerOpenOptions
        {
            AllowMultiple = false
        };
        var result = await dialog.OpenFolderPickerAsync(openOptions);
        if (result.Count == 0) return string.Empty;
        return result[0].Path.LocalPath;
    }
}
