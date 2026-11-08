using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;
using SubScreenCompanion.Models;

namespace SubScreenCompanion.Services
{
    public class HardwareMonitorService : IDisposable
    {
        private Computer? _computer;
        private CancellationTokenSource? _cts;
        private bool _isInitialized = false;

        // Fallback network tracking
        private long _prevBytesReceived = 0;
        private long _prevBytesSent = 0;
        private DateTime _prevNetTime = DateTime.UtcNow;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        public void Start(HardwareMetrics metrics, int intervalMs = 1000)
        {
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Run(async () =>
            {
                InitializeLibreHardware();

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        UpdateMetrics(metrics);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error updating metrics: {ex.Message}");
                    }

                    try
                    {
                        await Task.Delay(intervalMs, token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }, token);
        }

        private void InitializeLibreHardware()
        {
            try
            {
                _computer = new Computer
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = true,
                    IsMemoryEnabled = true,
                    IsMotherboardEnabled = true,
                    IsNetworkEnabled = true,
                    IsStorageEnabled = true
                };

                _computer.Open();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to open LibreHardwareMonitor: {ex.Message}");
                _isInitialized = false;
            }
        }

        private static string CleanCpuName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "CPU";
            return name
                .Replace("12th Gen ", "")
                .Replace("13th Gen ", "")
                .Replace("14th Gen ", "")
                .Replace("15th Gen ", "")
                .Replace("Intel(R) ", "")
                .Replace("Intel ", "")
                .Replace("Core(TM) ", "")
                .Replace("AMD ", "")
                .Trim();
        }

        private static string CleanGpuName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "GPU";
            return name
                .Replace("NVIDIA GeForce ", "")
                .Replace("NVIDIA ", "")
                .Replace("GeForce ", "")
                .Replace("AMD Radeon ", "")
                .Replace("Radeon ", "")
                .Trim();
        }

        public void UpdateMetrics(HardwareMetrics metrics)
        {
            // 1. Время и дата
            var now = DateTime.Now;
            metrics.TimeString = now.ToString("HH:mm:ss");
            metrics.DateString = now.ToString("dddd, d MMMM yyyy");

            // 2. RAM через Win32 API (работает мгновенно и независимо от прав)
            var memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                float totalGb = memStatus.ullTotalPhys / (1024f * 1024f * 1024f);
                float availGb = memStatus.ullAvailPhys / (1024f * 1024f * 1024f);
                float usedGb = totalGb - availGb;
                metrics.RamTotalGb = (float)Math.Round(totalGb, 1);
                metrics.RamUsedGb = (float)Math.Round(usedGb, 1);
                metrics.RamLoad = memStatus.dwMemoryLoad;
            }

            // 3. Сеть через NetworkInterface
            UpdateNetworkFallback(metrics);

            // 4. LibreHardwareMonitor (CPU / GPU)
            if (_isInitialized && _computer != null)
            {
                try
                {
                    float detectedCpuTemp = 0;
                    float detectedCpuClock = 0;
                    float detectedCpuPower = 0;
                    float detectedCpuLoad = 0;

                    foreach (var hardware in _computer.Hardware)
                    {
                        hardware.Update();
                        foreach (var sub in hardware.SubHardware)
                        {
                            sub.Update();
                        }

                        // CPU
                        if (hardware.HardwareType == HardwareType.Cpu)
                        {
                            metrics.CpuName = CleanCpuName(hardware.Name);

                            foreach (var sensor in hardware.Sensors)
                            {
                                if (sensor.SensorType == SensorType.Load)
                                {
                                    if (sensor.Name.Contains("Total", StringComparison.OrdinalIgnoreCase))
                                    {
                                        detectedCpuLoad = (float)Math.Round(sensor.Value ?? 0, 0);
                                    }
                                    else if (detectedCpuLoad == 0 && sensor.Value.HasValue)
                                    {
                                        detectedCpuLoad = (float)Math.Round(sensor.Value.Value, 0);
                                    }
                                }
                                else if (sensor.SensorType == SensorType.Temperature)
                                {
                                    if (sensor.Value.HasValue && sensor.Value > 0)
                                    {
                                        if (sensor.Name.Contains("Package", StringComparison.OrdinalIgnoreCase) ||
                                            sensor.Name.Contains("Core (Tctl/Tdie)", StringComparison.OrdinalIgnoreCase) ||
                                            sensor.Name.Contains("Core Max", StringComparison.OrdinalIgnoreCase) ||
                                            sensor.Name.Contains("Core Average", StringComparison.OrdinalIgnoreCase))
                                        {
                                            detectedCpuTemp = (float)Math.Round(sensor.Value.Value, 0);
                                        }
                                        else if (detectedCpuTemp == 0)
                                        {
                                            detectedCpuTemp = (float)Math.Round(sensor.Value.Value, 0);
                                        }
                                    }
                                }
                                else if (sensor.SensorType == SensorType.Clock)
                                {
                                    if (sensor.Value.HasValue && sensor.Value > 100)
                                    {
                                        if (sensor.Name.Contains("Core", StringComparison.OrdinalIgnoreCase) ||
                                            sensor.Name.Contains("CPU", StringComparison.OrdinalIgnoreCase) ||
                                            detectedCpuClock == 0)
                                        {
                                            detectedCpuClock = (float)Math.Round(sensor.Value.Value, 0);
                                        }
                                    }
                                }
                                else if (sensor.SensorType == SensorType.Power)
                                {
                                    if (sensor.Value.HasValue && sensor.Value > 0)
                                    {
                                        if (sensor.Name.Contains("Package", StringComparison.OrdinalIgnoreCase) ||
                                            sensor.Name.Contains("CPU", StringComparison.OrdinalIgnoreCase) ||
                                            detectedCpuPower == 0)
                                        {
                                            detectedCpuPower = (float)Math.Round(sensor.Value.Value, 0);
                                        }
                                    }
                                }
                            }
                        }

                        // Motherboard (резервные датчики для CPU)
                        if (hardware.HardwareType == HardwareType.Motherboard && detectedCpuTemp == 0)
                        {
                            foreach (var sensor in hardware.Sensors)
                            {
                                if (sensor.SensorType == SensorType.Temperature &&
                                    (sensor.Name.Contains("CPU", StringComparison.OrdinalIgnoreCase) || sensor.Name.Contains("Processor", StringComparison.OrdinalIgnoreCase)))
                                {
                                    if (sensor.Value.HasValue && sensor.Value > 0)
                                    {
                                        detectedCpuTemp = (float)Math.Round(sensor.Value.Value, 0);
                                    }
                                }
                            }
                        }

                        // GPU (Nvidia, AMD, Intel)
                        if (hardware.HardwareType == HardwareType.GpuNvidia ||
                            hardware.HardwareType == HardwareType.GpuAmd ||
                            hardware.HardwareType == HardwareType.GpuIntel)
                        {
                            metrics.GpuName = CleanGpuName(hardware.Name);

                            foreach (var sensor in hardware.Sensors)
                            {
                                if (sensor.SensorType == SensorType.Load && (sensor.Name.Contains("Core", StringComparison.OrdinalIgnoreCase) || sensor.Name.Contains("GPU", StringComparison.OrdinalIgnoreCase)))
                                {
                                    metrics.GpuLoad = (float)Math.Round(sensor.Value ?? 0, 0);
                                }
                                else if (sensor.SensorType == SensorType.Temperature && (sensor.Name.Contains("Core", StringComparison.OrdinalIgnoreCase) || sensor.Name.Contains("GPU", StringComparison.OrdinalIgnoreCase)))
                                {
                                    metrics.GpuTemp = (float)Math.Round(sensor.Value ?? 0, 0);
                                }
                                else if (sensor.SensorType == SensorType.SmallData && sensor.Name.Contains("Memory Used", StringComparison.OrdinalIgnoreCase))
                                {
                                    metrics.GpuMemoryUsed = (float)Math.Round((sensor.Value ?? 0) / 1024f, 1);
                                }
                                else if (sensor.SensorType == SensorType.SmallData && sensor.Name.Contains("Memory Total", StringComparison.OrdinalIgnoreCase))
                                {
                                    metrics.GpuMemoryTotal = (float)Math.Round((sensor.Value ?? 0) / 1024f, 1);
                                }
                                else if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Memory", StringComparison.OrdinalIgnoreCase))
                                {
                                    metrics.GpuMemoryPercent = (float)Math.Round(sensor.Value ?? 0, 0);
                                }
                            }

                            if (metrics.GpuMemoryPercent == 0 && metrics.GpuMemoryTotal > 0)
                            {
                                metrics.GpuMemoryPercent = (float)Math.Round((metrics.GpuMemoryUsed / metrics.GpuMemoryTotal) * 100f, 0);
                            }
                        }

                        // Storage
                        if (hardware.HardwareType == HardwareType.Storage)
                        {
                            foreach (var sensor in hardware.Sensors)
                            {
                                if (sensor.SensorType == SensorType.Load && metrics.DiskLoad == 0)
                                {
                                    metrics.DiskLoad = (float)Math.Round(sensor.Value ?? 0, 0);
                                }
                                else if (sensor.SensorType == SensorType.Temperature && metrics.DiskTemp == 0)
                                {
                                    metrics.DiskTemp = (float)Math.Round(sensor.Value ?? 0, 0);
                                }
                            }
                        }
                    }

                    if (detectedCpuLoad > 0) metrics.CpuLoad = detectedCpuLoad;
                    if (detectedCpuTemp > 0) metrics.CpuTemp = detectedCpuTemp;
                    if (detectedCpuClock > 0) metrics.CpuClock = detectedCpuClock;
                    if (detectedCpuPower > 0) metrics.CpuPower = detectedCpuPower;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"LibreHardwareMonitor error: {ex.Message}");
                }
            }
        }

        private void UpdateNetworkFallback(HardwareMetrics metrics)
        {
            try
            {
                long currentRecv = 0;
                long currentSent = 0;

                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus == OperationalStatus.Up &&
                        nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        var stats = nic.GetIPv4Statistics();
                        currentRecv += stats.BytesReceived;
                        currentSent += stats.BytesSent;
                    }
                }

                var now = DateTime.UtcNow;
                var elapsedSec = (now - _prevNetTime).TotalSeconds;

                if (elapsedSec > 0.5 && _prevBytesReceived > 0)
                {
                    double bytesPerSecDown = (currentRecv - _prevBytesReceived) / elapsedSec;
                    double bytesPerSecUp = (currentSent - _prevBytesSent) / elapsedSec;

                    metrics.NetDownSpeed = FormatSpeed(bytesPerSecDown);
                    metrics.NetUpSpeed = FormatSpeed(bytesPerSecUp);
                }

                _prevBytesReceived = currentRecv;
                _prevBytesSent = currentSent;
                _prevNetTime = now;
            }
            catch
            {
                // Ignore network query failures
            }
        }

        private static string FormatSpeed(double bytesPerSec)
        {
            if (bytesPerSec < 0) bytesPerSec = 0;
            if (bytesPerSec >= 1024 * 1024)
            {
                return $"{(bytesPerSec / (1024 * 1024)):F1} MB/s";
            }
            else
            {
                return $"{(bytesPerSec / 1024):F0} KB/s";
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            try
            {
                _computer?.Close();
            }
            catch { }
            GC.SuppressFinalize(this);
        }
    }
}

