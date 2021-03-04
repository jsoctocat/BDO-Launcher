namespace Launcher
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.LoginGroupBox = new System.Windows.Forms.GroupBox();
            this.GameModeCheckBox = new System.Windows.Forms.CheckBox();
            this.UsernameTextBox = new System.Windows.Forms.TextBox();
            this.UsernameLabel = new System.Windows.Forms.Label();
            this.PasswordLabel = new System.Windows.Forms.Label();
            this.PasswordTextBox = new System.Windows.Forms.TextBox();
            this.RegionLabel = new System.Windows.Forms.Label();
            this.RegionComboBox = new System.Windows.Forms.ComboBox();
            this.OTPCheckBox = new System.Windows.Forms.CheckBox();
            this.RememberDataCheckBox = new System.Windows.Forms.CheckBox();
            this.LoginAutomaticallyCheckBox = new System.Windows.Forms.CheckBox();
            this.StartGameButton = new System.Windows.Forms.Button();
            this.GameDirectoryPathLinkLabel = new System.Windows.Forms.LinkLabel();
            this.GithubLinkLabel = new System.Windows.Forms.LinkLabel();
            this.LoginGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // LoginGroupBox
            // 
            this.LoginGroupBox.Controls.Add(this.GameModeCheckBox);
            this.LoginGroupBox.Controls.Add(this.UsernameTextBox);
            this.LoginGroupBox.Controls.Add(this.UsernameLabel);
            this.LoginGroupBox.Controls.Add(this.PasswordLabel);
            this.LoginGroupBox.Controls.Add(this.PasswordTextBox);
            this.LoginGroupBox.Controls.Add(this.RegionLabel);
            this.LoginGroupBox.Controls.Add(this.RegionComboBox);
            this.LoginGroupBox.Controls.Add(this.OTPCheckBox);
            this.LoginGroupBox.Controls.Add(this.RememberDataCheckBox);
            this.LoginGroupBox.Controls.Add(this.LoginAutomaticallyCheckBox);
            this.LoginGroupBox.Controls.Add(this.StartGameButton);
            this.LoginGroupBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.LoginGroupBox.Location = new System.Drawing.Point(12, 12);
            this.LoginGroupBox.Name = "LoginGroupBox";
            this.LoginGroupBox.Size = new System.Drawing.Size(337, 164);
            this.LoginGroupBox.TabIndex = 1;
            this.LoginGroupBox.TabStop = false;
            this.LoginGroupBox.Text = "Login";
            // 
            // GameModeCheckBox
            // 
            this.GameModeCheckBox.Location = new System.Drawing.Point(263, 78);
            this.GameModeCheckBox.Name = "GameModeCheckBox";
            this.GameModeCheckBox.Size = new System.Drawing.Size(56, 24);
            this.GameModeCheckBox.TabIndex = 7;
            this.GameModeCheckBox.Text = "32 BIT";
            this.GameModeCheckBox.UseVisualStyleBackColor = true;
            this.GameModeCheckBox.CheckedChanged += new System.EventHandler(this.GameModeCheckBox_CheckedChanged);
            // 
            // UsernameTextBox
            // 
            this.UsernameTextBox.Location = new System.Drawing.Point(83, 21);
            this.UsernameTextBox.Name = "UsernameTextBox";
            this.UsernameTextBox.Size = new System.Drawing.Size(236, 22);
            this.UsernameTextBox.TabIndex = 1;
            // 
            // UsernameLabel
            // 
            this.UsernameLabel.AutoSize = true;
            this.UsernameLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.UsernameLabel.Location = new System.Drawing.Point(16, 24);
            this.UsernameLabel.Name = "UsernameLabel";
            this.UsernameLabel.Size = new System.Drawing.Size(61, 13);
            this.UsernameLabel.TabIndex = 0;
            this.UsernameLabel.Text = "Username:";
            // 
            // PasswordLabel
            // 
            this.PasswordLabel.AutoSize = true;
            this.PasswordLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.PasswordLabel.Location = new System.Drawing.Point(18, 52);
            this.PasswordLabel.Name = "PasswordLabel";
            this.PasswordLabel.Size = new System.Drawing.Size(59, 13);
            this.PasswordLabel.TabIndex = 2;
            this.PasswordLabel.Text = "Password:";
            // 
            // PasswordTextBox
            // 
            this.PasswordTextBox.Location = new System.Drawing.Point(83, 49);
            this.PasswordTextBox.Name = "PasswordTextBox";
            this.PasswordTextBox.PasswordChar = '*';
            this.PasswordTextBox.Size = new System.Drawing.Size(236, 22);
            this.PasswordTextBox.TabIndex = 3;
            // 
            // RegionLabel
            // 
            this.RegionLabel.Location = new System.Drawing.Point(29, 80);
            this.RegionLabel.Name = "RegionLabel";
            this.RegionLabel.Size = new System.Drawing.Size(48, 21);
            this.RegionLabel.TabIndex = 4;
            this.RegionLabel.Text = "Region:";
            // 
            // RegionComboBox
            // 
            this.RegionComboBox.AutoCompleteCustomSource.AddRange(new string[] {"NA", "EU"});
            this.RegionComboBox.FormattingEnabled = true;
            this.RegionComboBox.Items.AddRange(new object[] {"NA", "EU"});
            this.RegionComboBox.Location = new System.Drawing.Point(83, 80);
            this.RegionComboBox.MaxDropDownItems = 2;
            this.RegionComboBox.Name = "RegionComboBox";
            this.RegionComboBox.Size = new System.Drawing.Size(91, 21);
            this.RegionComboBox.TabIndex = 5;
            this.RegionComboBox.SelectedIndexChanged += new System.EventHandler(this.RegionComboBox_SelectedIndexChanged);
            // 
            // OTPCheckBox
            // 
            this.OTPCheckBox.Location = new System.Drawing.Point(193, 78);
            this.OTPCheckBox.Name = "OTPCheckBox";
            this.OTPCheckBox.Size = new System.Drawing.Size(55, 24);
            this.OTPCheckBox.TabIndex = 6;
            this.OTPCheckBox.Text = "OTP";
            this.OTPCheckBox.UseVisualStyleBackColor = true;
            this.OTPCheckBox.CheckedChanged += new System.EventHandler(this.OTPCheckBox_CheckedChanged);
            // 
            // RememberDataCheckBox
            // 
            this.RememberDataCheckBox.AutoSize = true;
            this.RememberDataCheckBox.Location = new System.Drawing.Point(16, 107);
            this.RememberDataCheckBox.Name = "RememberDataCheckBox";
            this.RememberDataCheckBox.Size = new System.Drawing.Size(107, 17);
            this.RememberDataCheckBox.TabIndex = 8;
            this.RememberDataCheckBox.Text = "Remember Data";
            this.RememberDataCheckBox.UseVisualStyleBackColor = true;
            this.RememberDataCheckBox.CheckedChanged += new System.EventHandler(this.RememberDataCheckBox_CheckedChanged);
            // 
            // LoginAutomaticallyCheckBox
            // 
            this.LoginAutomaticallyCheckBox.AutoSize = true;
            this.LoginAutomaticallyCheckBox.Location = new System.Drawing.Point(193, 107);
            this.LoginAutomaticallyCheckBox.Name = "LoginAutomaticallyCheckBox";
            this.LoginAutomaticallyCheckBox.Size = new System.Drawing.Size(126, 17);
            this.LoginAutomaticallyCheckBox.TabIndex = 9;
            this.LoginAutomaticallyCheckBox.Text = "Login automatically";
            this.LoginAutomaticallyCheckBox.UseVisualStyleBackColor = true;
            this.LoginAutomaticallyCheckBox.CheckedChanged += new System.EventHandler(this.LoginAutomaticallyCheckBox_CheckedChanged);
            // 
            // StartGameButton
            // 
            this.StartGameButton.Location = new System.Drawing.Point(83, 130);
            this.StartGameButton.Name = "StartGameButton";
            this.StartGameButton.Size = new System.Drawing.Size(155, 23);
            this.StartGameButton.TabIndex = 10;
            this.StartGameButton.Text = "Start Game";
            this.StartGameButton.UseVisualStyleBackColor = true;
            this.StartGameButton.Click += new System.EventHandler(this.StartGameButton_Click);
            // 
            // GameDirectoryPathLinkLabel
            // 
            this.GameDirectoryPathLinkLabel.AutoSize = true;
            this.GameDirectoryPathLinkLabel.Location = new System.Drawing.Point(12, 186);
            this.GameDirectoryPathLinkLabel.Name = "GameDirectoryPathLinkLabel";
            this.GameDirectoryPathLinkLabel.Size = new System.Drawing.Size(81, 13);
            this.GameDirectoryPathLinkLabel.TabIndex = 11;
            this.GameDirectoryPathLinkLabel.TabStop = true;
            this.GameDirectoryPathLinkLabel.Text = "Set Game Path";
            this.GameDirectoryPathLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GameDirectoryPathLinkLabel_LinkClicked);
            // 
            // GithubLinkLabel
            // 
            this.GithubLinkLabel.AutoSize = true;
            this.GithubLinkLabel.Location = new System.Drawing.Point(259, 186);
            this.GithubLinkLabel.Name = "GithubLinkLabel";
            this.GithubLinkLabel.Size = new System.Drawing.Size(90, 13);
            this.GithubLinkLabel.TabIndex = 12;
            this.GithubLinkLabel.TabStop = true;
            this.GithubLinkLabel.Text = "Original Creator";
            this.GithubLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GithubLinkLabel_LinkClicked);
            // 
            // MainForm
            // 
            this.AcceptButton = this.StartGameButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(364, 208);
            this.Controls.Add(this.GameDirectoryPathLinkLabel);
            this.Controls.Add(this.GithubLinkLabel);
            this.Controls.Add(this.LoginGroupBox);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte) (0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Launcher";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.LoginGroupBox.ResumeLayout(false);
            this.LoginGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        #endregion

        private System.Windows.Forms.GroupBox LoginGroupBox;
        private System.Windows.Forms.Label UsernameLabel;
        private System.Windows.Forms.TextBox UsernameTextBox;
        private System.Windows.Forms.Label PasswordLabel;
        private System.Windows.Forms.TextBox PasswordTextBox;
        private System.Windows.Forms.Label RegionLabel;
        private System.Windows.Forms.ComboBox RegionComboBox;
        private System.Windows.Forms.CheckBox OTPCheckBox;
        private System.Windows.Forms.CheckBox GameModeCheckBox;
        private System.Windows.Forms.CheckBox RememberDataCheckBox;
        private System.Windows.Forms.CheckBox LoginAutomaticallyCheckBox;
        private System.Windows.Forms.Button StartGameButton;
        private System.Windows.Forms.LinkLabel GithubLinkLabel;
        private System.Windows.Forms.LinkLabel GameDirectoryPathLinkLabel;
    }
}

