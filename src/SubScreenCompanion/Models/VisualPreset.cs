using System.Collections.Generic;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace SubScreenCompanion.Models
{
    public class VisualPreset
    {
        public string Id { get; set; } = "Cyberpunk";
        public string DisplayName { get; set; } = "Cyberpunk Neon";
        public string Description { get; set; } = "Неоновый бирюзовый и пурпурный акценты";

        public Brush BackgroundBrush { get; set; } = (SolidColorBrush)new BrushConverter().ConvertFromString("#0B0E14")!;
        public Brush CardBrush { get; set; } = (SolidColorBrush)new BrushConverter().ConvertFromString("#141A24")!;
        public Brush CardBorderBrush { get; set; } = (SolidColorBrush)new BrushConverter().ConvertFromString("#00F0FF33")!;
        public Brush AccentBrush { get; set; } = (SolidColorBrush)new BrushConverter().ConvertFromString("#00F0FF")!;
        public Brush SecondaryAccentBrush { get; set; } = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF007F")!;
        public Brush TextBrush { get; set; } = (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFF")!;
        public Brush SubtextBrush { get; set; } = (SolidColorBrush)new BrushConverter().ConvertFromString("#8293A6")!;
        public Brush GaugeTrackBrush { get; set; } = (SolidColorBrush)new BrushConverter().ConvertFromString("#1D2736")!;
        public bool EnableGlow { get; set; } = true;

        public static Dictionary<string, VisualPreset> AvailablePresets { get; } = new()
        {
            ["Cyberpunk"] = new VisualPreset
            {
                Id = "Cyberpunk",
                DisplayName = "Cyberpunk Neon",
                Description = "Неоновый циан и фуксия, глубокий космос",
                BackgroundBrush = BrushesFromHex("#090C12"),
                CardBrush = BrushesFromHex("#111823"),
                CardBorderBrush = BrushesFromHex("#00F0FF44"),
                AccentBrush = BrushesFromHex("#00F0FF"),
                SecondaryAccentBrush = BrushesFromHex("#FF007F"),
                TextBrush = BrushesFromHex("#F0F6FC"),
                SubtextBrush = BrushesFromHex("#70859E"),
                GaugeTrackBrush = BrushesFromHex("#192435"),
                EnableGlow = true
            },
            ["Stealth"] = new VisualPreset
            {
                Id = "Stealth",
                DisplayName = "Stealth Dark",
                Description = "Строгий темный минимализм с платиновыми акцентами",
                BackgroundBrush = BrushesFromHex("#101012"),
                CardBrush = BrushesFromHex("#1A1A1E"),
                CardBorderBrush = BrushesFromHex("#2C2C34"),
                AccentBrush = BrushesFromHex("#A4B1C2"),
                SecondaryAccentBrush = BrushesFromHex("#5E6D82"),
                TextBrush = BrushesFromHex("#E6EDF3"),
                SubtextBrush = BrushesFromHex("#79828E"),
                GaugeTrackBrush = BrushesFromHex("#25252C"),
                EnableGlow = false
            },
            ["GamingRed"] = new VisualPreset
            {
                Id = "GamingRed",
                DisplayName = "ROG / Gaming Red",
                Description = "Агрессивный черно-красный стиль геймерской сборки",
                BackgroundBrush = BrushesFromHex("#0D0304"),
                CardBrush = BrushesFromHex("#1C080B"),
                CardBorderBrush = BrushesFromHex("#FF2A4D44"),
                AccentBrush = BrushesFromHex("#FF2A4D"),
                SecondaryAccentBrush = BrushesFromHex("#FF7B00"),
                TextBrush = BrushesFromHex("#FFFFFF"),
                SubtextBrush = BrushesFromHex("#A8606B"),
                GaugeTrackBrush = BrushesFromHex("#310F15"),
                EnableGlow = true
            },
            ["WhiteSnow"] = new VisualPreset
            {
                Id = "WhiteSnow",
                DisplayName = "Snow / Clean White",
                Description = "Светлая серебристая тема для белых корпусов (Jonsbo D31 White)",
                BackgroundBrush = BrushesFromHex("#E8ECF2"),
                CardBrush = BrushesFromHex("#FFFFFF"),
                CardBorderBrush = BrushesFromHex("#D1D9E6"),
                AccentBrush = BrushesFromHex("#2563EB"),
                SecondaryAccentBrush = BrushesFromHex("#059669"),
                TextBrush = BrushesFromHex("#0F172A"),
                SubtextBrush = BrushesFromHex("#64748B"),
                GaugeTrackBrush = BrushesFromHex("#E2E8F0"),
                EnableGlow = false
            },
            ["AmberRetro"] = new VisualPreset
            {
                Id = "AmberRetro",
                DisplayName = "Amber Synthwave / Sci-Fi",
                Description = "Теплый янтарный научно-фантастический терминал",
                BackgroundBrush = BrushesFromHex("#120C04"),
                CardBrush = BrushesFromHex("#1F1508"),
                CardBorderBrush = BrushesFromHex("#FFA50044"),
                AccentBrush = BrushesFromHex("#FF9E1B"),
                SecondaryAccentBrush = BrushesFromHex("#FFD000"),
                TextBrush = BrushesFromHex("#FFF4DB"),
                SubtextBrush = BrushesFromHex("#A87E4B"),
                GaugeTrackBrush = BrushesFromHex("#36250F"),
                EnableGlow = true
            }
        };

        public static VisualPreset GetPreset(string key)
        {
            if (AvailablePresets.TryGetValue(key, out var preset))
            {
                return preset;
            }
            return AvailablePresets["Cyberpunk"];
        }

        private static SolidColorBrush BrushesFromHex(string hex)
        {
            var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(hex)!;
            brush.Freeze();
            return brush;
        }
    }
}
