﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Launcher.Source;

namespace Launcher.Forms
{
    
    public partial class MainForm : Form
    {
        private Configuration _configuration;
        private const string Version = "1.1.6";
        private const string Title = "Custom Black Desert Launcher (" + Version + ")";
        
        public MainForm()
        {
            InitializeComponent();
            Text = Title;
        }

        private async Task CheckVersion(bool launcherUpdate, bool gameUpdate)
        {
            Uri launcherVersionUrl = 
                new Uri("https://gist.githubusercontent.com/jsoctocat/4aeb78c8b7d92aca96911afa393614d5/raw/version");

            Uri gameVersionUrl = new Uri("https://naeu-o-dn.playblackdesert.com/UploadData/client_version");
            
            // Check version on load
            if (launcherUpdate)
            {            
                using (var handler = new HttpClientHandler { CookieContainer = new CookieContainer() })
                using (var client = new HttpClient(handler) { BaseAddress = launcherVersionUrl })
                {
                    using (var result = await client.GetAsync(launcherVersionUrl))
                    {
                        if (!result.IsSuccessStatusCode)
                            return;
                        
                        // Grab latest version string from github
                        var resultContent = await result.Content.ReadAsStringAsync();

                        // Check for launcher update
                        if (Version != resultContent)
                        {
                            if (MessageBox.Show(
                                "New version is available for this launcher, would you like to update?",
                                "Custom Launcher Update Notice",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Asterisk
                            ) == DialogResult.Yes)
                            {
                                Process.Start("https://github.com/jsoctocat/BDO-Launcher/releases");
                                Close();
                            }
                        }
                    }
                }
            }

            if (gameUpdate)
            {
                using (var handler = new HttpClientHandler { CookieContainer = new CookieContainer() })
                using (var client = new HttpClient(handler) { BaseAddress = gameVersionUrl })
                {
                    using (var result = await client.GetAsync(gameVersionUrl))
                    {
                        if (!result.IsSuccessStatusCode)
                            return;
                        
                        var resultContent = await result.Content.ReadAsStringAsync();
                        string[] versions = resultContent.Split('\n');
                        
                        var metaFilePath = Path.Combine(_configuration.GameDirectoryPath, "Paz", "pad00000.meta");
                        FileStream metaFile = new FileStream(metaFilePath, FileMode.Open);
                        
                        var clientVersionBytes = 4;
                        var buffer = new byte[clientVersionBytes];
                        metaFile.Read(buffer, 0, clientVersionBytes);
                        var clientVersion = BitConverter.ToInt32(buffer, 0);

                        if (clientVersion < int.Parse(versions[0]))
                        {
                            if (MessageBox.Show(
                                "Game version is lower than required to start\nWould you like to start the official launcher?",
                                "Game Update Notice",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Asterisk
                            ) == DialogResult.Yes)
                            {
                                var gameExecutableFilePath = Path.Combine(_configuration.GameDirectoryPath, "BlackDesertLauncher.exe");
                                Process.Start(gameExecutableFilePath);
                                Close();
                            }
                        }
                    }
                }
            }

        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            _configuration = ConfigurationManager.Load();

            if (_configuration == null)
            {
                _configuration = new Configuration();

                ConfigurationManager.Save(_configuration);
            }

            if (_configuration.LoginAutomatically && (Environment.GetCommandLineArgs().Length >= 2) && (Environment.GetCommandLineArgs()[1].ToLower() == "--disable-automatic-login"))
                _configuration.LoginAutomatically = false;

            if (CheckGameDirectoryPathAndPrompt())
                Text = $"Launcher | {_configuration.GameDirectoryPath}";

            if (_configuration.RememberData)
            {
                UsernameTextBox.Text = _configuration.Username;
                PasswordTextBox.Text = _configuration.GetPassword();
                MacAddressTextBox.Text = _configuration.MacAddress;
            }

            OtpCheckBox.Checked = _configuration.Otp;
            OtpTextBox.Text = _configuration.GetOtp();
            RegionComboBox.SelectedIndex = _configuration.RegionComboBox;
            MacAddressCheckBox.Checked = _configuration.PcRegistration;
            GameMode32BitCheckBox.Checked = _configuration.GameMode32Bit;
            RememberDataCheckBox.Checked = _configuration.RememberData;
            LoginAutomaticallyCheckBox.Checked = _configuration.LoginAutomatically;
            launcherUpdateCheckBox.Checked = _configuration.LauncherUpdate;
            gameUpdateCheckBox.Checked = _configuration.GameUpdate;
            adminCheckBox.Checked = _configuration.RunAsAdmin;

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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (RememberDataCheckBox.Checked)
            {
                _configuration.Username = UsernameTextBox.Text;
                _configuration.SetPassword(PasswordTextBox.Text);
                _configuration.MacAddress = MacAddressTextBox.Text;
            }
            // Always save OTP
            _configuration.SetOtp(OtpTextBox.Text);

            ConfigurationManager.Save(_configuration);
        }
        
        private void OtpCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // Do not allow edit OTP if Otp is enabled
            OtpTextBox.Enabled = !OtpCheckBox.Checked;
            
            _configuration.Otp = OtpCheckBox.Checked;

            ConfigurationManager.Save(_configuration);
        }

        private void RegionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _configuration.RegionComboBox = RegionComboBox.SelectedIndex;
            ConfigurationManager.Save(_configuration);
            
            string[] region = { "NA", "EU" };
            string[] regionInfo =
            {
                $"[SERVICE]\nTYPE=NA\nRES=_EN_\nnationType=0\n\n[NA]\nAUTHENTIC_DOMAIN=gameauth.na.playblackdesert.com\nAUTHENTIC_PORT=8888\nPATCH_URL=http://naeu-o-dn.playblackdesert.com/UploadData/\nviewTradeMarketUrl=https://na-trade.naeu.playblackdesert.com/\ngameTradeMarketUrl=https://na-game-trade.naeu.playblackdesert.com/",
                $"[SERVICE]\nTYPE=NA\nRES=_EN_\nnationType=1\n\n[NA]\nAUTHENTIC_DOMAIN=gameauth.eu.playblackdesert.com\nAUTHENTIC_PORT=8888\nPATCH_URL=http://naeu-o-dn.playblackdesert.com/UploadData/\nviewTradeMarketUrl=https://eu-trade.naeu.playblackdesert.com/\ngameTradeMarketUrl=https://eu-game-trade.naeu.playblackdesert.com/"
            };
            
            var regionFilePath = Path.Combine(_configuration.GameDirectoryPath, "region");

            if (!File.Exists(regionFilePath))
            {
                File.WriteAllText(regionFilePath, RegionComboBox.SelectedItem.ToString());
                File.WriteAllText(Path.Combine(_configuration.GameDirectoryPath, "service.ini"),
                    regionInfo[RegionComboBox.SelectedIndex]);
            }
            
            var currentRegion = File.ReadAllText(regionFilePath);

            if (currentRegion == RegionComboBox.SelectedItem.ToString()) return;
                
            File.WriteAllText(regionFilePath, region[RegionComboBox.SelectedIndex]);
            File.WriteAllText(Path.Combine(_configuration.GameDirectoryPath, "service.ini"),
                regionInfo[RegionComboBox.SelectedIndex]);
        }
        
        private void MacAddressCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // Do not allow edit Mac Address if PC Registration is enabled
            MacAddressTextBox.Enabled = !MacAddressCheckBox.Checked;
            
            _configuration.PcRegistration = MacAddressCheckBox.Checked;

            ConfigurationManager.Save(_configuration);
        }

        private void GameModeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.GameMode32Bit = GameMode32BitCheckBox.Checked;

            ConfigurationManager.Save(_configuration);
        }
        
        private void RememberDataCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.RememberData = RememberDataCheckBox.Checked;

            ConfigurationManager.Save(_configuration);
        }

        private void LoginAutomaticallyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (LoginAutomaticallyCheckBox.Checked)
            {
                RememberDataCheckBox.Checked = true;
                RememberDataCheckBox.Enabled = false;
            }
            else
                RememberDataCheckBox.Enabled = true;

            _configuration.LoginAutomatically = LoginAutomaticallyCheckBox.Checked;

            ConfigurationManager.Save(_configuration);
        }

        private void launcherUpdate_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.LauncherUpdate = launcherUpdateCheckBox.Checked;

            ConfigurationManager.Save(_configuration);
        }
        
        private void gameUpdateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.GameUpdate = gameUpdateCheckBox.Checked;

            ConfigurationManager.Save(_configuration);
        }

        private void adminCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.RunAsAdmin = adminCheckBox.Checked;

            ConfigurationManager.Save(_configuration);
        }
        
        private void StartGameButton_Click(object sender, EventArgs e)
        {
            GameStart();
        }
        
        private async void GameStart()
        {
            btn_startGame.Enabled = false;
            
            if (OtpCheckBox.Checked && string.IsNullOrEmpty(OtpTextBox.Text))
                OneTimePasswordAsync();
            else if (await StartGameAsync(true, null))
            {
                Close();
                Environment.Exit(0);
            }
            else
                btn_startGame.Enabled = true;
        }

        private void GameDirectoryPathLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string newGameDirectoryPath = SelectGameDirectoryPath();

            if (newGameDirectoryPath != null)
            {
                _configuration.GameDirectoryPath = newGameDirectoryPath;

                ConfigurationManager.Save(_configuration);

                Text = $"Launcher | {_configuration.GameDirectoryPath}";
            }
        }

        private void GithubLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/jsoctocat/BDO-Launcher/issues");
        }

        private bool CheckGameDirectoryPathAndPrompt()
        {
            string messageBoxText = null;

            if (String.IsNullOrEmpty(_configuration.GameDirectoryPath))
                messageBoxText = "The path to the game is not set.\nDo you want to set it now?";
            else if (!Directory.Exists(_configuration.GameDirectoryPath) || !File.Exists(Path.Combine(_configuration.GameDirectoryPath, "BlackDesertLauncher.exe")))
                messageBoxText = "The path to the game is invalid.\nDo you want to set it now?";
            else
                return true;

            if (MessageBox.Show(messageBoxText,
                Text, MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                string newGameDirectoryPath = SelectGameDirectoryPath();

                if (newGameDirectoryPath != null)
                {
                    _configuration.GameDirectoryPath = newGameDirectoryPath;

                    ConfigurationManager.Save(_configuration);

                    Text = $"Launcher | {_configuration.GameDirectoryPath}";
                }
            }

            Activate();

            return false;
        }

        private string SelectGameDirectoryPath()
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.ShowNewFolderButton = false;

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    return folderBrowserDialog.SelectedPath;
            }

            return null;
        }

        private void OneTimePasswordAsync()
        {
            var size = new Size(200, 55);
            var otpInputBox = new Form
            {
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                ClientSize = size,
                Text = "OTP"
            };
        
            var otpNotMasterTextBox = new TextBox
            {
                Size = new Size(size.Width - 20, 25),
                Location = new Point(10, 5)
            };
        
            otpInputBox.Controls.Add(otpNotMasterTextBox);
        
            var loginButton = new Button
            {
                Size = new Size(size.Width - 20, 25),
                Text = "&Login",
                Location = new Point(10, 25)
            };
            otpInputBox.Controls.Add(loginButton);
        
            async void OkButton_Click(object sender, EventArgs e)
            {
                if (otpNotMasterTextBox.Text.Length != 6 || !otpNotMasterTextBox.Text.All(char.IsDigit))
                {
                    MessageBox.Show("Please enter a valid OTP.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
                
                loginButton.Enabled = false;
                
                if (await StartGameAsync(false, otpNotMasterTextBox.Text))
                {
                    otpInputBox.Close();
                    Close();
                    Environment.Exit(0);
                }
                else
                {
                    loginButton.Enabled = false;
                }
            }
            loginButton.Click += OkButton_Click;

            void Exit(object sender, FormClosingEventArgs e)
            {
                btn_startGame.Enabled = true;
            }
            otpInputBox.FormClosing += Exit;

            void TextBox_KeyPress(object sender, KeyPressEventArgs e)
            {
                if (e.KeyChar == Convert.ToChar(Keys.Return))
                {
                    loginButton.PerformClick();
                }
            }
            otpNotMasterTextBox.KeyPress += TextBox_KeyPress;
        
            otpInputBox.Show(this);
        }
        
        private async Task<bool> StartGameAsync(bool useMasterOTP, string otpNotMaster)
        {
            var gameExecutableFilePath = Path.Combine(_configuration.GameDirectoryPath, "BlackDesertEAC.exe");

            if (!File.Exists(gameExecutableFilePath))
            {
                MessageBox.Show($"Failed to find `BlackDesertEAC.exe`.\nUsed path: `{gameExecutableFilePath}`.\nPlease set the correct path to the game.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }

            if (string.IsNullOrEmpty(UsernameTextBox.Text) || string.IsNullOrEmpty(PasswordTextBox.Text))
            {
                MessageBox.Show("Please enter the valid credential(s).",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }

            var authenticationServiceProvider = new AuthenticationServiceProvider();
            string macAddress = null;
            string otp = useMasterOTP ? OtpTextBox.Text : otpNotMaster;
            
            if (MacAddressCheckBox.Checked && string.IsNullOrEmpty(MacAddressTextBox.Text))
                macAddress = "?";
            else if (MacAddressCheckBox.Checked && !string.IsNullOrEmpty(MacAddressTextBox.Text))
                macAddress = MacAddressTextBox.Text;
            
            var playToken = await authenticationServiceProvider.AuthenticateAsync(
                UsernameTextBox.Text, 
                PasswordTextBox.Text, 
                RegionComboBox.SelectedItem.ToString(),
                OtpCheckBox.Checked,
                useMasterOTP,
                otp,
                macAddress);
            
            if (!playToken.StartsWith("0x"))
            {
                if (playToken.Contains("Change Password"))
                {
                    if (MessageBox.Show($"Your password is too old, please login using the official launcher to change your password\n\nThis error is from the game server, it will come up every 3 months",
                            "Password Too Old Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error)
                        == DialogResult.OK)
                    {
                        Close();
                        Environment.Exit(0);
                    }
                }
                else if (MessageBox.Show($"{playToken}\n\nPlease report the error if the error isn't your username/password/otp",
                        "Authentication Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
                    == DialogResult.OK)
                {
                    return false;
                }
            }
            
            if (!GameMode32BitCheckBox.Checked)
                playToken += " -eac_launcher_settings Settings64.json";
            
            using (var process = new Process())
            {
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WorkingDirectory = Path.GetDirectoryName(gameExecutableFilePath);
                
                if (!adminCheckBox.Checked)
                {
                    // RunAsInvoker
                    process.StartInfo.FileName = "CMD";
                    process.StartInfo.Arguments = "/min /C set __COMPAT_LAYER=RUNASINVOKER && start /affinity 5550000 \"\" \"" + gameExecutableFilePath + "\" " + playToken;
                }
                else
                {
                    // RunAsAdmin
                    process.StartInfo.Verb = "runas";
                    process.StartInfo.FileName = gameExecutableFilePath;
                    process.StartInfo.Arguments = playToken;
                }
                
                process.Start();
            }

            return true;
        }
    }
}
