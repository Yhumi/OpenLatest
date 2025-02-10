using OpenLatest.Properties;
using System.Windows;

namespace OpenLatest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        NotifyIcon nIcon = new NotifyIcon();
        ContextMenuStrip menuStrip = new();

        public App()
        {
            Settings.Default.Upgrade();

            nIcon.Icon = new Icon(@"Assets/fumo.ico");
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

            if (e.Button == MouseButtons.Right)
            {
                if (nIcon.ContextMenuStrip != null)
                    nIcon.ContextMenuStrip.Show();
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
