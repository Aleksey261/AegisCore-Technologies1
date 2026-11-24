using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Win32;

namespace AegisCore.Services;

public class SystemInfo
{
    // Железо
    public string ComputerName { get; set; } = string.Empty;
    public string OSVersion { get; set; } = string.Empty;
    public string CPUName { get; set; } = string.Empty;
    public long TotalRAM_GB { get; set; }
    public string DiskModel { get; set; } = string.Empty;
    
    // Сеть
    public List<NetworkInterfaceInfo> NetworkInterfaces { get; set; } = new();
    public List<OpenPortInfo> OpenPorts { get; set; } = new();
    public List<ActiveConnectionInfo> ActiveConnections { get; set; } = new();
    
    // ПО и безопасность
    public List<InstalledSoftware> InstalledSoftware { get; set; } = new();
    public List<RunningProcess> RunningProcesses { get; set; } = new();
    public List<WindowsServiceInfo> Services { get; set; } = new();
    
    // Состояние безопасности
    public bool FirewallEnabled { get; set; }
    public bool AntivirusPresent { get; set; }
    public string AntivirusName { get; set; } = string.Empty;
    public bool WindowsUpdateEnabled { get; set; }
    public DateTime? LastUpdateDate { get; set; }
    public int CriticalUpdatesMissing { get; set; }
    
    // Уязвимости
    public List<VulnerabilityInfo> Vulnerabilities { get; set; } = new();
}

public class NetworkInterfaceInfo
{
    public string Name { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
    public long Speed_Mbps { get; set; }
    public bool IsUp { get; set; }
}

public class OpenPortInfo
{
    public int Port { get; set; }
    public string Protocol { get; set; } = "TCP";
    public string ServiceName { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
}

public class ActiveConnectionInfo
{
    public string LocalAddress { get; set; } = string.Empty;
    public int LocalPort { get; set; }
    public string RemoteAddress { get; set; } = string.Empty;
    public int RemotePort { get; set; }
    public string State { get; set; } = string.Empty;
}

public class InstalledSoftware
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public DateTime? InstallDate { get; set; }
}

public class RunningProcess
{
    public string Name { get; set; } = string.Empty;
    public int Id { get; set; }
    public long Memory_MB { get; set; }
    public string? FileName { get; set; }
}

public class WindowsServiceInfo
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StartType { get; set; } = string.Empty;
}

public class VulnerabilityInfo
{
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}

public class SystemCollectorService
{
    private readonly ILogger<SystemCollectorService>? _logger;
    
    // Опасные порты по ФСТЭК
    private static readonly HashSet<int> RiskyPorts = new() 
    { 
        21, 23, 135, 139, 445, 1433, 3306, 3389, 5432, 5900, 8080, 8443 
    };
    
    // Устаревшее ПО (примеры)
    private static readonly string[] OutdatedSoftwarePatterns = new[]
    {
        "Adobe Flash", "Adobe Reader 9", "Adobe Reader X", "Adobe Reader XI",
        "Java 6", "Java 7", "Java 8 Update",
        "Internet Explorer",
        "Windows XP", "Windows Vista", "Windows 7",
        "Office 2007", "Office 2010", "Office 2013",
        "OpenSSL 1.0", "OpenSSL 1.1.0", "OpenSSL 1.1.1"
    };

    public SystemCollectorService(ILogger<SystemCollectorService>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Главная функция: собирает полную информацию о системе
    /// </summary>
    public async Task<SystemInfo> CollectSystemInfoAsync()
    {
        var info = new SystemInfo();
        
        info.ComputerName = Environment.MachineName;
        info.OSVersion = Environment.OSVersion.VersionString;
        
        // Собираем данные параллельно для скорости
        var tasks = new[]
        {
            Task.Run(() => CollectHardwareInfo(info)),
            Task.Run(() => CollectNetworkInfo(info)),
            Task.Run(() => CollectSoftwareInfo(info)),
            Task.Run(() => CollectSecurityInfo(info)),
            Task.Run(() => CollectProcessesAndServices(info))
        };
        
        await Task.WhenAll(tasks);
        
        // Анализируем уязвимости на основе собранных данных
        AnalyzeVulnerabilities(info);
        
        return info;
    }

    private void CollectHardwareInfo(SystemInfo info)
    {
        try
        {
            // CPU через WMI
            using var searcher = new System.Management.ManagementObjectSearcher(
                "SELECT Name FROM Win32_Processor");
            foreach (var obj in searcher.Get())
            {
                info.CPUName = obj["Name"]?.ToString() ?? "Unknown";
                break;
            }
        }
        catch { info.CPUName = "Unknown"; }

        try
        {
            // RAM
            using var searcher = new System.Management.ManagementObjectSearcher(
                "SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem");
            foreach (var obj in searcher.Get())
            {
                info.TotalRAM_GB = Convert.ToInt64(obj["TotalVisibleMemorySize"]) / (1024 * 1024);
                break;
            }
        }
        catch { info.TotalRAM_GB = 0; }
    }

    private void CollectNetworkInfo(SystemInfo info)
    {
        // Сетевые интерфейсы
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up) continue;
            
            var addresses = ni.GetIPProperties().UnicastAddresses
                .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(a => a.Address.ToString())
                .ToList();
            
            info.NetworkInterfaces.Add(new NetworkInterfaceInfo
            {
                Name = ni.Name,
                IPAddress = string.Join(", ", addresses),
                Speed_Mbps = ni.Speed / 1_000_000,
                IsUp = true
            });
        }

        // Активные TCP-соединения и открытые порты
        var ipProps = IPGlobalProperties.GetIPGlobalProperties();
        
        var tcpConnections = ipProps.GetActiveTcpConnections();
        var tcpListeners = ipProps.GetActiveTcpListeners();
        
        var seenPorts = new HashSet<int>();
        
        foreach (var conn in tcpConnections)
        {
            info.ActiveConnections.Add(new ActiveConnectionInfo
            {
                LocalAddress = conn.LocalEndPoint.Address.ToString(),
                LocalPort = conn.LocalEndPoint.Port,
                RemoteAddress = conn.RemoteEndPoint.Address.ToString(),
                RemotePort = conn.RemoteEndPoint.Port,
                State = conn.State.ToString()
            });
            
            if (seenPorts.Add(conn.LocalEndPoint.Port))
            {
                info.OpenPorts.Add(new OpenPortInfo
                {
                    Port = conn.LocalEndPoint.Port,
                    Protocol = "TCP",
                    ServiceName = GetServiceName(conn.LocalEndPoint.Port),
                    ProcessName = GetProcessNameByPort(conn.LocalEndPoint.Port)
                });
            }
        }
        
        foreach (var listener in tcpListeners)
        {
            if (seenPorts.Add(listener.Port))
            {
                info.OpenPorts.Add(new OpenPortInfo
                {
                    Port = listener.Port,
                    Protocol = "TCP",
                    ServiceName = GetServiceName(listener.Port),
                    ProcessName = GetProcessNameByPort(listener.Port)
                });
            }
        }
    }

    private void CollectSoftwareInfo(SystemInfo info)
    {
        // Читаем из реестра установленное ПО
        var uninstallKeys = new[]
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        };
        
        foreach (var keyPath in uninstallKeys)
        {
            using var key = Registry.LocalMachine.OpenSubKey(keyPath);
            if (key == null) continue;
            
            foreach (var subKeyName in key.GetSubKeyNames())
            {
                using var subKey = key.OpenSubKey(subKeyName);
                if (subKey == null) continue;
                
                var name = subKey.GetValue("DisplayName")?.ToString();
                if (string.IsNullOrEmpty(name)) continue;
                
                var version = subKey.GetValue("DisplayVersion")?.ToString() ?? "";
                var publisher = subKey.GetValue("Publisher")?.ToString() ?? "";
                
                DateTime? installDate = null;
                var dateStr = subKey.GetValue("InstallDate")?.ToString();
                if (!string.IsNullOrEmpty(dateStr) && dateStr.Length == 8)
                {
                    if (DateTime.TryParseExact(dateStr, "yyyyMMdd", null, 
                        System.Globalization.DateTimeStyles.None, out var dt))
                    {
                        installDate = dt;
                    }
                }
                
                info.InstalledSoftware.Add(new InstalledSoftware
                {
                    Name = name,
                    Version = version,
                    Publisher = publisher,
                    InstallDate = installDate
                });
            }
        }
    }

    private void CollectSecurityInfo(SystemInfo info)
    {
        // Брандмауэр
        try
        {
            using var searcher = new System.Management.ManagementObjectSearcher(
                "SELECT Enabled FROM Win32_FirewallProduct");
            foreach (var obj in searcher.Get())
            {
                info.FirewallEnabled = Convert.ToBoolean(obj["Enabled"]);
                break;
            }
        }
        catch
        {
            // Fallback через реестр
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile");
            if (key != null)
            {
                var val = key.GetValue("EnableFirewall");
                info.FirewallEnabled = val != null && Convert.ToInt32(val) == 1;
            }
        }

        // Антивирус
        try
        {
            using var searcher = new System.Management.ManagementObjectSearcher(
                @"root\SecurityCenter2", "SELECT displayName FROM AntiVirusProduct");
            foreach (var obj in searcher.Get())
            {
                info.AntivirusPresent = true;
                info.AntivirusName = obj["displayName"]?.ToString() ?? "Unknown";
                break;
            }
        }
        catch { info.AntivirusPresent = false; }

        // Windows Update (упрощенно — через реестр)
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update");
            if (key != null)
            {
                var auOptions = key.GetValue("AUOptions");
                info.WindowsUpdateEnabled = auOptions != null && Convert.ToInt32(auOptions) >= 2;
            }
        }
        catch { info.WindowsUpdateEnabled = false; }
    }

    private void CollectProcessesAndServices(SystemInfo info)
    {
        // Процессы
        foreach (var proc in Process.GetProcesses())
        {
            try
            {
                info.RunningProcesses.Add(new RunningProcess
                {
                    Name = proc.ProcessName,
                    Id = proc.Id,
                    Memory_MB = proc.WorkingSet64 / (1024 * 1024),
                    FileName = proc.MainModule?.FileName
                });
            }
            catch { /* Нет доступа к некоторым процессам */ }
        }

        // Службы Windows
        try
        {
            using var searcher = new System.Management.ManagementObjectSearcher(
                "SELECT Name, DisplayName, State, StartMode FROM Win32_Service");
            foreach (var obj in searcher.Get())
            {
                info.Services.Add(new WindowsServiceInfo
                {
                    Name = obj["Name"]?.ToString() ?? "",
                    DisplayName = obj["DisplayName"]?.ToString() ?? "",
                    Status = obj["State"]?.ToString() ?? "",
                    StartType = obj["StartMode"]?.ToString() ?? ""
                });
            }
        }
        catch { }
    }

    /// <summary>
    /// Анализирует собранные данные и выявляет уязвимости
    /// </summary>
    private void AnalyzeVulnerabilities(SystemInfo info)
    {
        // 1. Опасные открытые порты
        foreach (var port in info.OpenPorts.Where(p => RiskyPorts.Contains(p.Port)))
        {
            info.Vulnerabilities.Add(new VulnerabilityInfo
            {
                Type = "Открытый опасный порт",
                Severity = port.Port == 23 || port.Port == 445 ? "Critical" : "High",
                Description = $"Порт {port.Port} ({port.ServiceName}) открыт и доступен",
                Recommendation = $"Закрыть порт {port.Port} через брандмауэр или отключить службу {port.ServiceName}"
            });
        }

        // 2. Устаревшее ПО
        foreach (var sw in info.InstalledSoftware)
        {
            foreach (var pattern in OutdatedSoftwarePatterns)
            {
                if (sw.Name.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    info.Vulnerabilities.Add(new VulnerabilityInfo
                    {
                        Type = "Устаревшее ПО",
                        Severity = "High",
                        Description = $"Установлено устаревшее ПО: {sw.Name} {sw.Version}",
                        Recommendation = $"Обновить {sw.Name} до актуальной версии"
                    });
                    break;
                }
            }
        }

        // 3. Отключенный брандмауэр
        if (!info.FirewallEnabled)
        {
            info.Vulnerabilities.Add(new VulnerabilityInfo
            {
                Type = "Отключенный брандмауэр",
                Severity = "Critical",
                Description = "Брандмауэр Windows отключен",
                Recommendation = "Включить брандмауэр Windows через Панель управления"
            });
        }

        // 4. Отсутствие антивируса
        if (!info.AntivirusPresent)
        {
            info.Vulnerabilities.Add(new VulnerabilityInfo
            {
                Type = "Отсутствие антивируса",
                Severity = "Critical",
                Description = "Антивирусное ПО не обнаружено",
                Recommendation = "Установить антивирусное ПО (включая встроенный Windows Defender)"
            });
        }

        // 5. Отключенные обновления
        if (!info.WindowsUpdateEnabled)
        {
            info.Vulnerabilities.Add(new VulnerabilityInfo
            {
                Type = "Отключенные обновления",
                Severity = "High",
                Description = "Автоматические обновления Windows отключены",
                Recommendation = "Включить автоматические обновления Windows"
            });
        }

        // 6. Подозрительные процессы
        var suspiciousProcesses = new[] { "mimikatz", "nc.exe", "netcat", "psexec", "cobalt" };
        foreach (var proc in info.RunningProcesses)
        {
            foreach (var susp in suspiciousProcesses)
            {
                if (proc.Name.Contains(susp, StringComparison.OrdinalIgnoreCase))
                {
                    info.Vulnerabilities.Add(new VulnerabilityInfo
                    {
                        Type = "Подозрительный процесс",
                        Severity = "Critical",
                        Description = $"Обнаружен подозрительный процесс: {proc.Name} (PID: {proc.Id})",
                        Recommendation = "Немедленно завершить процесс и провести антивирусную проверку"
                    });
                    break;
                }
            }
        }
    }

    private string GetServiceName(int port) => port switch
    {
        21 => "FTP", 22 => "SSH", 23 => "Telnet", 25 => "SMTP",
        53 => "DNS", 80 => "HTTP", 110 => "POP3", 135 => "MSRPC",
        139 => "NetBIOS", 443 => "HTTPS", 445 => "SMB", 993 => "IMAPS",
        995 => "POP3S", 1433 => "MSSQL", 1521 => "Oracle", 3306 => "MySQL",
        3389 => "RDP", 5432 => "PostgreSQL", 5900 => "VNC",
        8080 => "HTTP-Proxy", 8443 => "HTTPS-Alt",
        _ => "Unknown"
    };

    private string GetProcessNameByPort(int port)
    {
        try
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var connections = properties.GetActiveTcpConnections()
                .Where(c => c.LocalEndPoint.Port == port)
                .ToList();
            
            if (connections.Count == 0) return "Unknown";
            
            var pid = -1;
            // Получаем PID через netstat (упрощенно)
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "netstat",
                    Arguments = "-ano",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(2000);
            
            foreach (var line in output.Split('\n'))
            {
                if (line.Contains($":{port} ") || line.Contains($":{port}\t"))
                {
                    var parts = line.Trim().Split(new[] { ' ', '\t' }, 
                        StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 5 && int.TryParse(parts[4], out var p))
                    {
                        pid = p;
                        break;
                    }
                }
            }
            
            if (pid > 0)
            {
                var proc = Process.GetProcessById(pid);
                return proc.ProcessName;
            }
        }
        catch { }
        return "Unknown";
    }
}