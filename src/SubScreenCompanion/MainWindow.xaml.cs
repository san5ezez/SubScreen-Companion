using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using SubScreenCompanion.Models;
using SubScreenCompanion.Services;
using SubScreenCompanion.Views;

namespace SubScreenCompanion
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const uint SWP_NOACTIVATE = 0x0010;

        private readonly HardwareMetrics _metrics;
        private readonly AppConfig _config;
        private readonly DisplayManagerService _displayManager;
        private readonly WindowTopmostKeeper _topmostKeeper;

        private readonly LandscapeView _landscapeView;
        private readonly PortraitView _portraitView;

        public MainWindow(HardwareMetrics metrics, AppConfig config, DisplayManagerService displayManager)
        {
            InitializeComponent();
            _metrics = metrics;
            _config = config;
            _displayManager = displayManager;
            _topmostKeeper = new WindowTopmostKeeper(this);

            DataContext = _metrics;

            _landscapeView = new LandscapeView { DataContext = _metrics };
            _portraitView = new PortraitView { DataContext = _metrics };

            Loaded += MainWindow_Loaded;
            SourceInitialized += MainWindow_SourceInitialized;
        }

        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            ApplyPositionAndOrientation();
            if (_config.TopmostEnforce)
            {
                _topmostKeeper.StartEnforcing(2);
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyLayout();
        }

        public void ApplyPositionAndOrientation()
        {
            var targetScreen = _displayManager.GetTargetScreen(_config.TargetScreenDeviceName);
            bool isLandscape = _displayManager.DetermineOrientation(targetScreen, _config.OrientationMode);
            _metrics.IsLandscape = isLandscape;

            ApplyLayout();

            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd != IntPtr.Zero)
            {
                Rectangle bounds = targetScreen.Bounds;
                SetWindowPos(hwnd, HWND_TOPMOST, bounds.X, bounds.Y, bounds.Width, bounds.Height,
                    SWP_SHOWWINDOW | SWP_NOACTIVATE);
            }
        }

        public void ApplyLayout()
        {
            if (_metrics.IsLandscape)
            {
                if (LayoutHost.Content != _landscapeView)
                {
                    LayoutHost.Content = _landscapeView;
                }
            }
            else
            {
                if (LayoutHost.Content != _portraitView)
                {
                    LayoutHost.Content = _portraitView;
                }
            }
        }
    }
}
