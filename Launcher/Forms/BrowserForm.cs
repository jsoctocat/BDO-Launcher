// Copyright © 2010-2015 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using System;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using Launcher.Source;

namespace Launcher.Forms
{
    public partial class BrowserForm : Form
    {
        private static ChromiumWebBrowser _browser;
        private static BrowserForm _browserForm;
        
        // Constructor is 'protected'
        private BrowserForm() { }

        public static BrowserForm BrowserFormInstance()
        {
            // Uses lazy initialization.
            // Note: this is not thread safe.
            if (_browserForm == null)
            {
                _browserForm = new BrowserForm();
                _browserForm.InitializeComponent();
                _browserForm.toolStripContainer.ContentPanel.Controls.Add(_browser);
            }
            return _browserForm;
        }
        
        public static ChromiumWebBrowser BrowserInstance()
        {
            // Uses lazy initialization.
            // Note: this is not thread safe.
            if (_browser == null)
            {
                _browser = new ChromiumWebBrowser("Initialize");
                var browserSettings = new BrowserSettings()
                {
                    WindowlessFrameRate = 1,
                    JavascriptAccessClipboard = CefState.Disabled,
                    JavascriptCloseWindows = CefState.Disabled,
                    JavascriptDomPaste = CefState.Disabled
                };
                _browser.BrowserSettings = browserSettings;
                _browser.RequestHandler = new CustomRequestHandler();
            }
            return _browser;
        }

        public static void StopLoading()
        {
            if(_browser != null && _browser.IsBrowserInitialized)
                _browser.Stop();
        }

        private void ExitMenuItemClick(object sender, EventArgs e)
        {
            _browser.Dispose();
            Cef.Shutdown();
            Close();
        }

        private void ShowDevToolsMenuItemClick(object sender, EventArgs e)
        {
            _browser.ShowDevTools();
        }
    }
}
