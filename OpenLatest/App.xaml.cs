using System.Windows;

namespace OpenLatest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        NotifyIcon nIcon = new NotifyIcon();
        public App()
        {
            nIcon.Icon = new Icon(@"Assets/fumo.ico");
            nIcon.Visible = true;
            nIcon.ShowBalloonTip(5000, "Running in background.", "Open Latest will continue to function.", ToolTipIcon.Info);
            nIcon.Click += nIcon_Click;
        }

        void nIcon_Click(object? sender, EventArgs e)
        {
            //events comes here
            MainWindow.Visibility = Visibility.Visible;
            MainWindow.WindowState = WindowState.Normal;
        }
    }
}
