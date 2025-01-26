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
using System.Windows.Media.Animation;
using CefSharp;

namespace OverlayEnabler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string[] subURLs;
        private const int WM_HOTKEY = 0x0312;
        private const int MOD_CONTROL = 0x0002;
        private const int VK_E = 0x45;
        private const int VK_R = 0x52;
        private const int VK_INSERT = 0x2D;
        private const int VK_F1 = 0x70;
        private const int HOTKEY_ID_EXIT = 9000;
        private const int HOTKEY_ID_SHOWHIDE = 9001;
        private const int HOTKEY_ID_RELOAD = 9002;
        private const int HOTKEY_ID_DESTROY_SUBBROWSER = 9003;
        private const int HOTKEY_ID_BASE_SUBURL = 9004;
        
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
            if (commandLineArgs.Contains("/help"))
            {
                ShowHelp();
                Application.Current.Shutdown();
                return;
            }
            if (commandLineArgs.Length > 1)
            {
                Browser.Address = commandLineArgs[1];
                if (commandLineArgs.Length > 2)
                {
                    subURLs = commandLineArgs.Skip(2).Take(5).ToArray();
                    RegisterSubUrlHotkeys(helper);
                    RegisterHotKey(helper, HOTKEY_ID_DESTROY_SUBBROWSER, MOD_CONTROL, VK_F1); // Destroy SubBrowser hotkey: Ctrl + F1
                }
            } 
            else
            {
                ExecuteStartBat();
                Application.Current.Shutdown();
                return;
            }
            // Register the hotkey
            RegisterHotKey(helper, HOTKEY_ID_EXIT, MOD_CONTROL, VK_E); // Exit hotkey: Ctrl + E
            RegisterHotKey(helper, HOTKEY_ID_SHOWHIDE, 0, VK_INSERT); // Show/hide hotkey: Insert
            RegisterHotKey(helper, HOTKEY_ID_RELOAD, MOD_CONTROL, VK_R); // Reload hotkey: Ctrl + R

            // Add the hook to handle the hotkey
            HwndSource source = HwndSource.FromHwnd(helper);
            source.AddHook(HwndHook);
        }
        private void RegisterSubUrlHotkeys(IntPtr helper)
        {
            for (int i = 0; i < subURLs.Length && i < 5; i++)
            {
                RegisterHotKey(helper, HOTKEY_ID_BASE_SUBURL + i, MOD_CONTROL, (uint)(0x71 + i)); // F2 is 0x71, F3 is 0x72, ..., F6 is 0x75
            }
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
                else if (hotkeyId == HOTKEY_ID_RELOAD)
                {
                    Browser.Reload();
                    handled = true;
                }
                else if (hotkeyId >= HOTKEY_ID_BASE_SUBURL && hotkeyId < HOTKEY_ID_BASE_SUBURL + subURLs.Length)
                {
                    SubBrowser.Opacity = 1.0;
                    MessageBox.Show(SubBrowser.Opacity.ToString(), "Help", MessageBoxButton.OK, MessageBoxImage.Information);
                    int index = hotkeyId - HOTKEY_ID_BASE_SUBURL;
                    SubBrowser.Address = subURLs[index];
                    handled = true;
                }
                else if (hotkeyId == HOTKEY_ID_DESTROY_SUBBROWSER)
                {
                    DestroySubBrowser();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }
        private void DestroySubBrowser()
        {
            if (SubBrowser != null)
            {
                DoubleAnimation fadeAnimation = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.5,
                    Duration = new Duration(TimeSpan.FromSeconds(0.3))
                };
                fadeAnimation.Completed += (s, e) =>
                {
                    SubBrowser.Address = "about:blank";
                    SubBrowser.Opacity = 1.0;
                    MessageBox.Show(SubBrowser.Opacity.ToString(), "Help", MessageBoxButton.OK, MessageBoxImage.Information);
                };
                SubBrowser.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);
            }
        }
        private void ShowHelp()
        {
            string helpText = "OverlayEnabler Help:\n\n" +
                              "Usage: OverlayEnabler.exe [URL] [subURL] [options]\n\n" +
                              "Options:\n" +
                              "/help\t\tShow this help message\n\n" +
                              "Hotkeys:\n" +
                              "Ctrl + E\t\tExit the application\n" +
                              "Insert\t\tToggle show/hide the window\n" +
                              "Ctrl + R\t\tReload the page\n" +
                              "Ctrl + F2-F6\tDisplay address to corresponding subURL\n" +
                              "Ctrl + F1\tHide subURL";
            MessageBox.Show(helpText, "Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void ExecuteStartBat()
        {
            string batFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "start.bat");
            if (System.IO.File.Exists(batFilePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = batFilePath,
                    UseShellExecute = true,
                    CreateNoWindow = true
                });
            }
            else
            {
                MessageBox.Show("No URL provided, please run start.bat in application directory or run 'OverlayEnabler.exe /help' for more info", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ToggleWindowVisibility()
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(0.3))
            };

            if (this.Visibility == Visibility.Visible)
            {
                fadeAnimation.From = 1.0;
                fadeAnimation.To = 0.0;
                fadeAnimation.Completed += (s, e) => this.Hide();
                this.BeginAnimation(Window.OpacityProperty, fadeAnimation);
            }
            else
            {
                this.Show();
                fadeAnimation.From = 0.0;
                fadeAnimation.To = 1.0;
                this.BeginAnimation(Window.OpacityProperty, fadeAnimation);
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            // Unregister the hotkey
            UnregisterHotKey(new WindowInteropHelper(this).Handle, HOTKEY_ID_EXIT);
            UnregisterHotKey(new WindowInteropHelper(this).Handle, HOTKEY_ID_SHOWHIDE);
            for (int i = 0; i < subURLs.Length && i < 5; i++)
            {
                UnregisterHotKey(new WindowInteropHelper(this).Handle, HOTKEY_ID_BASE_SUBURL + i);
            }
            base.OnClosed(e);
        }

        private void Window_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var window = (Window)sender;
            window.Topmost = true;
        }
    }
}
