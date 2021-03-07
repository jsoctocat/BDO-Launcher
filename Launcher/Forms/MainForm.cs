using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Launcher
{
    public partial class MainForm : Form
    {
        private Configuration _configuration;
        private Otp _otp;
        
        public MainForm()
        {
            InitializeComponent();
            _otp = new Otp();
        }

        private void MainForm_Load(object sender, EventArgs e)
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
            }

            OtpCheckBox.Checked = _configuration.Otp;
            OtpTextBox.Text = _configuration.GetOtp();
            RegionComboBox.SelectedIndex = _configuration.RegionComboBox;
            GameMode32BitCheckBox.Checked = _configuration.GameMode32Bit;
            RememberDataCheckBox.Checked = _configuration.RememberData;
            LoginAutomaticallyCheckBox.Checked = _configuration.LoginAutomatically;

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
            }
            //Always save OTP
            _configuration.SetOtp(OtpTextBox.Text);

            ConfigurationManager.Save(_configuration);
        }

        private void OtpCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            //Do not allow edit OTP if Otp is enabled
            OtpTextBox.Enabled = !OtpCheckBox.Checked;
            
            _configuration.Otp = OtpCheckBox.Checked;

            ConfigurationManager.Save(_configuration);
        }

        private void RegionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _configuration.RegionComboBox = RegionComboBox.SelectedIndex;
            
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
        
        private async void GameStart()
        {
            StartGameButton.Enabled = false;
            
            if (await StartGameAsync())
                Close();
            else
                StartGameButton.Enabled = true;
        }
        
        private void StartGameButton_Click(object sender, EventArgs e)
        {
            GameStart();
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
            Process.Start("https://github.com/bdoscientist/Launcher");
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
                MessageBox.Show("Please enter valid credential(s).",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }

            if (OtpCheckBox.Checked && string.IsNullOrEmpty(OtpTextBox.Text))
            {
                MessageBox.Show("Please enter valid OTP.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }

            using (AuthenticationServiceProvider authenticationServiceProvider = new AuthenticationServiceProvider())
            {
                var otp = 0;
                if (OtpCheckBox.Checked)
                {
                    _otp.Password = Base32.ToBytes(OtpTextBox.Text);
                    otp = _otp.OneTimePassword;
                }

                var playToken = await authenticationServiceProvider.AuthenticateAsync(
                    UsernameTextBox.Text, 
                    PasswordTextBox.Text, 
                    RegionComboBox.SelectedItem.ToString(), 
                    otp);

                if (playToken == null)
                {
                    MessageBox.Show("Username, Password, or OTP is not correct.\n(Or there might be an authentication problem.)",
                        "Authentication Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return false;
                }

                if (!GameMode32BitCheckBox.Checked)
                    playToken += " -eac_launcher_settings Settings64.json";

                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "CMD";
                    process.StartInfo.Arguments = "/min /C set __COMPAT_LAYER=RUNASINVOKER && start \"\" \"" + gameExecutableFilePath + "\" " + playToken;
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.RedirectStandardOutput = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.WorkingDirectory = Path.GetDirectoryName(gameExecutableFilePath);
                    process.Start();

                    //process.StartInfo.FileName = gameExecutableFilePath;
                    //process.StartInfo.Arguments = playToken;
                    //process.StartInfo.WorkingDirectory = Path.GetDirectoryName(gameExecutableFilePath);

                    //process.Start();
                }
            }

            return true;
        }
    }

}
