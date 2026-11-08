using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SubScreenCompanion.Services
{
    public class ScreenInfo
    {
        public string DeviceName { get; set; } = string.Empty;
        public string FriendlyName { get; set; } = string.Empty;
        public Rectangle Bounds { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsLandscape => Bounds.Width >= Bounds.Height;
        public int Width => Bounds.Width;
        public int Height => Bounds.Height;

        public override string ToString()
        {
            var orientation = IsLandscape ? "Горизонтальный" : "Вертикальный";
            var primary = IsPrimary ? " (Основной)" : " (Дополнительный)";
            return $"{FriendlyName}{primary} — {Width}x{Height} [{orientation}]";
        }
    }

    public class DisplayManagerService
    {
        public event EventHandler? DisplaySettingsChanged;

        public DisplayManagerService()
        {
            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
        }

        private void OnDisplaySettingsChanged(object? sender, EventArgs e)
        {
            DisplaySettingsChanged?.Invoke(this, EventArgs.Empty);
        }

        public List<ScreenInfo> GetAllScreens()
        {
            var list = new List<ScreenInfo>();
            int index = 1;
            foreach (var screen in Screen.AllScreens)
            {
                list.Add(new ScreenInfo
                {
                    DeviceName = screen.DeviceName,
                    FriendlyName = $"Экран {index++}",
                    Bounds = screen.Bounds,
                    IsPrimary = screen.Primary
                });
            }
            return list;
        }

        public ScreenInfo GetTargetScreen(string targetDeviceName)
        {
            var screens = GetAllScreens();
            if (screens.Count == 0)
            {
                return new ScreenInfo
                {
                    DeviceName = "Default",
                    FriendlyName = "Экран 1",
                    Bounds = new Rectangle(0, 0, 1920, 1080),
                    IsPrimary = true
                };
            }

            // 1. Попытка найти по сохраненному имени устройства
            if (!string.IsNullOrEmpty(targetDeviceName) && targetDeviceName != "Auto")
            {
                var found = screens.FirstOrDefault(s => s.DeviceName.Equals(targetDeviceName, StringComparison.OrdinalIgnoreCase));
                if (found != null) return found;
            }

            // 2. В режиме "Auto": ищем первый неосновной монитор (второй экран / экран в корпусе)
            var secondary = screens.FirstOrDefault(s => !s.IsPrimary);
            if (secondary != null)
            {
                return secondary;
            }

            // 3. Если только 1 монитор, возвращаем его
            return screens[0];
        }

        public bool DetermineOrientation(ScreenInfo screen, string orientationMode)
        {
            if (orientationMode == "Landscape") return true;
            if (orientationMode == "Portrait") return false;
            // Auto
            return screen.IsLandscape;
        }
    }
}
