using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Http;

namespace Launcher
{
    public partial class MainForm : Form
    {
        private Configuration _configuration;
        private Otp _otp;
        private const string _version = "1.1.2b";
        
        public MainForm()
        {
            InitializeComponent();
            _otp = new Otp();
        }

        private async Task CheckVersion(bool launcherUpdate, bool gameUpdate)
        {
            Uri launcherVersionUrl = 
                new Uri("https://gist.githubusercontent.com/jsoctocat/4aeb78c8b7d92aca96911afa393614d5/raw/version");
            
            // Check version on load
            using (var handler = new HttpClientHandler { CookieContainer = new CookieContainer() })
            using (var client = new HttpClient(handler) { BaseAddress = launcherVersionUrl })
            {
                using (var result = await client.GetAsync(launcherVersionUrl))
                {
                    if (!result.IsSuccessStatusCode)
                        return;
                    
                    var resultContent = await result.Content.ReadAsStringAsync();
                    string[] versions = resultContent.Split(',');

                    // Check for launcher update
                    if (launcherUpdate && _version != versions[0])
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
                    
                    if (gameUpdate)
                    {
                        var metaFilePath = Path.Combine(_configuration.GameDirectoryPath, "Paz", "pad00000.meta");
                        FileStream metaFile = new FileStream(metaFilePath, FileMode.Open);
                        
                        var clientVersionBytes = 4;
                        var buffer = new byte[clientVersionBytes];
                        metaFile.Read(buffer, 0, clientVersionBytes);
                        var clientVersion = BitConverter.ToInt32(buffer, 0);

                        if (clientVersion < int.Parse(versions[1]))
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
        
        private void StartGameButton_Click(object sender, EventArgs e)
        {
            GameStart();
        }
        
        private async void GameStart()
        {
            StartGameButton.Enabled = false;

            if (OtpCheckBox.Checked && string.IsNullOrEmpty(OtpTextBox.Text))
                OneTimePasswordAsync();
            else if (await StartGameAsync())
                Close();
            else
                StartGameButton.Enabled = true;
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
        
            var otpTextBox = new TextBox
            {
                Size = new Size(size.Width - 20, 25),
                Location = new Point(10, 5)
            };
        
            otpInputBox.Controls.Add(otpTextBox);
        
            var loginButton = new Button
            {
                Size = new Size(size.Width - 20, 25),
                Text = "&Login",
                Location = new Point(10, 25)
            };
            otpInputBox.Controls.Add(loginButton);
        
            async void OkButton_Click(object sender, EventArgs e)
            {
                if (string.IsNullOrEmpty(otpTextBox.Text))
                {
                    MessageBox.Show("Please enter a valid OTP.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }
                
                _otp.OneTimePassword = int.Parse(otpTextBox.Text);
                
                if (await StartGameAsync())
                {
                    otpInputBox.Close();
                    Close();
                }
                else
                    StartGameButton.Enabled = true;
            }
            loginButton.Click += OkButton_Click;
        
            void Exit(object sender, FormClosingEventArgs e)
            {
                StartGameButton.Enabled = true;
            }
            otpInputBox.FormClosing += Exit;
        
            void TextBox_KeyPress(object sender, KeyPressEventArgs e)
            {
                if (e.KeyChar == Convert.ToChar(Keys.Return))
                {
                    loginButton.PerformClick();
                }
            }
            otpTextBox.KeyPress += TextBox_KeyPress;
        
            otpInputBox.Show(this);
        }
        
        private async Task<bool> StartGameAsync()
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
            var otp = 0;
            string macAddress = null;

            if (MacAddressCheckBox.Checked && string.IsNullOrEmpty(MacAddressTextBox.Text))
                macAddress = "1";
            else if (MacAddressCheckBox.Checked && !string.IsNullOrEmpty(MacAddressTextBox.Text))
                macAddress = MacAddressTextBox.Text;
                
            if (OtpCheckBox.Checked)
            {
                // Skip if not using master OTP
                if(!string.IsNullOrEmpty(OtpTextBox.Text))
                    _otp.Password = Base32Converter.ToBytes(OtpTextBox.Text);
                
                otp = _otp.OneTimePassword;
            }

            var playToken = await authenticationServiceProvider.AuthenticateAsync(
                UsernameTextBox.Text, 
                PasswordTextBox.Text, 
                RegionComboBox.SelectedItem.ToString(), 
                otp, macAddress);

            if (!playToken.StartsWith("0x"))
            {
                if (MessageBox.Show($"{playToken}\n\nPlease report the error if the error isn't your username/password/otp",
                        "Authentication Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error)
                    == DialogResult.OK)
                {
                    Close();
                }
            }
            
            if (!GameMode32BitCheckBox.Checked)
                playToken += " -eac_launcher_settings Settings64.json";

            using (var process = new Process())
            {
                process.StartInfo.FileName = "CMD";
                process.StartInfo.Arguments = "/min /C set __COMPAT_LAYER=RUNASINVOKER && start \"\" \"" + gameExecutableFilePath + "\" " + playToken;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WorkingDirectory = Path.GetDirectoryName(gameExecutableFilePath);
                process.Start();

                // The following will start the game normally
                //process.StartInfo.FileName = gameExecutableFilePath;
                //process.StartInfo.Arguments = playToken;
                //process.StartInfo.WorkingDirectory = Path.GetDirectoryName(gameExecutableFilePath);
                //process.Start();
            }

            return true;
        }
    }
}
