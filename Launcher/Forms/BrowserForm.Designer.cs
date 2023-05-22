namespace Launcher.Forms
{
    partial class BrowserForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
	        this.toolStripContainer = new System.Windows.Forms.ToolStripContainer();
	        this.statusLabel = new System.Windows.Forms.Label();
	        this.outputLabel = new System.Windows.Forms.Label();
	        this.menuStrip1 = new System.Windows.Forms.MenuStrip();
	        this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
	        this.showDevToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
	        this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
	        this.toolStripContainer.ContentPanel.SuspendLayout();
	        this.toolStripContainer.SuspendLayout();
	        this.menuStrip1.SuspendLayout();
	        this.SuspendLayout();
	        // 
	        // toolStripContainer
	        // 
	        // 
	        // toolStripContainer.ContentPanel
	        // 
	        this.toolStripContainer.ContentPanel.Controls.Add(this.statusLabel);
	        this.toolStripContainer.ContentPanel.Controls.Add(this.outputLabel);
	        this.toolStripContainer.ContentPanel.Size = new System.Drawing.Size(730, 466);
	        this.toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
	        this.toolStripContainer.LeftToolStripPanelVisible = false;
	        this.toolStripContainer.Location = new System.Drawing.Point(0, 24);
	        this.toolStripContainer.Name = "toolStripContainer";
	        this.toolStripContainer.RightToolStripPanelVisible = false;
	        this.toolStripContainer.Size = new System.Drawing.Size(730, 466);
	        this.toolStripContainer.TabIndex = 0;
	        this.toolStripContainer.Text = "toolStripContainer1";
	        // 
	        // statusLabel
	        // 
	        this.statusLabel.AutoSize = true;
	        this.statusLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
	        this.statusLabel.Location = new System.Drawing.Point(0, 440);
	        this.statusLabel.Name = "statusLabel";
	        this.statusLabel.Size = new System.Drawing.Size(0, 13);
	        this.statusLabel.TabIndex = 1;
	        // 
	        // outputLabel
	        // 
	        this.outputLabel.AutoSize = true;
	        this.outputLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
	        this.outputLabel.Location = new System.Drawing.Point(0, 453);
	        this.outputLabel.Name = "outputLabel";
	        this.outputLabel.Size = new System.Drawing.Size(0, 13);
	        this.outputLabel.TabIndex = 0;
	        // 
	        // menuStrip1
	        // 
	        this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.fileToolStripMenuItem });
	        this.menuStrip1.Location = new System.Drawing.Point(0, 0);
	        this.menuStrip1.Name = "menuStrip1";
	        this.menuStrip1.Size = new System.Drawing.Size(730, 24);
	        this.menuStrip1.TabIndex = 1;
	        this.menuStrip1.Text = "menuStrip1";
	        // 
	        // fileToolStripMenuItem
	        // 
	        this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { this.showDevToolsToolStripMenuItem, this.exitToolStripMenuItem });
	        this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
	        this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
	        this.fileToolStripMenuItem.Text = "File";
	        // 
	        // showDevToolsToolStripMenuItem
	        // 
	        this.showDevToolsToolStripMenuItem.Name = "showDevToolsToolStripMenuItem";
	        this.showDevToolsToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
	        this.showDevToolsToolStripMenuItem.Text = "Show DevTools";
	        this.showDevToolsToolStripMenuItem.Click += new System.EventHandler(this.ShowDevToolsMenuItemClick);
	        // 
	        // exitToolStripMenuItem
	        // 
	        this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
	        this.exitToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
	        this.exitToolStripMenuItem.Text = "Exit";
	        this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitMenuItemClick);
	        // 
	        // BrowserForm
	        // 
	        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
	        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
	        this.ClientSize = new System.Drawing.Size(730, 490);
	        this.Controls.Add(this.toolStripContainer);
	        this.Controls.Add(this.menuStrip1);
	        this.MainMenuStrip = this.menuStrip1;
	        this.Name = "BrowserForm";
	        this.Text = "BrowserForm";
	        this.toolStripContainer.ContentPanel.ResumeLayout(false);
	        this.toolStripContainer.ContentPanel.PerformLayout();
	        this.toolStripContainer.ResumeLayout(false);
	        this.toolStripContainer.PerformLayout();
	        this.menuStrip1.ResumeLayout(false);
	        this.menuStrip1.PerformLayout();
	        this.ResumeLayout(false);
	        this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStripContainer toolStripContainer;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Label outputLabel;
        private System.Windows.Forms.Label statusLabel;
		private System.Windows.Forms.ToolStripMenuItem showDevToolsToolStripMenuItem;

    }
}