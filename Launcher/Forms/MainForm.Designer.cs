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
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.UsernameLabel = new System.Windows.Forms.Label();
            this.LoginGroupBox = new System.Windows.Forms.GroupBox();
            this.UsernameTextBox = new System.Windows.Forms.TextBox();
            this.PasswordTextBox = new System.Windows.Forms.TextBox();
            this.PasswordLabel = new System.Windows.Forms.Label();
            this.LoginAutomaticallyCheckBox = new System.Windows.Forms.CheckBox();
            this.RememberDataCheckBox = new System.Windows.Forms.CheckBox();
            this.StartGameButton = new System.Windows.Forms.Button();
            this.GithubLinkLabel = new System.Windows.Forms.LinkLabel();
            this.GameDirectoryPathLinkLabel = new System.Windows.Forms.LinkLabel();
            this.LoginGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // UsernameLabel
            // 
            this.UsernameLabel.AutoSize = true;
            this.UsernameLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UsernameLabel.Location = new System.Drawing.Point(16, 24);
            this.UsernameLabel.Name = "UsernameLabel";
            this.UsernameLabel.Size = new System.Drawing.Size(61, 13);
            this.UsernameLabel.TabIndex = 0;
            this.UsernameLabel.Text = "Username:";
            // 
            // LoginGroupBox
            // 
            this.LoginGroupBox.Controls.Add(this.StartGameButton);
            this.LoginGroupBox.Controls.Add(this.RememberDataCheckBox);
            this.LoginGroupBox.Controls.Add(this.LoginAutomaticallyCheckBox);
            this.LoginGroupBox.Controls.Add(this.PasswordLabel);
            this.LoginGroupBox.Controls.Add(this.PasswordTextBox);
            this.LoginGroupBox.Controls.Add(this.UsernameTextBox);
            this.LoginGroupBox.Controls.Add(this.UsernameLabel);
            this.LoginGroupBox.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LoginGroupBox.Location = new System.Drawing.Point(12, 12);
            this.LoginGroupBox.Name = "LoginGroupBox";
            this.LoginGroupBox.Size = new System.Drawing.Size(337, 164);
            this.LoginGroupBox.TabIndex = 1;
            this.LoginGroupBox.TabStop = false;
            this.LoginGroupBox.Text = "Login";
            // 
            // UsernameTextBox
            // 
            this.UsernameTextBox.Location = new System.Drawing.Point(83, 21);
            this.UsernameTextBox.Name = "UsernameTextBox";
            this.UsernameTextBox.Size = new System.Drawing.Size(236, 22);
            this.UsernameTextBox.TabIndex = 1;
            // 
            // PasswordTextBox
            // 
            this.PasswordTextBox.Location = new System.Drawing.Point(83, 49);
            this.PasswordTextBox.Name = "PasswordTextBox";
            this.PasswordTextBox.PasswordChar = '*';
            this.PasswordTextBox.Size = new System.Drawing.Size(236, 22);
            this.PasswordTextBox.TabIndex = 2;
            // 
            // PasswordLabel
            // 
            this.PasswordLabel.AutoSize = true;
            this.PasswordLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PasswordLabel.Location = new System.Drawing.Point(18, 52);
            this.PasswordLabel.Name = "PasswordLabel";
            this.PasswordLabel.Size = new System.Drawing.Size(59, 13);
            this.PasswordLabel.TabIndex = 3;
            this.PasswordLabel.Text = "Password:";
            // 
            // LoginAutomaticallyCheckBox
            // 
            this.LoginAutomaticallyCheckBox.AutoSize = true;
            this.LoginAutomaticallyCheckBox.Location = new System.Drawing.Point(83, 105);
            this.LoginAutomaticallyCheckBox.Name = "LoginAutomaticallyCheckBox";
            this.LoginAutomaticallyCheckBox.Size = new System.Drawing.Size(126, 17);
            this.LoginAutomaticallyCheckBox.TabIndex = 5;
            this.LoginAutomaticallyCheckBox.Text = "Login automatically";
            this.LoginAutomaticallyCheckBox.UseVisualStyleBackColor = true;
            this.LoginAutomaticallyCheckBox.CheckedChanged += new System.EventHandler(this.LoginAutomaticallyCheckBox_CheckedChanged);
            // 
            // RememberDataCheckBox
            // 
            this.RememberDataCheckBox.AutoSize = true;
            this.RememberDataCheckBox.Location = new System.Drawing.Point(83, 82);
            this.RememberDataCheckBox.Name = "RememberDataCheckBox";
            this.RememberDataCheckBox.Size = new System.Drawing.Size(107, 17);
            this.RememberDataCheckBox.TabIndex = 4;
            this.RememberDataCheckBox.Text = "Remember Data";
            this.RememberDataCheckBox.UseVisualStyleBackColor = true;
            this.RememberDataCheckBox.CheckedChanged += new System.EventHandler(this.RememberDataCheckBox_CheckedChanged);
            // 
            // StartGameButton
            // 
            this.StartGameButton.Location = new System.Drawing.Point(83, 128);
            this.StartGameButton.Name = "StartGameButton";
            this.StartGameButton.Size = new System.Drawing.Size(155, 23);
            this.StartGameButton.TabIndex = 6;
            this.StartGameButton.Text = "Start Game";
            this.StartGameButton.UseVisualStyleBackColor = true;
            this.StartGameButton.Click += new System.EventHandler(this.StartGameButton_Click);
            // 
            // GithubLinkLabel
            // 
            this.GithubLinkLabel.AutoSize = true;
            this.GithubLinkLabel.Location = new System.Drawing.Point(127, 184);
            this.GithubLinkLabel.Name = "GithubLinkLabel";
            this.GithubLinkLabel.Size = new System.Drawing.Size(222, 13);
            this.GithubLinkLabel.TabIndex = 2;
            this.GithubLinkLabel.TabStop = true;
            this.GithubLinkLabel.Text = "https://github.com/bdoscientist/Launcher";
            this.GithubLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GithubLinkLabel_LinkClicked);
            // 
            // GameDirectoryPathLinkLabel
            // 
            this.GameDirectoryPathLinkLabel.AutoSize = true;
            this.GameDirectoryPathLinkLabel.Location = new System.Drawing.Point(12, 186);
            this.GameDirectoryPathLinkLabel.Name = "GameDirectoryPathLinkLabel";
            this.GameDirectoryPathLinkLabel.Size = new System.Drawing.Size(81, 13);
            this.GameDirectoryPathLinkLabel.TabIndex = 3;
            this.GameDirectoryPathLinkLabel.TabStop = true;
            this.GameDirectoryPathLinkLabel.Text = "Set Game Path";
            this.GameDirectoryPathLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GameDirectoryPathLinkLabel_LinkClicked);
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
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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

        private System.Windows.Forms.Label UsernameLabel;
        private System.Windows.Forms.GroupBox LoginGroupBox;
        private System.Windows.Forms.TextBox UsernameTextBox;
        private System.Windows.Forms.Label PasswordLabel;
        private System.Windows.Forms.TextBox PasswordTextBox;
        private System.Windows.Forms.CheckBox RememberDataCheckBox;
        private System.Windows.Forms.CheckBox LoginAutomaticallyCheckBox;
        private System.Windows.Forms.Button StartGameButton;
        private System.Windows.Forms.LinkLabel GithubLinkLabel;
        private System.Windows.Forms.LinkLabel GameDirectoryPathLinkLabel;
    }
}

