using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;

namespace OverlayEnabler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WM_HOTKEY = 0x0312;
        private const int MOD_CONTROL = 0x0002;
        private const int VK_E = 0x45;
        private const int VK_INSERT = 0x2D;
        private const int HOTKEY_ID_EXIT = 9000;
        private const int HOTKEY_ID_SHOWHIDE = 9001;
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        private const int GWL_EX_STYLE = -20;
        private const int WS_EX_APPWINDOW = 0x00040000, WS_EX_TOOLWINDOW = 0x00000080;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public MainWindow()
        {
            InitializeComponent();
        }
        //Form loaded event handler
        void FormLoaded(object sender, RoutedEventArgs args)
        {
            //Variable to hold the handle for the form
            var helper = new WindowInteropHelper(this).Handle;
            //Performing some magic to hide the form from Alt+Tab
            SetWindowLong(helper, GWL_EX_STYLE, (GetWindowLong(helper, GWL_EX_STYLE) | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length > 1)
            {
                Browser.Address = commandLineArgs[1];
            }
            else
            {
                MessageBox.Show("No URL provided. Please run the application with a URL parameter.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            // Register the hotkey
            RegisterHotKey(helper, HOTKEY_ID_EXIT, MOD_CONTROL, VK_E);
            RegisterHotKey(helper, HOTKEY_ID_SHOWHIDE, 0, VK_INSERT);


            // Add the hook to handle the hotkey
            HwndSource source = HwndSource.FromHwnd(helper);
            source.AddHook(HwndHook);
        }
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int hotkeyId = wParam.ToInt32();
                if (hotkeyId == HOTKEY_ID_EXIT)
                {
                    Application.Current.Shutdown();
                    handled = true;
                }
                else if (hotkeyId == HOTKEY_ID_SHOWHIDE)
                {
                    ToggleWindowVisibility();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }
        private void ToggleWindowVisibility()
        {
            if (this.Visibility == Visibility.Visible)
            {
                this.Hide();
            }
            else
            {
                this.Show();
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            // Unregister the hotkey
            UnregisterHotKey(new WindowInteropHelper(this).Handle, HOTKEY_ID_EXIT);
            UnregisterHotKey(new WindowInteropHelper(this).Handle, HOTKEY_ID_SHOWHIDE);
            base.OnClosed(e);
        }

        private void Window_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var window = (Window)sender;
            window.Topmost = true;
        }
    }
}
