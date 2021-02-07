using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Diagnostics;
using Microsoft.Web.WebView2.Core;
using System.Runtime.InteropServices;
using System.Threading;

namespace LocalBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string pathDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string port = "";
        Process staticServerProcess = null;

        public MainWindow()
        {
            InitializeComponent();

            StartServer(ref staticServerProcess, pathDirectory);

            Thread.Sleep(7000);

            port = File.ReadAllText($"{pathDirectory}/Logs.txt");

            MessageBox.Show(port);

            File.WriteAllText($"{pathDirectory}/Logs.txt", "");

            Application.Current.MainWindow.Closing += new CancelEventHandler(Window_Closed);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //if (webView != null && webView.CoreWebView2 != null)
            //{
            //    webView.CoreWebView2.Navigate(addressBar.Text);
            //}

            try
            {
                OpenUrl($"http://localhost:{port}");
            }
            catch (Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        public static void StartServer(ref Process staticServerProcess, string pathDirectory)
        {
            try
            {
                using (staticServerProcess = new Process())
                {
                    staticServerProcess.StartInfo.UseShellExecute = true;
                    staticServerProcess.StartInfo.FileName = $"{pathDirectory}/public/StaticServer.exe";
                    staticServerProcess.StartInfo.CreateNoWindow = false;
                    staticServerProcess.Start();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void RunCmd(string strCmdText)
        {
            Process.Start("CMD.exe", "/c " + strCmdText);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // show the message box here and collect the result
            var result = MessageBox.Show("Are you sure you want to close the PSJ Documentation?", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            if (result == MessageBoxResult.OK)
            {
                RunCmd("taskkill /F /im StaticServer.exe");

                //App.Current.Shutdown();
                //this.Close();
            }
            else
            {
            // if you want to stop it, set e.Cancel = true
                e.Cancel = true;
            }
            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //browser.Dispose();
            //engine.Dispose();
        }

        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
