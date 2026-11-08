using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SubScreenCompanion.Models
{
    public class HardwareMetrics : INotifyPropertyChanged
    {
        private string _cpuName = "CPU";
        private float _cpuLoad = 0;
        private float _cpuTemp = 0;
        private float _cpuClock = 0;
        private float _cpuPower = 0;

        private string _gpuName = "GPU";
        private float _gpuLoad = 0;
        private float _gpuTemp = 0;
        private float _gpuMemoryUsed = 0;
        private float _gpuMemoryTotal = 0;
        private float _gpuMemoryPercent = 0;

        private float _ramUsedGb = 0;
        private float _ramTotalGb = 0;
        private float _ramLoad = 0;

        private string _netDownSpeed = "0 KB/s";
        private string _netUpSpeed = "0 KB/s";

        private float _diskLoad = 0;
        private float _diskTemp = 0;

        private string _timeString = "00:00:00";
        private string _dateString = "";

        private bool _isLandscape = true;
        private VisualPreset _currentPreset = VisualPreset.GetPreset("Cyberpunk");

        public string CpuName { get => _cpuName; set => SetField(ref _cpuName, value); }
        public float CpuLoad { get => _cpuLoad; set => SetField(ref _cpuLoad, value); }
        public float CpuTemp { get => _cpuTemp; set => SetField(ref _cpuTemp, value); }
        public float CpuClock { get => _cpuClock; set => SetField(ref _cpuClock, value); }
        public float CpuPower { get => _cpuPower; set => SetField(ref _cpuPower, value); }

        public string GpuName { get => _gpuName; set => SetField(ref _gpuName, value); }
        public float GpuLoad { get => _gpuLoad; set => SetField(ref _gpuLoad, value); }
        public float GpuTemp { get => _gpuTemp; set => SetField(ref _gpuTemp, value); }
        public float GpuMemoryUsed { get => _gpuMemoryUsed; set => SetField(ref _gpuMemoryUsed, value); }
        public float GpuMemoryTotal { get => _gpuMemoryTotal; set => SetField(ref _gpuMemoryTotal, value); }
        public float GpuMemoryPercent { get => _gpuMemoryPercent; set => SetField(ref _gpuMemoryPercent, value); }

        public float RamUsedGb { get => _ramUsedGb; set => SetField(ref _ramUsedGb, value); }
        public float RamTotalGb { get => _ramTotalGb; set => SetField(ref _ramTotalGb, value); }
        public float RamLoad { get => _ramLoad; set => SetField(ref _ramLoad, value); }

        public string NetDownSpeed { get => _netDownSpeed; set => SetField(ref _netDownSpeed, value); }
        public string NetUpSpeed { get => _netUpSpeed; set => SetField(ref _netUpSpeed, value); }

        public float DiskLoad { get => _diskLoad; set => SetField(ref _diskLoad, value); }
        public float DiskTemp { get => _diskTemp; set => SetField(ref _diskTemp, value); }

        public string TimeString { get => _timeString; set => SetField(ref _timeString, value); }
        public string DateString { get => _dateString; set => SetField(ref _dateString, value); }

        public bool IsLandscape { get => _isLandscape; set => SetField(ref _isLandscape, value); }
        public VisualPreset CurrentPreset { get => _currentPreset; set => SetField(ref _currentPreset, value); }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}

