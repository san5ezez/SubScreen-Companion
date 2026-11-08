using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using SubScreenCompanion.Models;
using SubScreenCompanion.Services;
using Application = System.Windows.Application;

namespace SubScreenCompanion
{
    public partial class App : Application
    {
        private AppConfig _config = null!;
        private HardwareMetrics _metrics = null!;
        private DisplayManagerService _displayManager = null!;
        private HardwareMonitorService _hardwareMonitor = null!;
        private MouseBarrierService _mouseBarrier = null!;

        private MainWindow _mainWindow = null!;
        private SettingsWindow _settingsWindow = null!;
        private NotifyIcon _notifyIcon = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // 1. Загрузка конфигурации
                _config = AppConfig.Load();

                // 2. Инициализация моделей и сервисов
                _metrics = new HardwareMetrics
                {
                    CurrentPreset = VisualPreset.GetPreset(_config.PresetName)
                };

                _displayManager = new DisplayManagerService();
                _displayManager.DisplaySettingsChanged += (s, ev) => Dispatcher.Invoke(OnDisplaySettingsChanged);

                _mouseBarrier = new MouseBarrierService
                {
                    IsEnabled = _config.MouseBarrierEnabled
                };

                // 3. Создание главного оверлея
                _mainWindow = new MainWindow(_metrics, _config, _displayManager);
                _mainWindow.Show();

                // 4. Настройка барьера мыши по координатам целевого и основного экранов
                UpdateMouseBarrierBounds();

                // 5. Запуск сбора телеметрии датчиков оборудования
                _hardwareMonitor = new HardwareMonitorService();
                _hardwareMonitor.Start(_metrics, _config.RefreshIntervalMs);

                // 6. Создание окна настроек (на основном мониторе)
                _settingsWindow = new SettingsWindow(_config, _metrics, _displayManager, _mouseBarrier, _mainWindow);

                // 7. Инициализация системного трея
                InitializeSystemTray();

                // 8. Регистрация глобальной горячей клавиши (Ctrl + Alt + M) для разблокировки мыши
                RegisterGlobalHotkey();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Ошибка запуска SubScreen Companion: {ex.Message}\n{ex.StackTrace}",
                    "SubScreen Companion",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
            }
        }

        private const int HOTKEY_ID = 9001;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint VK_M = 0x4D;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private void RegisterGlobalHotkey()
        {
            var helper = new System.Windows.Interop.WindowInteropHelper(_mainWindow);
            if (helper.Handle != IntPtr.Zero)
            {
                RegisterHotKey(helper.Handle, HOTKEY_ID, MOD_CONTROL | MOD_ALT, VK_M);
                var source = System.Windows.Interop.HwndSource.FromHwnd(helper.Handle);
                source?.AddHook(WndProc);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                _mouseBarrier.ToggleTemporaryUnlock();
                bool unlocked = _mouseBarrier.IsTemporarilyUnlocked;
                _notifyIcon.ShowBalloonTip(
                    2000,
                    "SubScreen Companion",
                    unlocked ? "🔓 Мышь временно РАЗБЛОКИРОВАНА на экране корпуса." : "🔒 Мышь ЗАБЛОКИРОВАНА на основном экране.",
                    ToolTipIcon.Info);
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void InitializeSystemTray()
        {
            _notifyIcon = new NotifyIcon
            {
                Text = "SubScreen Companion (Экран корпуса)",
                Visible = true
            };

            // Загрузка иконки из Assets или fallback
            try
            {
                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "icon.ico");
                if (File.Exists(iconPath))
                {
                    _notifyIcon.Icon = new Icon(iconPath);
                }
                else
                {
                    _notifyIcon.Icon = SystemIcons.Application;
                }
            }
            catch
            {
                _notifyIcon.Icon = SystemIcons.Application;
            }

            _notifyIcon.DoubleClick += (s, e) => ShowSettingsWindow();

            // Создание контекстного меню
            var contextMenu = new ContextMenuStrip();

            // 1. Настройки...
            var settingsItem = new ToolStripMenuItem("⚙️ Настройки...", null, (s, e) => ShowSettingsWindow())
            {
                Font = new Font(contextMenu.Font, System.Drawing.FontStyle.Bold)
            };
            contextMenu.Items.Add(settingsItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            // 2. Пресеты (подменю)
            var presetsMenu = new ToolStripMenuItem("🎨 Пресет оформления");
            foreach (var preset in VisualPreset.AvailablePresets.Values)
            {
                var item = new ToolStripMenuItem(preset.DisplayName, null, (s, e) =>
                {
                    _config.PresetName = preset.Id;
                    _metrics.CurrentPreset = preset;
                    _config.Save();
                });
                presetsMenu.DropDownItems.Add(item);
            }
            contextMenu.Items.Add(presetsMenu);

            // 3. Блокировка мыши
            var mouseBarrierItem = new ToolStripMenuItem("🔒 Блокировка мыши на экране корпуса")
            {
                Checked = _mouseBarrier.IsEnabled,
                CheckOnClick = true
            };
            mouseBarrierItem.Click += (s, e) =>
            {
                _config.MouseBarrierEnabled = mouseBarrierItem.Checked;
                _mouseBarrier.IsEnabled = mouseBarrierItem.Checked;
                _config.Save();
            };
            contextMenu.Items.Add(mouseBarrierItem);

            // 4. Позиционирование / Сброс
            var refreshItem = new ToolStripMenuItem("🔄 Обновить позицию экрана", null, (s, e) =>
            {
                OnDisplaySettingsChanged();
            });
            contextMenu.Items.Add(refreshItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            // 5. Выход
            var exitItem = new ToolStripMenuItem("❌ Выход", null, (s, e) =>
            {
                Shutdown();
            });
            contextMenu.Items.Add(exitItem);

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        public void ShowSettingsWindow()
        {
            if (_settingsWindow.WindowState == WindowState.Minimized)
            {
                _settingsWindow.WindowState = WindowState.Normal;
            }
            _settingsWindow.Show();
            _settingsWindow.Activate();
        }

        private void OnDisplaySettingsChanged()
        {
            _mainWindow.ApplyPositionAndOrientation();
            UpdateMouseBarrierBounds();
        }

        private void UpdateMouseBarrierBounds()
        {
            var targetScreen = _displayManager.GetTargetScreen(_config.TargetScreenDeviceName);
            var primaryScreen = Screen.PrimaryScreen != null ? Screen.PrimaryScreen.Bounds : targetScreen.Bounds;
            _mouseBarrier.SetTargetScreen(targetScreen.Bounds, primaryScreen);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                var helper = new System.Windows.Interop.WindowInteropHelper(_mainWindow);
                if (helper.Handle != IntPtr.Zero)
                {
                    UnregisterHotKey(helper.Handle, HOTKEY_ID);
                }
            }
            catch { }

            _notifyIcon?.Dispose();
            _mouseBarrier?.Dispose();
            _hardwareMonitor?.Dispose();
            base.OnExit(e);
        }
    }
}
