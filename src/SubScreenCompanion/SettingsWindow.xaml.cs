using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SubScreenCompanion.Models;
using SubScreenCompanion.Services;
using RadioButton = System.Windows.Controls.RadioButton;
using MessageBox = System.Windows.MessageBox;

namespace SubScreenCompanion
{
    public partial class SettingsWindow : Window
    {
        private readonly AppConfig _config;
        private readonly HardwareMetrics _metrics;
        private readonly DisplayManagerService _displayManager;
        private readonly MouseBarrierService _mouseBarrier;
        private readonly MainWindow _mainWindow;

        private List<ScreenInfo> _screens = new();
        private bool _isInitializing = true;

        public SettingsWindow(
            AppConfig config,
            HardwareMetrics metrics,
            DisplayManagerService displayManager,
            MouseBarrierService mouseBarrier,
            MainWindow mainWindow)
        {
            InitializeComponent();
            _config = config;
            _metrics = metrics;
            _displayManager = displayManager;
            _mouseBarrier = mouseBarrier;
            _mainWindow = mainWindow;

            Loaded += SettingsWindow_Loaded;
            Closing += SettingsWindow_Closing;
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadScreens();
            LoadConfigToUI();
            _isInitializing = false;
        }

        private void SettingsWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Не закрываем приложение, а просто прячем окно настроек
            e.Cancel = true;
            Hide();
        }

        private void LoadScreens()
        {
            _screens = _displayManager.GetAllScreens();
            CbScreens.ItemsSource = null;
            CbScreens.ItemsSource = _screens;

            var target = _screens.FirstOrDefault(s => s.DeviceName.Equals(_config.TargetScreenDeviceName, StringComparison.OrdinalIgnoreCase))
                         ?? _screens.FirstOrDefault(s => !s.IsPrimary)
                         ?? _screens.FirstOrDefault();

            if (target != null)
            {
                CbScreens.SelectedItem = target;
            }
        }

        private void LoadConfigToUI()
        {
            // Пресет
            switch (_config.PresetName)
            {
                case "Stealth": RbPresetStealth.IsChecked = true; break;
                case "GamingRed": RbPresetGamingRed.IsChecked = true; break;
                case "WhiteSnow": RbPresetWhiteSnow.IsChecked = true; break;
                case "AmberRetro": RbPresetAmberRetro.IsChecked = true; break;
                default: RbPresetCyberpunk.IsChecked = true; break;
            }

            // Ориентация
            switch (_config.OrientationMode)
            {
                case "Landscape": RbOrientLandscape.IsChecked = true; break;
                case "Portrait": RbOrientPortrait.IsChecked = true; break;
                default: RbOrientAuto.IsChecked = true; break;
            }

            ChkMouseBarrier.IsChecked = _config.MouseBarrierEnabled;
            ChkAutostart.IsChecked = AutostartService.IsAutostartEnabled();
            ChkTopmost.IsChecked = _config.TopmostEnforce;
        }

        private void OnPresetSelected(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;

            if (sender is RadioButton rb && rb.Tag is string presetKey)
            {
                _config.PresetName = presetKey;
                _metrics.CurrentPreset = VisualPreset.GetPreset(presetKey);
            }
        }

        private void OnScreenSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;

            if (CbScreens.SelectedItem is ScreenInfo selectedScreen)
            {
                _config.TargetScreenDeviceName = selectedScreen.DeviceName;
            }
        }

        private void OnRefreshScreensClick(object sender, RoutedEventArgs e)
        {
            LoadScreens();
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            // Сохранение ориентации
            if (RbOrientLandscape.IsChecked == true) _config.OrientationMode = "Landscape";
            else if (RbOrientPortrait.IsChecked == true) _config.OrientationMode = "Portrait";
            else _config.OrientationMode = "Auto";

            // Сохранение мышиного барьера
            _config.MouseBarrierEnabled = ChkMouseBarrier.IsChecked ?? true;
            _mouseBarrier.IsEnabled = _config.MouseBarrierEnabled;

            // Настройка автозапуска Windows
            bool autostart = ChkAutostart.IsChecked ?? false;
            _config.AutostartEnabled = autostart;
            AutostartService.SetAutostart(autostart);

            _config.TopmostEnforce = ChkTopmost.IsChecked ?? true;

            // Обновление барьера мыши и позиции окна
            var targetScreen = _displayManager.GetTargetScreen(_config.TargetScreenDeviceName);
            var primaryScreen = _screens.FirstOrDefault(s => s.IsPrimary) ?? targetScreen;
            _mouseBarrier.SetTargetScreen(targetScreen.Bounds, primaryScreen.Bounds);

            _mainWindow.ApplyPositionAndOrientation();
            _config.Save();

            MessageBox.Show(
                "Настройки успешно сохранены и применены к экрану корпуса!",
                "SubScreen Companion",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Hide();
        }

        private void OnHideClick(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
