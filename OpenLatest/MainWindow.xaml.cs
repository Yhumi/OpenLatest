﻿using Microsoft.Win32;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;

namespace OpenLatest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow_Data Data = new(); 

        public event PropertyChangedEventHandler? PropertyChanged;

        [DllImport("User32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(
            [In] IntPtr hWnd,
            [In] int id,
            [In] uint fsModifiers,
            [In] uint vk);

        [DllImport("User32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(
            [In] IntPtr hWnd,
            [In] int id);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern ushort GlobalAddAtom(string lpString);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern ushort GlobalDeleteAtom(ushort atom);

        private HwndSource _source;
        private readonly ushort HOTKEY_ID;

        public MainWindow()
        {
            InitializeComponent();            
            DataContext = Data;
            ResizeMode = ResizeMode.CanMinimize;

            var hotkey_str = App.Current.GetType().Assembly.FullName;
            HOTKEY_ID = GlobalAddAtom(hotkey_str);

            Properties.Settings.Default.Upgrade();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(HwndHook);
            RegisterHotKey(Data.GetModifierFlags(), Data.VK);
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            _source = null;
            GlobalDeleteAtom(HOTKEY_ID);
            UnregisterHotKey();
            Data.UpdateSettings();
            base.OnClosed(e);
        }

        private void RegisterHotKey(uint modifier, uint vk)
        {
            var helper = new WindowInteropHelper(this);
            if (!RegisterHotKey(helper.Handle, HOTKEY_ID, modifier, vk))
            {
                // handle error
                var err = Marshal.GetLastPInvokeErrorMessage();
                System.Windows.MessageBox.Show($"{err}", "Error.", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    var id = wParam.ToInt32();
                    if (id == HOTKEY_ID)
                    {
                        OpenLatest();
                        handled = true;
                    }
                    break;
                case 0x0112:
                    switch (wParam.ToInt32())
                    {
                        case 0xF020:
                            // Cancel the minimize.
                            handled = true;

                            Visibility = Visibility.Hidden;
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

        private void OpenExeDialog(object sender, RoutedEventArgs e)
        {
            var fileDialog = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "Select Executable",
                Filter = "Executable Files (*.exe)|*.exe",
                Multiselect = false
            };

            if (fileDialog.ShowDialog() == true)
            {
                Data.ExecutablePath = fileDialog.FileName;
            }
        }

        private void ReRegisterHotkey(object sender, RoutedEventArgs e)
        {
            //Unregister current then re-register new
            UnregisterHotKey();

            var modifier = Data.GetModifierFlags();
            RegisterHotKey(modifier, Data.VK);
        }

        private void OpenLatest()
        {
            if (Directory.Exists(Data.Folder))
            {
                var latestItem = new DirectoryInfo(Data.Folder).GetFiles().OrderByDescending(x => x.LastWriteTimeUtc).First();

                try
                {
                    ProcessStartInfo processStartInfo = new ProcessStartInfo(latestItem.FullName);

                    processStartInfo.FileName = (!Data.OpenWithExecutable || String.IsNullOrWhiteSpace(Data.ExecutablePath)) ? latestItem.FullName : Data.ExecutablePath;
                    if (Data.OpenWithExecutable && !String.IsNullOrWhiteSpace(Data.ExecutablePath)) processStartInfo.Arguments = $"\"{latestItem.FullName}\"";

                    processStartInfo.UseShellExecute = true;

                    if (Data.CopyToClipboard) System.Windows.Clipboard.SetDataObject(latestItem.FullName);
                    Process.Start(processStartInfo);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"An error occurred opening {latestItem.FullName}. \n\n{ex.Message}", "Error.", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    public class MainWindow_Data : INotifyPropertyChanged
    {
        private string _folder = string.Empty;
        private string _executablePath = string.Empty;
        private bool _openWithExecutable = false;
        public string Folder { get => _folder; set { _folder = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Folder")); } }
        public string ExecutablePath { get => _executablePath; set { _executablePath = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExecutablePath")); } }
        public bool CopyToClipboard { get; set; } = false;
        public bool OpenWithExecutable { get => _openWithExecutable; set { _openWithExecutable = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("OpenWithExecutable")); } }

        public bool Ctrl { get; set; } = true;
        public bool Alt { get; set; } = true;
        public bool Shift { get; set; } = false;

        public uint VK { get; set; } = 0xBE;

        public CollectionView KeysCombo { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private const uint CtrlModifier = 0x0002;
        private const uint AltModifier = 0x0001;
        private const uint ShiftModifier = 0x0004;

        public MainWindow_Data()
        {
            IList<ComboListData> combo = [];
            foreach (var key in Enum.GetValues(typeof(Keys)).Cast<Keys>())
            {
                combo.Add(new ComboListData(key.ToString(), (uint)key));
            }
            KeysCombo = new(combo);

            //User settings
            try
            {
                Ctrl = Properties.Settings.Default.Ctrl;
                Alt = Properties.Settings.Default.Alt;
                Shift = Properties.Settings.Default.Shift;
                VK = Properties.Settings.Default.VK;

                CopyToClipboard = Properties.Settings.Default.CopyToClipboard;
                Folder = Properties.Settings.Default.Folder;

                OpenWithExecutable = Properties.Settings.Default.OpenWithExecutable;
                ExecutablePath = Properties.Settings.Default.ExecutableFile;
            }
            catch (Exception ex) { }
        }

        public void UpdateSettings()
        {
            Properties.Settings.Default.Ctrl = Ctrl;
            Properties.Settings.Default.Alt = Alt;
            Properties.Settings.Default.Shift = Shift;
            Properties.Settings.Default.VK = VK;

            Properties.Settings.Default.CopyToClipboard = CopyToClipboard;
            Properties.Settings.Default.Folder = Folder;

            Properties.Settings.Default.OpenWithExecutable = OpenWithExecutable;
            Properties.Settings.Default.ExecutableFile = ExecutablePath;

            Properties.Settings.Default.Save();
        }

        public uint GetModifierFlags()
        {
            var modifVal = (uint)0x0000;

            if (Ctrl) modifVal |= CtrlModifier;
            if (Alt) modifVal |= AltModifier;
            if (Shift) modifVal |= ShiftModifier;

            return modifVal;
        }
    }

    public class ComboListData
    {
        public uint Tag { get; set; }
        public string Name { get; set; } = string.Empty;
        public ComboListData(string display, uint vkVal)
        {
            Tag = vkVal;
            Name = display;
        }
    }
}