using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace SubScreenCompanion.Services
{
    public class WindowTopmostKeeper
    {
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private readonly Window _window;
        private DispatcherTimer? _enforceTimer;

        public WindowTopmostKeeper(Window window)
        {
            _window = window;
        }

        public void StartEnforcing(int intervalSeconds = 2)
        {
            _enforceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(intervalSeconds)
            };
            _enforceTimer.Tick += (s, e) => EnsureTopmost();
            _enforceTimer.Start();
        }

        public void Stop()
        {
            _enforceTimer?.Stop();
        }

        public void EnsureTopmost()
        {
            try
            {
                if (!_window.IsVisible) return;

                var helper = new WindowInteropHelper(_window);
                if (helper.Handle != IntPtr.Zero)
                {
                    SetWindowPos(helper.Handle, HWND_TOPMOST, 0, 0, 0, 0,
                        SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
                }
            }
            catch { }
        }
    }
}

