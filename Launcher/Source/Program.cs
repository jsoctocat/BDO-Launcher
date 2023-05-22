using CefSharp.WinForms;
using System;
using System.IO;
using System.Windows.Forms;
using CefSharp;
using Launcher.Forms;

namespace Launcher.Source
{

    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            CefSharpSettings.SubprocessExitIfParentProcessClosed = true;
            var settings = new CefSettings()
            {
                UserAgent = "BLACKDESERT",
                AcceptLanguageList = "en-US",
                //By default CefSharp will use an in-memory cache, you need to specify a Cache Folder to persist data
                CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
            };
            
            // Only initialize Cef once, this is a framework limitation
            if(!Cef.IsInitialized) Cef.Initialize(settings,true, browserProcessHandler: null);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

    }

}
