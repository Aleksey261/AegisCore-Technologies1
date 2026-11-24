using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Win32;
using AegisCore.Models;

namespace AegisCore.Services;

public class ScannerService
{
    private readonly JsonDataStore _store;
    private static readonly int[] DefaultPorts = { 21, 22, 23, 25, 53, 80, 135, 139, 443, 445, 3389, 5985, 502, 102, 4840, 44818 };

    public ScannerService(JsonDataStore store) => _store = store;

    public async Task<ScanJob> StartScanAsync(string target = "127.0.0.1", string scanMode = "C1")
    {
        var job = new ScanJob
        {
            JobId = $"JOB-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Name = $"Scan {target}",
            Description = "C# scanner job",
            Status = "RUNNING",
            StartedAt = DateTime.UtcNow,
            CurrentStep = "Сбор данных ПК и проверка портов",
            Progress = 10,
            ScanMode = scanMode
        };

        var jobs = _store.ScanJobs;
        jobs.Add(job);
        _store.SaveScanJobs(jobs);

        var results = _store.ScanResults;
        var scanItems = _store.PcScanItems;
        var scanRunId = job.JobId;
        scanItems.AddRange(CollectPcInventory(scanRunId));
        _store.SavePcScanItems(scanItems);

        foreach (var port in DefaultPorts)
        {
            if (await IsPortOpenAsync(target, port, TimeSpan.FromMilliseconds(350)))
            {
                results.Add(new ScanResult
                {
                    ScanJobId = job.JobId,
                    Host = target,
                    Port = port,
                    Protocol = "TCP",
                    Service = GuessService(port),
                    Product = GuessProduct(port),
                    Version = "unknown",
                    VulnStatus = "UNKNOWN",
                    FstecSeverity = PortRisk(port),
                    FstecDescription = $"Открыт порт {port}/{GuessService(port)}. Требуется проверка необходимости публикации сервиса.",
                    Remediation = "Закрыть неиспользуемый порт или ограничить доступ межсетевым экраном."
                });
            }
        }

        job.PortsScanned = DefaultPorts.Length;
        job.HostsDiscovered = 1;
        job.Progress = 100;
        job.Status = "COMPLETED";
        job.CurrentStep = "Завершено";
        job.CompletedAt = DateTime.UtcNow;
        job.VulnerabilitiesFound = results.Count(r => r.ScanJobId == job.JobId);

        _store.SaveScanResults(results);
        _store.SaveScanJobs(jobs.Select(j => j.Id == job.Id ? job : j).ToList());
        return job;
    }

    public List<PcScanItem> CollectPcInventory(string scanRunId)
    {
        var items = new List<PcScanItem>();
        void Add(string category, string key, string value, string details = "") => items.Add(new PcScanItem { ScanRunId = scanRunId, Category = category, ItemKey = key, ItemValue = value, Details = details });

        Add("System", "MachineName", Environment.MachineName);
        Add("System", "OSVersion", Environment.OSVersion.ToString());
        Add("System", "UserName", Environment.UserName);
        Add("Hardware", "ProcessorCount", Environment.ProcessorCount.ToString());
        Add("Hardware", "Is64BitOperatingSystem", Environment.Is64BitOperatingSystem.ToString());
        Add("Hardware", "SystemDirectory", Environment.SystemDirectory);

        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
        {
            Add("Disk", drive.Name, $"{drive.TotalSize / 1024 / 1024 / 1024} GB", drive.DriveFormat);
        }

        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            var props = ni.GetIPProperties();
            var ips = string.Join(", ", props.UnicastAddresses.Select(a => a.Address.ToString()));
            Add("Network", ni.Name, ni.GetPhysicalAddress().ToString(), $"{ni.NetworkInterfaceType}; {ni.OperationalStatus}; {ips}");
        }

        foreach (var p in Process.GetProcesses().OrderBy(p => p.ProcessName).Take(250))
        {
            Add("Process", p.ProcessName, p.Id.ToString());
        }

        foreach (var app in ReadInstalledPrograms().Take(1000))
        {
            Add("Software", app.Name, app.Version ?? string.Empty, app.Publisher ?? string.Empty);
        }

        return items;
    }

    private static IEnumerable<(string Name, string? Version, string? Publisher)> ReadInstalledPrograms()
    {
        if (!OperatingSystem.IsWindows()) yield break;
        var paths = new[]
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        };
        foreach (var root in new[] { Registry.LocalMachine, Registry.CurrentUser })
        foreach (var path in paths)
        using (var key = root.OpenSubKey(path))
        {
            if (key == null) continue;
            foreach (var subName in key.GetSubKeyNames())
            using (var sub = key.OpenSubKey(subName))
            {
                var name = sub?.GetValue("DisplayName")?.ToString();
                if (string.IsNullOrWhiteSpace(name)) continue;
                yield return (name, sub?.GetValue("DisplayVersion")?.ToString(), sub?.GetValue("Publisher")?.ToString());
            }
        }
    }

    private static async Task<bool> IsPortOpenAsync(string host, int port, TimeSpan timeout)
    {
        using var client = new TcpClient();
        try
        {
            var connectTask = client.ConnectAsync(host, port);
            var timeoutTask = Task.Delay(timeout);
            var completed = await Task.WhenAny(connectTask, timeoutTask);
            return completed == connectTask && client.Connected;
        }
        catch { return false; }
    }

    private static string GuessService(int port) => port switch
    {
        21 => "FTP", 22 => "SSH", 23 => "Telnet", 25 => "SMTP", 53 => "DNS", 80 => "HTTP", 135 => "RPC", 139 => "NetBIOS", 443 => "HTTPS", 445 => "SMB", 3389 => "RDP", 5985 => "WinRM", 502 => "Modbus/TCP", 102 => "Siemens S7", 4840 => "OPC UA", 44818 => "EtherNet/IP", _ => "Unknown"
    };
    private static string GuessProduct(int port) => port switch
    {
        502 or 102 or 4840 or 44818 => "Industrial network service",
        3389 => "Microsoft Remote Desktop",
        445 => "Microsoft SMB",
        5985 => "Microsoft WinRM",
        _ => GuessService(port)
    };
    private static string PortRisk(int port) => port switch
    {
        23 or 502 or 102 or 4840 or 44818 => "Высокий",
        445 or 3389 or 5985 => "Средний",
        _ => "Низкий"
    };
}
