using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SubScreenCompanion.Services
{
    public class MouseBarrierService : IDisposable
    {
        private const int WH_MOUSE_LL = 14;
        private const int WM_MOUSEMOVE = 0x0200;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr _hookId = IntPtr.Zero;
        private LowLevelMouseProc? _proc;
        private Rectangle _blockedScreenBounds;
        private Rectangle _safeScreenBounds;
        private bool _isEnabled = true;
        private bool _isTemporarilyUnlocked = false;
        private bool _isAdjustingPos = false;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                if (!_isEnabled)
                {
                    Stop();
                }
                else
                {
                    Start();
                }
            }
        }

        public bool IsTemporarilyUnlocked
        {
            get => _isTemporarilyUnlocked;
            set => _isTemporarilyUnlocked = value;
        }

        public void SetTargetScreen(Rectangle blockedBounds, Rectangle primarySafeBounds)
        {
            _blockedScreenBounds = blockedBounds;
            _safeScreenBounds = primarySafeBounds;
        }

        public void Start()
        {
            if (_hookId != IntPtr.Zero) return;

            _proc = HookCallback;
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            if (curModule?.ModuleName != null)
            {
                _hookId = SetWindowsHookEx(WH_MOUSE_LL, _proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        public void Stop()
        {
            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
        }

        public void ToggleTemporaryUnlock()
        {
            _isTemporarilyUnlocked = !_isTemporarilyUnlocked;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_MOUSEMOVE)
            {
                if (_isEnabled && !_isTemporarilyUnlocked && !_isAdjustingPos)
                {
                    var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                    int x = hookStruct.pt.x;
                    int y = hookStruct.pt.y;

                    // Проверяем, попал ли курсор в координаты экрана корпуса
                    if (_blockedScreenBounds.Contains(x, y))
                    {
                        _isAdjustingPos = true;
                        try
                        {
                            // Вычисляем ближайшую точку на безопасном мониторе
                            int safeX = x;
                            int safeY = y;

                            // Если суб-экран находится справа от основного
                            if (_blockedScreenBounds.Left >= _safeScreenBounds.Right)
                            {
                                safeX = _safeScreenBounds.Right - 1;
                                safeY = Math.Clamp(y, _safeScreenBounds.Top, _safeScreenBounds.Bottom - 1);
                            }
                            // Если суб-экран находится слева от основного
                            else if (_blockedScreenBounds.Right <= _safeScreenBounds.Left)
                            {
                                safeX = _safeScreenBounds.Left + 1;
                                safeY = Math.Clamp(y, _safeScreenBounds.Top, _safeScreenBounds.Bottom - 1);
                            }
                            // Если суб-экран находится сверху
                            else if (_blockedScreenBounds.Bottom <= _safeScreenBounds.Top)
                            {
                                safeX = Math.Clamp(x, _safeScreenBounds.Left, _safeScreenBounds.Right - 1);
                                safeY = _safeScreenBounds.Top + 1;
                            }
                            // Если суб-экран находится снизу
                            else if (_blockedScreenBounds.Top >= _safeScreenBounds.Bottom)
                            {
                                safeX = Math.Clamp(x, _safeScreenBounds.Left, _safeScreenBounds.Right - 1);
                                safeY = _safeScreenBounds.Bottom - 1;
                            }
                            else
                            {
                                // Универсальный возврат в центр безопасного монитора
                                safeX = _safeScreenBounds.Left + _safeScreenBounds.Width / 2;
                                safeY = _safeScreenBounds.Top + _safeScreenBounds.Height / 2;
                            }

                            SetCursorPos(safeX, safeY);
                            // Блокируем дальнейшую доставку события
                            return (IntPtr)1;
                        }
                        finally
                        {
                            _isAdjustingPos = false;
                        }
                    }
                }
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}
