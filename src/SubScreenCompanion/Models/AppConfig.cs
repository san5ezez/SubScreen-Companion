using System;
using System.IO;
using System.Text.Json;

namespace SubScreenCompanion.Models
{
    public class AppConfig
    {
        public string TargetScreenDeviceName { get; set; } = "Auto";
        public string OrientationMode { get; set; } = "Auto"; // "Auto", "Landscape", "Portrait"
        public string PresetName { get; set; } = "Cyberpunk"; // "Cyberpunk", "Stealth", "GamingRed", "WhiteSnow", "AmberRetro"
        public string LayoutMode { get; set; } = "FullStats"; // "FullStats", "CompactStats", "ClockFocus"
        
        public bool MouseBarrierEnabled { get; set; } = true;
        public string UnlockHotkey { get; set; } = "Ctrl+Alt+M";
        public bool AutostartEnabled { get; set; } = true;
        public bool TopmostEnforce { get; set; } = true;
        public int RefreshIntervalMs { get; set; } = 1000;

        public bool ShowClock { get; set; } = true;
        public bool ShowCpu { get; set; } = true;
        public bool ShowGpu { get; set; } = true;
        public bool ShowRam { get; set; } = true;
        public bool ShowNetwork { get; set; } = true;
        public bool ShowStorage { get; set; } = false;

        private static readonly string ConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SubScreenCompanion");
        private static readonly string ConfigFilePath = Path.Combine(ConfigDir, "config.json");

        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    var json = File.ReadAllText(ConfigFilePath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);
                    if (config != null) return config;
                }
            }
            catch
            {
                // Fallback to default
            }

            var defaultConfig = new AppConfig();
            defaultConfig.Save();
            return defaultConfig;
        }

        public void Save()
        {
            try
            {
                if (!Directory.Exists(ConfigDir))
                {
                    Directory.CreateDirectory(ConfigDir);
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save config: {ex.Message}");
            }
        }
    }
}

