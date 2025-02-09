﻿using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenLatest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow_Data Data = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        [DllImport("User32.dll")]
        private static extern bool RegisterHotKey(
            [In] IntPtr hWnd,
            [In] int id,
            [In] uint fsModifiers,
            [In] uint vk);

        [DllImport("User32.dll")]
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        private HwndSource _source;
        private const int HOTKEY_ID = 9000;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = Data;
            ResizeMode = ResizeMode.CanMinimize;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            RegisterHotKey();
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            _source = null;
            UnregisterHotKey();
            base.OnClosed(e);
        }

        private void RegisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            if (!RegisterHotKey(helper.Handle, HOTKEY_ID, 0x0003, 0xBE))
            {
                // handle error
                MessageBox.Show("Fatal Error.", "Error.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UnregisterHotKey()
        {
            var helper = new WindowInteropHelper(this);
            UnregisterHotKey(helper.Handle, HOTKEY_ID);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            OpenLatest();
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        private void OpenFolderDialog(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog()
            {
                Title = "Select Folder",
            };

            if (folderDialog.ShowDialog() == true)
            {
                Data.Folder = folderDialog.FolderName;
            }
        }

        private void OpenLatest()
        {
            if (Directory.Exists(Data.Folder))
            {
                
                var latestItem = new DirectoryInfo(Data.Folder).GetFiles().OrderByDescending(x => x.LastWriteTimeUtc).First();

                try
                {
                    ProcessStartInfo processStartInfo = new ProcessStartInfo(latestItem.FullName);
                    processStartInfo.UseShellExecute = true;

                    Process.Start(processStartInfo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred opening {latestItem.FullName}. \n\n{ex.Message}", "Error.", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    public class MainWindow_Data : INotifyPropertyChanged
    {
        private string _folder = string.Empty;
        public string Folder { get => _folder; set { _folder = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Folder")); } }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}