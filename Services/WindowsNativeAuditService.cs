using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace AegisCore.Services;

public class WindowsNativeAudit
{
    public DateTime CollectedAt { get; set; }
    public SystemInfoDetailed System { get; set; } = new();
    public CpuInfoDetailed Cpu { get; set; } = new();
    public RamInfoDetailed Ram { get; set; } = new();
    public MotherboardInfoDetailed Motherboard { get; set; } = new();
    public BiosInfoDetailed Bios { get; set; } = new();
    public List<DiskInfoDetailed> PhysicalDisks { get; set; } = new();
    public List<VolumeInfoDetailed> LogicalDisks { get; set; } = new();
    public List<GpuInfoDetailed> Gpus { get; set; } = new();
    public List<NetworkAdapterDetailed> NetworkAdapters { get; set; } = new();
    public List<SoundDeviceInfo> SoundDevices { get; set; } = new();
    public List<UsbDeviceInfo> UsbDevices { get; set; } = new();
    public List<OpticalDriveInfo> OpticalDrives { get; set; } = new();
    public BatteryInfoDetailed? Battery { get; set; }
    public List<InstalledProgramInfo> InstalledPrograms { get; set; } = new();
    public List<DriverInfoDetailed> Drivers { get; set; } = new();
    public List<ServiceInfoDetailed> Services { get; set; } = new();
    public List<ProcessInfoDetailed> Processes { get; set; } = new();
    public List<StartupItemDetailed> StartupItems { get; set; } = new();
    public List<ScheduledTaskDetailed> ScheduledTasks { get; set; } = new();
    public List<WindowsUpdateDetailed> Updates { get; set; } = new();
    public List<NetworkConnectionDetailed> Connections { get; set; } = new();
    public List<OpenPortDetailed> OpenPorts { get; set; } = new();
    public List<UserAccountDetailed> UserAccounts { get; set; } = new();
    public List<UserGroupDetailed> UserGroups { get; set; } = new();
    public List<SharedFolderDetailed> SharedFolders { get; set; } = new();
    public FirewallInfoDetailed Firewall { get; set; } = new();
    public PerformanceMetricsDetailed Performance { get; set; } = new();
    public List<EventLogEntryDetailed> SystemLog { get; set; } = new();
    public List<EventLogEntryDetailed> ApplicationLog { get; set; } = new();
    public List<EventLogEntryDetailed> SecurityLog { get; set; } = new();
    public DirectXInfoDetailed DirectX { get; set; } = new();
    public List<PrinterInfoDetailed> Printers { get; set; } = new();
}

// ==================== ЖЕЛЕЗО ====================

public class SystemInfoDetailed
{
    public string Name { get; set; } = "";
    public string Os { get; set; } = "";
    public string OsVersion { get; set; } = "";
    public string OsBuild { get; set; } = "";
    public string OsArchitecture { get; set; } = "";
    public string SystemType { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string Model { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public DateTime InstallDate { get; set; }
    public DateTime LastBootTime { get; set; }
    public TimeSpan Uptime { get; set; }
    public string Domain { get; set; } = "";
    public string Workgroup { get; set; } = "";
}

public class CpuInfoDetailed
{
    public string Name { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public int Cores { get; set; }
    public int LogicalProcessors { get; set; }
    public double MaxClockSpeedMhz { get; set; }
    public double CurrentClockSpeedMhz { get; set; }
    public int L1CacheKb { get; set; }
    public int L2CacheKb { get; set; }
    public int L3CacheKb { get; set; }
    public string Architecture { get; set; } = "";
    public string Socket { get; set; } = "";
    public double LoadPercentage { get; set; }
}

public class RamInfoDetailed
{
    public double TotalGb { get; set; }
    public double FreeGb { get; set; }
    public double UsedGb { get; set; }
    public int TotalSlots { get; set; }
    public int UsedSlots { get; set; }
    public List<RamStickInfo> Sticks { get; set; } = new();
}

public class RamStickInfo
{
    public string Manufacturer { get; set; } = "";
    public double CapacityGb { get; set; }
    public int SpeedMhz { get; set; }
    public string Type { get; set; } = "";
    public string Serial { get; set; } = "";
    public string PartNumber { get; set; } = "";
}

public class MotherboardInfoDetailed
{
    public string Manufacturer { get; set; } = "";
    public string Product { get; set; } = "";
    public string Version { get; set; } = "";
    public string Serial { get; set; } = "";
}

public class BiosInfoDetailed
{
    public string Manufacturer { get; set; } = "";
    public string Version { get; set; } = "";
    public string ReleaseDate { get; set; } = "";
    public string Serial { get; set; } = "";
}

public class DiskInfoDetailed
{
    public string Model { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public double SizeGb { get; set; }
    public string MediaType { get; set; } = "";
    public string Interface { get; set; } = "";
    public string Serial { get; set; } = "";
    public string Status { get; set; } = "";
    public int Partitions { get; set; }
}

public class VolumeInfoDetailed
{
    public string DeviceId { get; set; } = "";
    public string VolumeName { get; set; } = "";
    public string FileSystem { get; set; } = "";
    public double SizeGb { get; set; }
    public double FreeGb { get; set; }
    public string Serial { get; set; } = "";
}

public class GpuInfoDetailed
{
    public string Name { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public double VramGb { get; set; }
    public string DriverVersion { get; set; } = "";
    public string DriverDate { get; set; } = "";
    public string Status { get; set; } = "";
    public string Resolution { get; set; } = "";
    public int RefreshRateHz { get; set; }
}

public class NetworkAdapterDetailed
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Mac { get; set; } = "";
    public List<string> IpAddresses { get; set; } = new();
    public List<string> SubnetMasks { get; set; } = new();
    public List<string> Gateways { get; set; } = new();
    public List<string> DnsServers { get; set; } = new();
    public bool DhcpEnabled { get; set; }
    public string DhcpServer { get; set; } = "";
    public string Speed { get; set; } = "";
    public bool IsConnected { get; set; }
    public string Status { get; set; } = "";
}

public class SoundDeviceInfo
{
    public string Name { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string Status { get; set; } = "";
}

public class UsbDeviceInfo
{
    public string Name { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string DeviceId { get; set; } = "";
    public string Status { get; set; } = "";
}

public class OpticalDriveInfo
{
    public string Name { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string MediaType { get; set; } = "";
    public string Status { get; set; } = "";
}

public class BatteryInfoDetailed
{
    public string Name { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string Serial { get; set; } = "";
    public int DesignCapacityMwh { get; set; }
    public int FullChargeCapacityMwh { get; set; }
    public int CycleCount { get; set; }
    public double HealthPercentage { get; set; }
    public string Status { get; set; } = "";
}

// ==================== ПО ====================

public class InstalledProgramInfo
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public string Publisher { get; set; } = "";
    public string InstallDate { get; set; } = "";
    public string InstallLocation { get; set; } = "";
    public double SizeMb { get; set; }
    public string UninstallString { get; set; } = "";
    public string Source { get; set; } = "";
}

public class DriverInfoDetailed
{
    public string Name { get; set; } = "";
    public string DeviceName { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string Version { get; set; } = "";
    public string DriverDate { get; set; } = "";
    public string DriverType { get; set; } = "";
    public bool IsSigned { get; set; }
    public string InfName { get; set; } = "";
    public string State { get; set; } = "";
}

public class ServiceInfoDetailed
{
    public string Name { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Status { get; set; } = "";
    public string StartType { get; set; } = "";
    public string PathName { get; set; } = "";
    public string Description { get; set; } = "";
    public string ProcessId { get; set; } = "";
}

public class ProcessInfoDetailed
{
    public int Pid { get; set; }
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string User { get; set; } = "";
    public double CpuTime { get; set; }
    public long WorkingSetMb { get; set; }
    public long VirtualMemoryMb { get; set; }
    public string StartTime { get; set; } = "";
}

public class StartupItemDetailed
{
    public string Name { get; set; } = "";
    public string Command { get; set; } = "";
    public string Source { get; set; } = "";
    public bool IsEnabled { get; set; } = true;
}

public class ScheduledTaskDetailed
{
    public string TaskName { get; set; } = "";
    public string TaskPath { get; set; } = "";
    public string Status { get; set; } = "";
    public string NextRunTime { get; set; } = "";
    public string LastRunTime { get; set; } = "";
    public string LastResult { get; set; } = "";
}

public class WindowsUpdateDetailed
{
    public string HotFixId { get; set; } = "";
    public string Description { get; set; } = "";
    public string InstalledOn { get; set; } = "";
    public string InstalledBy { get; set; } = "";
}

// ==================== СЕТЬ ====================

public class NetworkConnectionDetailed
{
    public string Protocol { get; set; } = "";
    public string LocalAddress { get; set; } = "";
    public int LocalPort { get; set; }
    public string RemoteAddress { get; set; } = "";
    public int RemotePort { get; set; }
    public string State { get; set; } = "";
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = "";
}

public class OpenPortDetailed
{
    public int Port { get; set; }
    public string Protocol { get; set; } = "";
    public string ProcessName { get; set; } = "";
    public int ProcessId { get; set; }
    public string State { get; set; } = "";
}

// ==================== БЕЗОПАСНОСТЬ ====================

public class UserAccountDetailed
{
    public string Name { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Description { get; set; } = "";
    public string Sid { get; set; } = "";
    public bool Disabled { get; set; }
    public bool LockedOut { get; set; }
    public bool PasswordRequired { get; set; }
    public bool PasswordExpires { get; set; }
    public DateTime? LastLogon { get; set; }
    public List<string> Groups { get; set; } = new();
}

public class UserGroupDetailed
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Members { get; set; } = new();
}

public class SharedFolderDetailed
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string Type { get; set; } = "";
    public string Description { get; set; } = "";
    public int CurrentUsers { get; set; }
}

public class FirewallInfoDetailed
{
    public bool DomainProfileEnabled { get; set; }
    public bool PrivateProfileEnabled { get; set; }
    public bool PublicProfileEnabled { get; set; }
    public int InboundRulesCount { get; set; }
    public int OutboundRulesCount { get; set; }
    public List<string> InboundRules { get; set; } = new();
    public List<string> OutboundRules { get; set; } = new();
}

// ==================== ПРОИЗВОДИТЕЛЬНОСТЬ ====================

public class PerformanceMetricsDetailed
{
    public double CpuUsagePercent { get; set; }
    public double RamUsagePercent { get; set; }
    public double RamUsedGb { get; set; }
    public double RamTotalGb { get; set; }
    public Dictionary<string, DiskPerformanceInfo> DiskPerformance { get; set; } = new();
    public Dictionary<string, NetworkPerformanceInfo> NetworkPerformance { get; set; } = new();
}

public class DiskPerformanceInfo
{
    public double ReadBytesPerSec { get; set; }
    public double WriteBytesPerSec { get; set; }
    public double UsagePercent { get; set; }
}

public class NetworkPerformanceInfo
{
    public double BytesSentPerSec { get; set; }
    public double BytesReceivedPerSec { get; set; }
}

// ==================== ЖУРНАЛЫ ====================

public class EventLogEntryDetailed
{
    public string TimeGenerated { get; set; } = "";
    public string EntryType { get; set; } = "";
    public string Source { get; set; } = "";
    public int EventId { get; set; }
    public string Message { get; set; } = "";
}

// ==================== ДОПОЛНИТЕЛЬНО ====================

public class DirectXInfoDetailed
{
    public string Version { get; set; } = "";
    public string DriverModel { get; set; } = "";
    public string FeatureLevels { get; set; } = "";
}

public class PrinterInfoDetailed
{
    public string Name { get; set; } = "";
    public string DriverName { get; set; } = "";
    public string PortName { get; set; } = "";
    public bool IsDefault { get; set; }
    public string Status { get; set; } = "";
}

// ==================== СЕРВИС СБОРА ====================

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public class WindowsNativeAuditService
{
    public Task<WindowsNativeAudit> CollectAsync() => Task.Run(Collect);

    public WindowsNativeAudit Collect()
    {
        var audit = new WindowsNativeAudit { CollectedAt = DateTime.Now };
        
        Safe(() => CollectSystem(audit));
        Safe(() => CollectCpu(audit));
        Safe(() => CollectRam(audit));
        Safe(() => CollectMotherboard(audit));
        Safe(() => CollectBios(audit));
        Safe(() => CollectDisks(audit));
        Safe(() => CollectGpus(audit));
        Safe(() => CollectNetwork(audit));
        Safe(() => CollectSound(audit));
        Safe(() => CollectUsb(audit));
        Safe(() => CollectOpticalDrives(audit));
        Safe(() => CollectBattery(audit));
        Safe(() => CollectInstalledPrograms(audit));
        Safe(() => CollectDrivers(audit));
        Safe(() => CollectServices(audit));
        Safe(() => CollectProcesses(audit));
        Safe(() => CollectStartup(audit));
        Safe(() => CollectScheduledTasks(audit));
        Safe(() => CollectUpdates(audit));
        Safe(() => CollectConnections(audit));
        Safe(() => CollectUserAccounts(audit));
        Safe(() => CollectUserGroups(audit));
        Safe(() => CollectSharedFolders(audit));
        Safe(() => CollectFirewall(audit));
        Safe(() => CollectPerformance(audit));
        Safe(() => CollectEventLogs(audit));
        Safe(() => CollectDirectX(audit));
        Safe(() => CollectPrinters(audit));
        
        return audit;
    }

    private static void Safe(Action action)
    {
        try { action(); }
        catch { /* пропускаем недоступные разделы */ }
    }

    private static List<ManagementObject> Wql(string query, string scope = @"root\cimv2")
    {
        var result = new List<ManagementObject>();
        using var searcher = new ManagementObjectSearcher(scope, query);
        foreach (var o in searcher.Get())
            if (o is ManagementObject mo) result.Add(mo);
        return result;
    }

    private static string S(ManagementBaseObject o, string prop) => o[prop]?.ToString()?.Trim() ?? "";
    private static double D(ManagementBaseObject o, string prop) =>
        double.TryParse(o[prop]?.ToString(), System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;
    private static double Gb(double bytes) => Math.Round(bytes / 1073741824d, 2);

    private static void CollectSystem(WindowsNativeAudit a)
    {
        var os = Wql("SELECT * FROM Win32_OperatingSystem").FirstOrDefault();
        var cs = Wql("SELECT * FROM Win32_ComputerSystem").FirstOrDefault();
        var bios = Wql("SELECT * FROM Win32_BIOS").FirstOrDefault();

        a.System = new SystemInfoDetailed
        {
            Name = Environment.MachineName,
            Os = os != null ? S(os, "Caption") : "",
            OsVersion = os != null ? S(os, "Version") : "",
            OsBuild = os != null ? S(os, "BuildNumber") : "",
            OsArchitecture = os != null ? S(os, "OSArchitecture") : "",
            SystemType = cs != null ? S(cs, "SystemType") : "",
            Manufacturer = cs != null ? S(cs, "Manufacturer") : "",
            Model = cs != null ? S(cs, "Model") : "",
            SerialNumber = bios != null ? S(bios, "SerialNumber") : "",
            InstallDate = os != null ? ManagementDateTimeConverter.ToDateTime(S(os, "InstallDate")) : DateTime.MinValue,
            LastBootTime = os != null ? ManagementDateTimeConverter.ToDateTime(S(os, "LastBootUpTime")) : DateTime.MinValue,
            Uptime = os != null ? DateTime.Now - ManagementDateTimeConverter.ToDateTime(S(os, "LastBootUpTime")) : TimeSpan.Zero,
            Domain = cs != null ? S(cs, "Domain") : "",
            Workgroup = cs != null ? S(cs, "Workgroup") : ""
        };
    }

    private static void CollectCpu(WindowsNativeAudit a)
    {
        var cpu = Wql("SELECT * FROM Win32_Processor").FirstOrDefault();
        if (cpu != null)
        {
            a.Cpu = new CpuInfoDetailed
            {
                Name = S(cpu, "Name"),
                Manufacturer = S(cpu, "Manufacturer"),
                Cores = (int)D(cpu, "NumberOfCores"),
                LogicalProcessors = (int)D(cpu, "NumberOfLogicalProcessors"),
                MaxClockSpeedMhz = D(cpu, "MaxClockSpeed"),
                CurrentClockSpeedMhz = D(cpu, "CurrentClockSpeed"),
                L1CacheKb = (int)D(cpu, "L1CacheSize"),
                L2CacheKb = (int)D(cpu, "L2CacheSize"),
                L3CacheKb = (int)D(cpu, "L3CacheSize"),
                Architecture = S(cpu, "Architecture"),
                Socket = S(cpu, "SocketDesignation"),
                LoadPercentage = D(cpu, "LoadPercentage")
            };
        }
    }

    private static void CollectRam(WindowsNativeAudit a)
    {
        var cs = Wql("SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem").FirstOrDefault();
        var sticks = Wql("SELECT * FROM Win32_PhysicalMemory");
        var slots = Wql("SELECT * FROM Win32_PhysicalMemoryArray").FirstOrDefault();

        a.Ram = new RamInfoDetailed
        {
            TotalGb = cs != null ? Gb(D(cs, "TotalVisibleMemorySize") * 1024) : 0,
            FreeGb = cs != null ? Gb(D(cs, "FreePhysicalMemory") * 1024) : 0,
            UsedGb = cs != null ? Gb((D(cs, "TotalVisibleMemorySize") - D(cs, "FreePhysicalMemory")) * 1024) : 0,
            TotalSlots = slots != null ? (int)D(slots, "MemoryDevices") : 0,
            UsedSlots = sticks.Count,
            Sticks = sticks.Select(s => new RamStickInfo
            {
                Manufacturer = S(s, "Manufacturer"),
                CapacityGb = Gb(D(s, "Capacity")),
                SpeedMhz = (int)D(s, "Speed"),
                Type = S(s, "SMBIOSMemoryType"),
                Serial = S(s, "SerialNumber"),
                PartNumber = S(s, "PartNumber")
            }).ToList()
        };
    }

    private static void CollectMotherboard(WindowsNativeAudit a)
    {
        var board = Wql("SELECT * FROM Win32_BaseBoard").FirstOrDefault();
        if (board != null)
        {
            a.Motherboard = new MotherboardInfoDetailed
            {
                Manufacturer = S(board, "Manufacturer"),
                Product = S(board, "Product"),
                Version = S(board, "Version"),
                Serial = S(board, "SerialNumber")
            };
        }
    }

    private static void CollectBios(WindowsNativeAudit a)
    {
        var bios = Wql("SELECT * FROM Win32_BIOS").FirstOrDefault();
        if (bios != null)
        {
            a.Bios = new BiosInfoDetailed
            {
                Manufacturer = S(bios, "Manufacturer"),
                Version = S(bios, "SMBIOSBIOSVersion"),
                ReleaseDate = S(bios, "ReleaseDate"),
                Serial = S(bios, "SerialNumber")
            };
        }
    }

    private static void CollectDisks(WindowsNativeAudit a)
    {
        foreach (var d in Wql("SELECT * FROM Win32_DiskDrive"))
            a.PhysicalDisks.Add(new DiskInfoDetailed
            {
                Model = S(d, "Model"),
                Manufacturer = S(d, "Manufacturer"),
                SizeGb = Gb(D(d, "Size")),
                MediaType = S(d, "MediaType"),
                Interface = S(d, "InterfaceType"),
                Serial = S(d, "SerialNumber"),
                Status = S(d, "Status"),
                Partitions = (int)D(d, "Partitions")
            });

        foreach (var v in Wql("SELECT * FROM Win32_LogicalDisk WHERE DriveType=3"))
            a.LogicalDisks.Add(new VolumeInfoDetailed
            {
                DeviceId = S(v, "DeviceId"),
                VolumeName = S(v, "VolumeName"),
                FileSystem = S(v, "FileSystem"),
                SizeGb = Gb(D(v, "Size")),
                FreeGb = Gb(D(v, "FreeSpace")),
                Serial = S(v, "VolumeSerialNumber")
            });
    }

    private static void CollectGpus(WindowsNativeAudit a)
    {
        foreach (var g in Wql("SELECT * FROM Win32_VideoController"))
            a.Gpus.Add(new GpuInfoDetailed
            {
                Name = S(g, "Name"),
                Manufacturer = S(g, "AdapterCompatibility"),
                VramGb = Gb(D(g, "AdapterRAM")),
                DriverVersion = S(g, "DriverVersion"),
                DriverDate = S(g, "DriverDate"),
                Status = S(g, "Status"),
                Resolution = $"{S(g, "CurrentHorizontalResolution")}x{S(g, "CurrentVerticalResolution")}",
                RefreshRateHz = (int)D(g, "CurrentRefreshRate")
            });
    }

    private static void CollectNetwork(WindowsNativeAudit a)
    {
        foreach (var n in Wql("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled=True"))
        {
            var ips = n["IPAddress"] as string[] ?? Array.Empty<string>();
            var masks = n["IPSubnet"] as string[] ?? Array.Empty<string>();
            var gateways = n["DefaultIPGateway"] as string[] ?? Array.Empty<string>();
            var dns = n["DNSServerSearchOrder"] as string[] ?? Array.Empty<string>();

            a.NetworkAdapters.Add(new NetworkAdapterDetailed
            {
                Name = S(n, "Description"),
                Description = S(n, "Description"),
                Mac = S(n, "MACAddress"),
                IpAddresses = ips.ToList(),
                SubnetMasks = masks.ToList(),
                Gateways = gateways.ToList(),
                DnsServers = dns.ToList(),
                DhcpEnabled = S(n, "DHCPEnabled") == "True",
                DhcpServer = S(n, "DHCPServer"),
                Speed = "",
                IsConnected = true,
                Status = "Active"
            });
        }
    }

    private static void CollectSound(WindowsNativeAudit a)
    {
        foreach (var s in Wql("SELECT * FROM Win32_SoundDevice"))
            a.SoundDevices.Add(new SoundDeviceInfo
            {
                Name = S(s, "Name"),
                Manufacturer = S(s, "Manufacturer"),
                Status = S(s, "Status")
            });
    }

    private static void CollectUsb(WindowsNativeAudit a)
    {
        foreach (var u in Wql("SELECT * FROM Win32_USBHub"))
            a.UsbDevices.Add(new UsbDeviceInfo
            {
                Name = S(u, "Name"),
                Manufacturer = S(u, "Manufacturer"),
                DeviceId = S(u, "DeviceID"),
                Status = S(u, "Status")
            });
    }

    private static void CollectOpticalDrives(WindowsNativeAudit a)
    {
        foreach (var o in Wql("SELECT * FROM Win32_CDROMDrive"))
            a.OpticalDrives.Add(new OpticalDriveInfo
            {
                Name = S(o, "Name"),
                Manufacturer = S(o, "Manufacturer"),
                MediaType = S(o, "MediaType"),
                Status = S(o, "Status")
            });
    }

    private static void CollectBattery(WindowsNativeAudit a)
    {
        var bat = Wql("SELECT * FROM Win32_Battery").FirstOrDefault();
        if (bat != null)
        {
            a.Battery = new BatteryInfoDetailed
            {
                Name = S(bat, "Name"),
                Manufacturer = S(bat, "Manufacturer"),
                Serial = S(bat, "SerialNumber"),
                DesignCapacityMwh = (int)D(bat, "DesignCapacity"),
                FullChargeCapacityMwh = (int)D(bat, "FullChargeCapacity"),
                CycleCount = (int)D(bat, "CycleCount"),
                HealthPercentage = D(bat, "EstimatedChargeRemaining"),
                Status = S(bat, "Status")
            };
        }
    }

    private static void CollectInstalledPrograms(WindowsNativeAudit a)
    {
        var registryPaths = new[]
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        };

        foreach (var path in registryPaths)
        {
            using var key = Registry.LocalMachine.OpenSubKey(path);
            if (key == null) continue;

            foreach (var subKeyName in key.GetSubKeyNames())
            {
                using var subKey = key.OpenSubKey(subKeyName);
                if (subKey == null) continue;

                var name = subKey.GetValue("DisplayName")?.ToString();
                if (string.IsNullOrWhiteSpace(name)) continue;

                a.InstalledPrograms.Add(new InstalledProgramInfo
                {
                    Name = name,
                    Version = subKey.GetValue("DisplayVersion")?.ToString() ?? "",
                    Publisher = subKey.GetValue("Publisher")?.ToString() ?? "",
                    InstallDate = subKey.GetValue("InstallDate")?.ToString() ?? "",
                    InstallLocation = subKey.GetValue("InstallLocation")?.ToString() ?? "",
                    SizeMb = double.TryParse(subKey.GetValue("EstimatedSize")?.ToString(), out var size) ? size / 1024.0 : 0,
                    UninstallString = subKey.GetValue("UninstallString")?.ToString() ?? "",
                    Source = path.Contains("WOW6432Node") ? "Registry (x86)" : "Registry (x64)"
                });
            }
        }
    }

    private static void CollectDrivers(WindowsNativeAudit a)
    {
        foreach (var d in Wql("SELECT * FROM Win32_PnPSignedDriver"))
        {
            var name = S(d, "DeviceName");
            if (string.IsNullOrEmpty(name)) continue;

            a.Drivers.Add(new DriverInfoDetailed
            {
                Name = S(d, "DriverName"),
                DeviceName = name,
                Manufacturer = S(d, "Manufacturer"),
                Version = S(d, "DriverVersion"),
                DriverDate = S(d, "DriverDate"),
                DriverType = S(d, "DriverClass"),
                IsSigned = S(d, "IsSigned") != "False",
                InfName = S(d, "InfName"),
                State = S(d, "Status")
            });
        }
    }

    private static void CollectServices(WindowsNativeAudit a)
    {
        foreach (var s in Wql("SELECT * FROM Win32_Service"))
            a.Services.Add(new ServiceInfoDetailed
            {
                Name = S(s, "Name"),
                DisplayName = S(s, "DisplayName"),
                Status = S(s, "State"),
                StartType = S(s, "StartMode"),
                PathName = S(s, "PathName"),
                Description = S(s, "Description"),
                ProcessId = S(s, "ProcessId")
            });
    }

    private static void CollectProcesses(WindowsNativeAudit a)
    {
        foreach (var p in Process.GetProcesses())
        {
            try
            {
                a.Processes.Add(new ProcessInfoDetailed
                {
                    Pid = p.Id,
                    Name = p.ProcessName,
                    Path = p.MainModule?.FileName ?? "",
                    User = "",
                    CpuTime = p.TotalProcessorTime.TotalSeconds,
                    WorkingSetMb = p.WorkingSet64 / 1048576,
                    VirtualMemoryMb = p.VirtualMemorySize64 / 1048576,
                    StartTime = p.StartTime.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }
            catch { }
        }
    }

    private static void CollectStartup(WindowsNativeAudit a)
    {
        AddRunKey(a, Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "HKLM Run");
        AddRunKey(a, Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run", "HKLM Run (x86)");
        AddRunKey(a, Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "HKCU Run");
        AddStartupFolder(a, Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Startup (user)");
        AddStartupFolder(a, Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), "Startup (all users)");
    }

    private static void AddRunKey(WindowsNativeAudit a, RegistryKey root, string path, string source)
    {
        using var key = root.OpenSubKey(path);
        if (key == null) return;
        foreach (var name in key.GetValueNames())
            a.StartupItems.Add(new StartupItemDetailed
            {
                Name = name,
                Command = key.GetValue(name)?.ToString() ?? "",
                Source = source,
                IsEnabled = true
            });
    }

    private static void AddStartupFolder(WindowsNativeAudit a, string folder, string source)
    {
        if (!System.IO.Directory.Exists(folder)) return;
        foreach (var f in System.IO.Directory.GetFiles(folder))
            a.StartupItems.Add(new StartupItemDetailed
            {
                Name = System.IO.Path.GetFileName(f),
                Command = f,
                Source = source,
                IsEnabled = true
            });
    }

    private static void CollectScheduledTasks(WindowsNativeAudit a)
    {
        var psi = new ProcessStartInfo("schtasks", "/query /FO CSV /NH")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var p = Process.Start(psi);
        if (p == null) return;
        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit(15000);
        foreach (var raw in output.Split('\n'))
        {
            var parts = SplitCsv(raw.Trim());
            if (parts.Count >= 6)
                a.ScheduledTasks.Add(new ScheduledTaskDetailed
                {
                    TaskName = parts[0],
                    TaskPath = parts[0],
                    Status = parts[2],
                    NextRunTime = parts[1],
                    LastRunTime = parts[3],
                    LastResult = parts[4]
                });
        }
    }

    private static List<string> SplitCsv(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var cur = new StringBuilder();
        foreach (char c in line)
        {
            if (c == '"') inQuotes = !inQuotes;
            else if (c == ',' && !inQuotes) { result.Add(cur.ToString()); cur.Clear(); }
            else cur.Append(c);
        }
        result.Add(cur.ToString());
        return result;
    }

    private static void CollectUpdates(WindowsNativeAudit a)
    {
        foreach (var u in Wql("SELECT * FROM Win32_QuickFixEngineering"))
            a.Updates.Add(new WindowsUpdateDetailed
            {
                HotFixId = S(u, "HotFixID"),
                Description = S(u, "Description"),
                InstalledOn = S(u, "InstalledOn"),
                InstalledBy = S(u, "InstalledBy")
            });
    }

    private static void CollectConnections(WindowsNativeAudit a)
    {
        var properties = IPGlobalProperties.GetIPGlobalProperties();
        var connections = properties.GetActiveTcpConnections();
        foreach (var c in connections)
        {
            a.Connections.Add(new NetworkConnectionDetailed
            {
                Protocol = "TCP",
                LocalAddress = c.LocalEndPoint.Address.ToString(),
                LocalPort = c.LocalEndPoint.Port,
                RemoteAddress = c.RemoteEndPoint.Address.ToString(),
                RemotePort = c.RemoteEndPoint.Port,
                State = c.State.ToString(),
                ProcessId = 0,
                ProcessName = ""
            });
        }

        foreach (var listener in properties.GetActiveTcpListeners())
            a.OpenPorts.Add(new OpenPortDetailed
            {
                Port = listener.Port,
                Protocol = "TCP",
                ProcessName = "",
                ProcessId = 0,
                State = "Listening"
            });
    }

    private static void CollectUserAccounts(WindowsNativeAudit a)
    {
        foreach (var u in Wql("SELECT * FROM Win32_UserAccount WHERE LocalAccount=True"))
        {
            var groups = new List<string>();
            foreach (var gu in Wql($"SELECT GroupComponent FROM Win32_GroupUser WHERE PartComponent LIKE '%{S(u, "Name")}%'"))
            {
                var group = gu["GroupComponent"]?.ToString() ?? "";
                var m = System.Text.RegularExpressions.Regex.Match(group, "Name=\"([^\"]+)\"");
                if (m.Success) groups.Add(m.Groups[1].Value);
            }

            a.UserAccounts.Add(new UserAccountDetailed
            {
                Name = S(u, "Name"),
                FullName = S(u, "FullName"),
                Description = S(u, "Description"),
                Sid = S(u, "SID"),
                Disabled = S(u, "Disabled") == "True",
                LockedOut = S(u, "Lockout") == "True",
                PasswordRequired = S(u, "PasswordRequired") == "True",
                PasswordExpires = S(u, "PasswordExpires") == "True",
                LastLogon = null,
                Groups = groups
            });
        }
    }

    private static void CollectUserGroups(WindowsNativeAudit a)
    {
        foreach (var g in Wql("SELECT * FROM Win32_Group WHERE LocalAccount=True"))
        {
            var members = new List<string>();
            foreach (var gu in Wql($"SELECT PartComponent FROM Win32_GroupUser WHERE GroupComponent LIKE '%{S(g, "Name")}%'"))
            {
                var part = gu["PartComponent"]?.ToString() ?? "";
                var m = System.Text.RegularExpressions.Regex.Match(part, "Name=\"([^\"]+)\"");
                if (m.Success) members.Add(m.Groups[1].Value);
            }

            a.UserGroups.Add(new UserGroupDetailed
            {
                Name = S(g, "Name"),
                Description = S(g, "Description"),
                Members = members
            });
        }
    }

    private static void CollectSharedFolders(WindowsNativeAudit a)
    {
        foreach (var sh in Wql("SELECT * FROM Win32_Share"))
            a.SharedFolders.Add(new SharedFolderDetailed
            {
                Name = S(sh, "Name"),
                Path = S(sh, "Path"),
                Type = ShareTypeName(S(sh, "Type")),
                Description = S(sh, "Description"),
                CurrentUsers = (int)D(sh, "CurrentUsers")
            });
    }

    private static string ShareTypeName(string t) => t switch
    {
        "0" => "Disk", "1" => "Printer", "2" => "Device", "3" => "IPC",
        "2147483648" => "Admin", _ => "Other"
    };

    private static void CollectFirewall(WindowsNativeAudit a)
    {
        try
        {
            var psi = new ProcessStartInfo("netsh", "advfirewall show allprofiles state")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var p = Process.Start(psi);
            if (p == null) return;
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit(5000);

            a.Firewall.DomainProfileEnabled = output.Contains("Domain Profile") && output.Contains("ON");
            a.Firewall.PrivateProfileEnabled = output.Contains("Private Profile") && output.Contains("ON");
            a.Firewall.PublicProfileEnabled = output.Contains("Public Profile") && output.Contains("ON");
        }
        catch { }
    }

    private static void CollectPerformance(WindowsNativeAudit a)
    {
        var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        a.Performance.CpuUsagePercent = cpuCounter.NextValue();
        System.Threading.Thread.Sleep(100);
        a.Performance.CpuUsagePercent = cpuCounter.NextValue();

        a.Performance.RamTotalGb = a.Ram.TotalGb;
        a.Performance.RamUsedGb = a.Ram.UsedGb;
        a.Performance.RamUsagePercent = a.Ram.TotalGb > 0 ? (a.Ram.UsedGb / a.Ram.TotalGb) * 100 : 0;
    }

    private static void CollectEventLogs(WindowsNativeAudit a)
    {
        try
        {
            var log = new EventLog("System");
            a.SystemLog = log.Entries.Cast<EventLogEntry>()
                .Where(e => e.EntryType == EventLogEntryType.Error || e.EntryType == EventLogEntryType.Warning)
                .OrderByDescending(e => e.TimeGenerated)
                .Take(50)
                .Select(e => new EventLogEntryDetailed
                {
                    TimeGenerated = e.TimeGenerated.ToString("yyyy-MM-dd HH:mm:ss"),
                    EntryType = e.EntryType.ToString(),
                    Source = e.Source,
                    EventId = (int)e.InstanceId,
                    Message = e.Message
                }).ToList();
        }
        catch { }

        try
        {
            var log = new EventLog("Application");
            a.ApplicationLog = log.Entries.Cast<EventLogEntry>()
                .Where(e => e.EntryType == EventLogEntryType.Error)
                .OrderByDescending(e => e.TimeGenerated)
                .Take(50)
                .Select(e => new EventLogEntryDetailed
                {
                    TimeGenerated = e.TimeGenerated.ToString("yyyy-MM-dd HH:mm:ss"),
                    EntryType = e.EntryType.ToString(),
                    Source = e.Source,
                    EventId = (int)e.InstanceId,
                    Message = e.Message
                }).ToList();
        }
        catch { }
    }

    private static void CollectDirectX(WindowsNativeAudit a)
    {
        a.DirectX = new DirectXInfoDetailed
        {
            Version = "12",
            DriverModel = "WDDM 2.0+",
            FeatureLevels = "12_1, 12_0, 11_1, 11_0"
        };
    }

    private static void CollectPrinters(WindowsNativeAudit a)
    {
        foreach (var p in Wql("SELECT * FROM Win32_Printer"))
            a.Printers.Add(new PrinterInfoDetailed
            {
                Name = S(p, "Name"),
                DriverName = S(p, "DriverName"),
                PortName = S(p, "PortName"),
                IsDefault = S(p, "Default") == "True",
                Status = S(p, "PrinterStatus")
            });
    }

    public string BuildHtmlReport(WindowsNativeAudit a)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html lang='ru'><head><meta charset='utf-8'><title>Windows Native Audit</title><style>");
        sb.AppendLine("body{font-family:'Segoe UI',Arial,sans-serif;margin:24px}h1{font-size:24px}h2{font-size:18px;margin-top:30px;border-bottom:2px solid #4a6da7;padding-bottom:6px}table{border-collapse:collapse;width:100%;margin-top:10px;font-size:13px}th,td{border:1px solid #ddd;padding:6px 10px;text-align:left}th{background:#f5f5f5}.warn{color:#c00}.ok{color:#080}");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h1>Windows Native Audit: {a.System.Name}</h1>");
        sb.AppendLine($"<p><strong>Собрано:</strong> {a.CollectedAt:yyyy-MM-dd HH:mm:ss}</p>");
        sb.AppendLine($"<p><strong>Uptime:</strong> {a.System.Uptime.Days}d {a.System.Uptime.Hours}h {a.System.Uptime.Minutes}m</p>");

        // System
        Table(sb, "Система", new[] { "Параметр", "Значение" }, new[]
        {
            new[] { "Имя", a.System.Name },
            new[] { "ОС", $"{a.System.Os} {a.System.OsVersion}" },
            new[] { "Build", a.System.OsBuild },
            new[] { "Архитектура", a.System.OsArchitecture },
            new[] { "Производитель", a.System.Manufacturer },
            new[] { "Модель", a.System.Model },
            new[] { "Серийный", a.System.SerialNumber },
            new[] { "Установлена", a.System.InstallDate.ToString("yyyy-MM-dd") },
            new[] { "Последняя загрузка", a.System.LastBootTime.ToString("yyyy-MM-dd HH:mm:ss") },
            new[] { "Домен", a.System.Domain },
            new[] { "Рабочая группа", a.System.Workgroup }
        });

        // CPU
        Table(sb, "Процессор", new[] { "Параметр", "Значение" }, new[]
        {
            new[] { "Модель", a.Cpu.Name },
            new[] { "Производитель", a.Cpu.Manufacturer },
            new[] { "Ядер", a.Cpu.Cores.ToString() },
            new[] { "Потоков", a.Cpu.LogicalProcessors.ToString() },
            new[] { "Частота макс", $"{a.Cpu.MaxClockSpeedMhz} MHz" },
            new[] { "Частота текущая", $"{a.Cpu.CurrentClockSpeedMhz} MHz" },
            new[] { "L1 кэш", $"{a.Cpu.L1CacheKb} KB" },
            new[] { "L2 кэш", $"{a.Cpu.L2CacheKb} KB" },
            new[] { "L3 кэш", $"{a.Cpu.L3CacheKb} KB" },
            new[] { "Сокет", a.Cpu.Socket },
            new[] { "Загрузка", $"{a.Cpu.LoadPercentage}%" }
        });

        // RAM
        Table(sb, "Память", new[] { "Параметр", "Значение" }, new[]
        {
            new[] { "Всего", $"{a.Ram.TotalGb} GB" },
            new[] { "Свободно", $"{a.Ram.FreeGb} GB" },
            new[] { "Используется", $"{a.Ram.UsedGb} GB ({(a.Ram.TotalGb > 0 ? (a.Ram.UsedGb/a.Ram.TotalGb)*100 : 0):F1}%)" },
            new[] { "Слотов", $"{a.Ram.UsedSlots}/{a.Ram.TotalSlots}" }
        });

        if (a.Ram.Sticks.Count > 0)
            Table(sb, "Модули памяти", new[] { "Производитель", "Объём", "Скорость", "Тип", "Серийный" },
                a.Ram.Sticks.Select(s => new[] { s.Manufacturer, $"{s.CapacityGb} GB", $"{s.SpeedMhz} MHz", s.Type, s.Serial }));

        // Motherboard
        Table(sb, "Материнская плата", new[] { "Параметр", "Значение" }, new[]
        {
            new[] { "Производитель", a.Motherboard.Manufacturer },
            new[] { "Модель", a.Motherboard.Product },
            new[] { "Версия", a.Motherboard.Version },
            new[] { "Серийный", a.Motherboard.Serial }
        });

        // BIOS
        Table(sb, "BIOS", new[] { "Параметр", "Значение" }, new[]
        {
            new[] { "Производитель", a.Bios.Manufacturer },
            new[] { "Версия", a.Bios.Version },
            new[] { "Дата", a.Bios.ReleaseDate },
            new[] { "Серийный", a.Bios.Serial }
        });

        // Disks
        Table(sb, "Физические диски", new[] { "Модель", "Производитель", "Объём", "Тип", "Интерфейс", "Статус" },
            a.PhysicalDisks.Select(d => new[] { d.Model, d.Manufacturer, $"{d.SizeGb} GB", d.MediaType, d.Interface, d.Status }));

        Table(sb, "Логические диски", new[] { "Диск", "Метка", "ФС", "Объём", "Свободно" },
            a.LogicalDisks.Select(v => new[] { v.DeviceId, v.VolumeName, v.FileSystem, $"{v.SizeGb} GB", $"{v.FreeGb} GB" }));

        // GPUs
        Table(sb, "Видеокарты", new[] { "Модель", "Производитель", "VRAM", "Драйвер", "Разрешение", "Частота" },
            a.Gpus.Select(g => new[] { g.Name, g.Manufacturer, $"{g.VramGb} GB", g.DriverVersion, g.Resolution, $"{g.RefreshRateHz} Hz" }));

        // Network
        Table(sb, "Сетевые адаптеры", new[] { "Имя", "MAC", "IP", "DHCP", "Статус" },
            a.NetworkAdapters.Select(n => new[] { n.Name, n.Mac, string.Join(", ", n.IpAddresses), n.DhcpEnabled ? "Да" : "Нет", n.Status }));

        // Battery
        if (a.Battery != null)
            Table(sb, "Батарея", new[] { "Параметр", "Значение" }, new[]
            {
                new[] { "Название", a.Battery.Name },
                new[] { "Производитель", a.Battery.Manufacturer },
                new[] { "Серийный", a.Battery.Serial },
                new[] { "Ёмкость (design)", $"{a.Battery.DesignCapacityMwh} mWh" },
                new[] { "Ёмкость (полная)", $"{a.Battery.FullChargeCapacityMwh} mWh" },
                new[] { "Циклов", a.Battery.CycleCount.ToString() },
                new[] { "Здоровье", $"{a.Battery.HealthPercentage}%" },
                new[] { "Статус", a.Battery.Status }
            });

        // Installed Programs
        Table(sb, $"Установленные программы ({a.InstalledPrograms.Count})", new[] { "Название", "Версия", "Издатель", "Дата", "Размер" },
            a.InstalledPrograms.Take(100).Select(p => new[] { p.Name, p.Version, p.Publisher, p.InstallDate, $"{p.SizeMb:F1} MB" }));

        // Drivers
        Table(sb, $"Драйверы ({a.Drivers.Count})", new[] { "Устройство", "Производитель", "Версия", "Дата", "Подпись" },
            a.Drivers.Take(100).Select(d => new[] { d.DeviceName, d.Manufacturer, d.Version, d.DriverDate, d.IsSigned ? "Да" : "НЕТ" }));

        // Services
        Table(sb, $"Службы ({a.Services.Count})", new[] { "Имя", "Описание", "Статус", "Запуск", "Путь" },
            a.Services.Take(100).Select(s => new[] { s.DisplayName, s.Description, s.Status, s.StartType, s.PathName }));

        // Processes
        Table(sb, $"Процессы ({a.Processes.Count})", new[] { "PID", "Имя", "Путь", "CPU (с)", "RAM (MB)" },
            a.Processes.Take(100).Select(p => new[] { p.Pid.ToString(), p.Name, p.Path, p.CpuTime.ToString("F2"), p.WorkingSetMb.ToString() }));

        // Startup
        Table(sb, $"Автозагрузка ({a.StartupItems.Count})", new[] { "Имя", "Команда", "Источник" },
            a.StartupItems.Select(s => new[] { s.Name, s.Command, s.Source }));

        // Scheduled Tasks
        Table(sb, $"Задачи ({a.ScheduledTasks.Count})", new[] { "Задача", "Статус", "След. запуск", "Последний" },
            a.ScheduledTasks.Take(100).Select(t => new[] { t.TaskName, t.Status, t.NextRunTime, t.LastRunTime }));

        // Updates
        Table(sb, $"Обновления ({a.Updates.Count})", new[] { "KB", "Описание", "Дата", "Установил" },
            a.Updates.OrderByDescending(u => u.InstalledOn).Select(u => new[] { u.HotFixId, u.Description, u.InstalledOn, u.InstalledBy }));

        // Connections
        Table(sb, $"Соединения ({a.Connections.Count})", new[] { "Протокол", "Локальный", "Удалённый", "Статус" },
            a.Connections.Take(100).Select(c => new[] { c.Protocol, $"{c.LocalAddress}:{c.LocalPort}", $"{c.RemoteAddress}:{c.RemotePort}", c.State }));

        // Open Ports
        Table(sb, $"Открытые порты ({a.OpenPorts.Count})", new[] { "Порт", "Протокол", "Статус" },
            a.OpenPorts.Select(p => new[] { p.Port.ToString(), p.Protocol, p.State }));

        // Users
        Table(sb, "Учётные записи", new[] { "Имя", "Полное имя", "Статус", "Группы" },
            a.UserAccounts.Select(u => new[] { u.Name, u.FullName, u.Disabled ? "Отключена" : "Активна", string.Join(", ", u.Groups) }));

        // Groups
        Table(sb, "Группы", new[] { "Имя", "Описание", "Члены" },
            a.UserGroups.Select(g => new[] { g.Name, g.Description, string.Join(", ", g.Members) }));

        // Shared Folders
        Table(sb, "Общие папки", new[] { "Имя", "Путь", "Тип", "Пользователей" },
            a.SharedFolders.Select(s => new[] { s.Name, s.Path, s.Type, s.CurrentUsers.ToString() }));

        // Firewall
        Table(sb, "Firewall", new[] { "Профиль", "Статус" }, new[]
        {
            new[] { "Domain", a.Firewall.DomainProfileEnabled ? "Включен" : "Отключен" },
            new[] { "Private", a.Firewall.PrivateProfileEnabled ? "Включен" : "Отключен" },
            new[] { "Public", a.Firewall.PublicProfileEnabled ? "Включен" : "Отключен" }
        });

        // Performance
        Table(sb, "Производительность", new[] { "Показатель", "Значение" }, new[]
        {
            new[] { "CPU", $"{a.Performance.CpuUsagePercent:F1}%" },
            new[] { "RAM", $"{a.Performance.RamUsagePercent:F1}% ({a.Performance.RamUsedGb:F1}/{a.Performance.RamTotalGb:F1} GB)" }
        });

        // Event Logs
        if (a.SystemLog.Count > 0)
            Table(sb, $"System Log (последние {a.SystemLog.Count})", new[] { "Время", "Тип", "Источник", "ID", "Сообщение" },
                a.SystemLog.Select(e => new[] { e.TimeGenerated, e.EntryType, e.Source, e.EventId.ToString(), e.Message.Length > 100 ? e.Message.Substring(0, 100) + "..." : e.Message }));

        if (a.ApplicationLog.Count > 0)
            Table(sb, $"Application Log (последние {a.ApplicationLog.Count})", new[] { "Время", "Тип", "Источник", "ID", "Сообщение" },
                a.ApplicationLog.Select(e => new[] { e.TimeGenerated, e.EntryType, e.Source, e.EventId.ToString(), e.Message.Length > 100 ? e.Message.Substring(0, 100) + "..." : e.Message }));

        // DirectX
        Table(sb, "DirectX", new[] { "Параметр", "Значение" }, new[]
        {
            new[] { "Версия", a.DirectX.Version },
            new[] { "Driver Model", a.DirectX.DriverModel },
            new[] { "Feature Levels", a.DirectX.FeatureLevels }
        });

        // Printers
        if (a.Printers.Count > 0)
            Table(sb, "Принтеры", new[] { "Имя", "Драйвер", "Порт", "По умолчанию", "Статус" },
                a.Printers.Select(p => new[] { p.Name, p.DriverName, p.PortName, p.IsDefault ? "Да" : "Нет", p.Status }));

        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    private static void Table(StringBuilder sb, string title, string[] headers, IEnumerable<string[]> rows)
    {
        sb.AppendLine($"<h2>{title}</h2><table><tr>");
        foreach (var h in headers) sb.Append($"<th>{h}</th>");
        sb.AppendLine("</tr>");
        foreach (var r in rows)
        {
            sb.Append("<tr>");
            foreach (var c in r) sb.Append($"<td>{c}</td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</table>");
    }
}