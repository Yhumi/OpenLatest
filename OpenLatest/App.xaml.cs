﻿using OpenLatest.Properties;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace OpenLatest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        NotifyIcon nIcon = new NotifyIcon();
        ContextMenuStrip menuStrip = new();

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern ushort GlobalFindAtom(string lpString);

        public App()
        {
            var hotkey_str = GetType().Assembly.FullName;
            var atom = GlobalFindAtom(hotkey_str);
            if (atom > 0)
            {
                System.Windows.MessageBox.Show("Already running an instance of this application.", "Fatal.", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
                return;
            }

            Settings.Default.Upgrade();

            nIcon.Icon = new Icon(@"Assets/fumo.ico");
            nIcon.Text = "Open Latest";
            nIcon.Visible = true;
            //nIcon.ShowBalloonTip(5000, "Running in background.", "Open Latest will continue to function.", ToolTipIcon.Info);

            menuStrip.Items.Add("For Frey", null, titleText);
            menuStrip.Items.Add("-");
            menuStrip.Items.Add("Exit", null, exitApp);

            nIcon.ContextMenuStrip = menuStrip;
            nIcon.MouseClick += nIcon_Click;
        }

        void nIcon_Click(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //events comes here# 
                MainWindow.Visibility = Visibility.Visible;
                MainWindow.WindowState = WindowState.Normal;
            }
        }

        void titleText(object? sender, EventArgs e)
        {
            nIcon.ShowBalloonTip(5000, "Title", "Text", ToolTipIcon.Info);
        }

        void exitApp(object? sender, EventArgs e)
        {
            Current.Shutdown();
        }
    }
}
