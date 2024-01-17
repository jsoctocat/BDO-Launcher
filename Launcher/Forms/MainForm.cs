using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Launcher.Source;
using Newtonsoft.Json.Linq;

namespace Launcher.Forms
{
    
    public partial class MainForm : Form
    {
        private Configuration _configuration;
        private const string Version = "1.1.8";
        private const string Title = "Custom Black Desert Launcher (" + Version + ")";
        
        public MainForm()
        {
            InitializeComponent();
            Text = Title;
        }

        private async Task CheckVersion(bool launcherUpdate, bool gameUpdate)
        {
            Uri launcherVersionUrl = 
                new Uri("https://api.github.com/repos/jsoctocat/BDO-Launcher/tags");

            Uri gameVersionUrl = new Uri("https://naeu-o-dn.playblackdesert.com/UploadData/client_version");
            
            // Check for launcher update
            if (launcherUpdate)
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("user-agent", "BDO-Launcher");
                    var resultString = wc.DownloadString("https://api.github.com/repos/jsoctocat/BDO-Launcher/tags");
                    var json = JArray.Parse(resultString);
                    var name = json[0]["name"];
                    
                    if (name != null && name.Value<string>() != Version)
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

            // Check for game update
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
                usernameTextBox.Text = _configuration.Username;
                passwordTextBox.Text = _configuration.GetPassword();
                MacAddressTextBox.Text = _configuration.MacAddress;
                affinityBitmaskTextBox.Text = _configuration.AffinityBitmask;
            }

            otpCheckBox.Checked = _configuration.Otp;
            otpTextBox.Text = _configuration.GetOtp();
            regionComboBox.SelectedIndex = _configuration.RegionComboBox;
            pcRegCheckBox.Checked = _configuration.PcRegistration;
            coreAffinityCheckBox.Checked = _configuration.CoreAffinity;
            gameMode32BitCheckBox.Checked = _configuration.GameMode32Bit;
            rememberDataCheckBox.Checked = _configuration.RememberData;
            loginAutomaticallyCheckBox.Checked = _configuration.LoginAutomatically;
            launcherUpdateCheckBox.Checked = _configuration.LauncherUpdate;
            gameUpdateCheckBox.Checked = _configuration.GameUpdate;
            adminCheckBox.Checked = _configuration.RunAsAdmin;
            hideBrowserFormCheckBox.Checked = _configuration.HideBrowserForm;

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
            if (rememberDataCheckBox.Checked)
            {
                _configuration.Username = usernameTextBox.Text;
                _configuration.SetPassword(passwordTextBox.Text);
                _configuration.MacAddress = MacAddressTextBox.Text;
                _configuration.AffinityBitmask = affinityBitmaskTextBox.Text;
            }
            // Always save OTP
            _configuration.SetOtp(otpTextBox.Text);

            ConfigurationManager.Save(_configuration);
        }
        
        private void OtpCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // Do not allow edit OTP if Otp is enabled
            otpTextBox.Enabled = !otpCheckBox.Checked;
            
            _configuration.Otp = otpCheckBox.Checked;

            ConfigurationManager.Save(_configuration);
        }

        private void RegionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _configuration.RegionComboBox = regionComboBox.SelectedIndex;
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
                File.WriteAllText(regionFilePath, regionComboBox.SelectedItem.ToString());
                File.WriteAllText(Path.Combine(_configuration.GameDirectoryPath, "service.ini"),
                    regionInfo[regionComboBox.SelectedIndex]);
            }
            
            var currentRegion = File.ReadAllText(regionFilePath);

            if (currentRegion == regionComboBox.SelectedItem.ToString()) return;
                
            File.WriteAllText(regionFilePath, region[regionComboBox.SelectedIndex]);
            File.WriteAllText(Path.Combine(_configuration.GameDirectoryPath, "service.ini"),
                regionInfo[regionComboBox.SelectedIndex]);
        }
        
        private void MacAddressCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // Do not allow edit Mac Address if PC Registration is enabled
            MacAddressTextBox.Enabled = !pcRegCheckBox.Checked;
            
            _configuration.PcRegistration = pcRegCheckBox.Checked;

            ConfigurationManager.Save(_configuration);
        }

        private void CoreAffinityCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            // Do not allow edit Affinity Bitmask if Core Affinity is enabled
            affinityBitmaskTextBox.Enabled = !coreAffinityCheckBox.Checked;
            
            _configuration.CoreAffinity = coreAffinityCheckBox.Checked;

            ConfigurationManager.Save(_configuration);
        }

        private void AffinityBitmaskTextBox_TextChanged(object sender, EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void GameModeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.GameMode32Bit = gameMode32BitCheckBox.Checked;

            ConfigurationManager.Save(_configuration);
        }
        
        private void RememberDataCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.RememberData = rememberDataCheckBox.Checked;

            ConfigurationManager.Save(_configuration);
        }

        private void LoginAutomaticallyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (loginAutomaticallyCheckBox.Checked)
            {
                rememberDataCheckBox.Checked = true;
                rememberDataCheckBox.Enabled = false;
            }
            else
                rememberDataCheckBox.Enabled = true;

            _configuration.LoginAutomatically = loginAutomaticallyCheckBox.Checked;

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

        private void HideBrowserFormCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _configuration.HideBrowserForm = hideBrowserFormCheckBox.Checked;

            ConfigurationManager.Save(_configuration);
        }
        
        private void StartGameButton_Click(object sender, EventArgs e)
        {
            GameStart();
        }
        
        private async void GameStart()
        {
            startGameBtn.Enabled = false;
            
            if (otpCheckBox.Checked && string.IsNullOrEmpty(otpTextBox.Text))
                OneTimePasswordAsync();
            else if (await StartGameAsync(true, null))
            {
                Close();
                Environment.Exit(0);
            }
            else
                startGameBtn.Enabled = true;
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
                startGameBtn.Enabled = true;
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
            var gameExecutableFilePath = gameMode32BitCheckBox.Checked ? 
                Path.Combine("bin","BlackDesert32.exe") : Path.Combine("bin64", "BlackDesert64.exe");
            
            var launchPath = Path.Combine(_configuration.GameDirectoryPath, gameExecutableFilePath);

            if (!File.Exists(launchPath))
            {
                MessageBox.Show($"Failed to find `{launchPath}`.\nUsed path: `{_configuration.GameDirectoryPath}`.\nPlease set the correct path to the game's base directory where the bin/bin64 folder reside.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }

            if (string.IsNullOrEmpty(usernameTextBox.Text) || string.IsNullOrEmpty(passwordTextBox.Text))
            {
                MessageBox.Show("Please enter the valid credential(s).",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }

            var authenticationServiceProvider = new AuthenticationServiceProvider();
            string macAddress = null;
            string otp = useMasterOTP ? otpTextBox.Text : otpNotMaster;
            
            if (pcRegCheckBox.Checked && string.IsNullOrEmpty(MacAddressTextBox.Text))
                macAddress = "?";
            else if (pcRegCheckBox.Checked && !string.IsNullOrEmpty(MacAddressTextBox.Text))
                macAddress = MacAddressTextBox.Text;
            
            var playToken = await authenticationServiceProvider.AuthenticateAsync(
                usernameTextBox.Text, 
                passwordTextBox.Text, 
                regionComboBox.SelectedItem.ToString(),
                otpCheckBox.Checked,
                useMasterOTP,
                otp,
                macAddress,
                hideBrowserFormCheckBox.Checked);
            
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

            string affinityBitmask = "";
            if (coreAffinityCheckBox.Checked && !string.IsNullOrEmpty(affinityBitmaskTextBox.Text))
                affinityBitmask = " /affinity " + affinityBitmaskTextBox.Text.Trim();
            
            using (var process = new Process())
            {
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WorkingDirectory = Path.GetDirectoryName(launchPath);
                
                if (!adminCheckBox.Checked)
                {
                    // RunAsInvoker
                    process.StartInfo.FileName = "CMD";
                    process.StartInfo.Arguments = "/min /C set __COMPAT_LAYER=RUNASINVOKER && start" + affinityBitmask + " \"\" \"" + launchPath + "\" " + playToken;
                }
                else
                {
                    // RunAsAdmin
                    process.StartInfo.Verb = "runas";
                    process.StartInfo.FileName = launchPath;
                    process.StartInfo.Arguments = playToken;
                }
                
                process.Start();
            }

            return true;
        }
    }
}
