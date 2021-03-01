using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Launcher
{

    public partial class MainForm : Form
    {

        private Configuration configuration;

        public MainForm()
        {
            this.InitializeComponent();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            this.configuration = ConfigurationManager.Load();

            if (this.configuration == null)
            {
                this.configuration = new Configuration();

                ConfigurationManager.Save(this.configuration);
            }

            if (this.configuration.LoginAutomatically && (Environment.GetCommandLineArgs().Length >= 2) && (Environment.GetCommandLineArgs()[1].ToLower() == "--disable-automatic-login"))
                this.configuration.LoginAutomatically = false;

            if (this.CheckGameDirectoryPathAndPrompt())
                this.Text = $"Launcher | {this.configuration.GameDirectoryPath}";

            if (this.configuration.RememberData)
            {
                this.UsernameTextBox.Text = this.configuration.Username;
                this.PasswordTextBox.Text = this.configuration.GetPassword();
            }

            this.RememberDataCheckBox.Checked = this.configuration.RememberData;
            this.LoginAutomaticallyCheckBox.Checked = this.configuration.LoginAutomatically;

            if (this.configuration.LoginAutomatically)
            {
                this.StartGameButton.Enabled = false;

                if (await this.StartGameAsync())
                    this.Close();
                else
                    this.StartGameButton.Enabled = true;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.configuration.Username = this.UsernameTextBox.Text;

            this.configuration.SetPassword(this.PasswordTextBox.Text);

            ConfigurationManager.Save(this.configuration);
        }

        private void RememberDataCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.configuration.RememberData = this.RememberDataCheckBox.Checked;

            ConfigurationManager.Save(this.configuration);
        }

        private void LoginAutomaticallyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (this.LoginAutomaticallyCheckBox.Checked)
            {
                this.RememberDataCheckBox.Checked = true;
                this.RememberDataCheckBox.Enabled = false;
            }
            else
                this.RememberDataCheckBox.Enabled = true;

            this.configuration.LoginAutomatically = this.LoginAutomaticallyCheckBox.Checked;

            ConfigurationManager.Save(this.configuration);
        }

        private async void StartGameButton_Click(object sender, EventArgs e)
        {
            this.StartGameButton.Enabled = false;

            if (await this.StartGameAsync())
                this.Close();
            else
                this.StartGameButton.Enabled = true;
        }

        private void GameDirectoryPathLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string newGameDirectoryPath = this.SelectGameDirectoryPath();

            if (newGameDirectoryPath != null)
            {
                this.configuration.GameDirectoryPath = newGameDirectoryPath;

                ConfigurationManager.Save(this.configuration);

                this.Text = $"Launcher | {this.configuration.GameDirectoryPath}";
            }
        }

        private void GithubLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(this.GithubLinkLabel.Text);
        }

        private bool CheckGameDirectoryPathAndPrompt()
        {
            string messageBoxText = null;

            if (String.IsNullOrEmpty(this.configuration.GameDirectoryPath))
                messageBoxText = "The path to the game is not set.\nDo you want to set it now?";
            else if (!Directory.Exists(this.configuration.GameDirectoryPath) || !File.Exists(Path.Combine(this.configuration.GameDirectoryPath, "Black Desert Online Launcher.exe")))
                messageBoxText = "The path to the game is invalid.\nDo you want to set it now?";
            else
                return true;

            if (MessageBox.Show(messageBoxText,
                this.Text, MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                string newGameDirectoryPath = this.SelectGameDirectoryPath();

                if (newGameDirectoryPath != null)
                {
                    this.configuration.GameDirectoryPath = newGameDirectoryPath;

                    ConfigurationManager.Save(this.configuration);

                    this.Text = $"Launcher | {this.configuration.GameDirectoryPath}";
                }
            }

            this.Activate();

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
            string gameExecutableFilePath = Path.Combine(this.configuration.GameDirectoryPath, "bin64", "BlackDesert64.exe");

            if (!File.Exists(gameExecutableFilePath))
            {
                MessageBox.Show($"Failed to find `BlackDesert64.exe`.\nUsed path: `{gameExecutableFilePath}`.\nPlease set the correct path to the game.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }

            if (String.IsNullOrEmpty(this.UsernameTextBox.Text) || String.IsNullOrEmpty(this.PasswordTextBox.Text))
            {
                MessageBox.Show("Please enter valid credentials.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }

            using (AuthenticationServiceProvider authenticationServiceProvider = new AuthenticationServiceProvider())
            {
                string playToken = await authenticationServiceProvider.AuthenticateAsync(this.UsernameTextBox.Text, this.PasswordTextBox.Text);

                if (playToken == null)
                {
                    MessageBox.Show("Your username/password is not correct.\n(Or there might be an authentication problem.)",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return false;
                }

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
