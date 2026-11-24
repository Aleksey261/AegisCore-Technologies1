using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace AegisCore.Services;

// ==================== DTO ====================
public class FullSystemAudit
{
    public DateTime CollectedAt { get; set; }
    public ComputerInfo Computer { get; set; } = new();
    public BiosInfo Bios { get; set; } = new();
    public List<DiskInfo> Disks { get; set; } = new();
    public List<VolumeInfo> Volumes { get; set; } = new();
    public List<SmartInfo> SmartStatus { get; set; } = new();
    public List<GpuInfo> Gpus { get; set; } = new();
    public List<NetworkAdapterInfo> NetworkAdapters { get; set; } = new();
    public List<StartupItemInfo> StartupItems { get; set; } = new();
    public List<ScheduledTaskInfo> ScheduledTasks { get; set; } = new();
    public List<DriverInfo> Drivers { get; set; } = new();
    public List<WindowsUpdateInfo> InstalledUpdates { get; set; } = new();
    public List<UserAccountInfo> UserAccounts { get; set; } = new();
    public List<SharedFolderInfo> SharedFolders { get; set; } = new();
    public AuditSummary Summary { get; set; } = new();
}

public class ComputerInfo
{
    public string Name { get; set; } = "";
    public string Os { get; set; } = "";
    public string OsArchitecture { get; set; } = "";
    public string Cpu { get; set; } = "";
    public int CpuCores { get; set; }
    public int CpuLogical { get; set; }
    public double TotalRamGb { get; set; }
    public double FreeRamGb { get; set; }
    public string Manufacturer { get; set; } = "";
    public string Model { get; set; } = "";
    public string SystemType { get; set; } = "";
    public DateTime LastBoot { get; set; }
}

public class BiosInfo
{
    public string Manufacturer { get; set; } = "";
    public string Version { get; set; } = "";
    public string ReleaseDate { get; set; } = "";
    public string Serial { get; set; } = "";
    public string BoardManufacturer { get; set; } = "";
    public string BoardProduct { get; set; } = "";
    public string BoardSerial { get; set; } = "";
}

public class DiskInfo
{
    public string Model { get; set; } = "";
    public double SizeGb { get; set; }
    public string MediaType { get; set; } = "";
    public string Interface { get; set; } = "";
    public string Serial { get; set; } = "";
    public string Status { get; set; } = "";
}

public class VolumeInfo
{
    public string DeviceId { get; set; } = "";
    public string VolumeName { get; set; } = "";
    public string FileSystem { get; set; } = "";
    public double SizeGb { get; set; }
    public double FreeGb { get; set; }
}

public class SmartInfo
{
    public bool Active { get; set; }
    public bool PredictFailure { get; set; }
}

public class GpuInfo
{
    public string Name { get; set; } = "";
    public double VramGb { get; set; }
    public string DriverVersion { get; set; } = "";
    public string Status { get; set; } = "";
    public string Resolution { get; set; } = "";
}

public class NetworkAdapterInfo
{
    public string Description { get; set; } = "";
    public string Mac { get; set; } = "";
    public string IpAddresses { get; set; } = "";
    public bool DhcpEnabled { get; set; }
    public string DhcpServer { get; set; } = "";
}

public class StartupItemInfo
{
    public string Name { get; set; } = "";
    public string Command { get; set; } = "";
    public string Source { get; set; } = "";
}

public class ScheduledTaskInfo
{
    public string TaskName { get; set; } = "";
    public string NextRun { get; set; } = "";
    public string Status { get; set; } = "";
}

public class DriverInfo
{
    public string DeviceName { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string Version { get; set; } = "";
    public string DriverDate { get; set; } = "";
    public bool IsSigned { get; set; }
}

public class WindowsUpdateInfo
{
    public string HotFixId { get; set; } = "";
    public string Description { get; set; } = "";
    public string InstalledOn { get; set; } = "";
}

public class UserAccountInfo
{
    public string Name { get; set; } = "";
    public string FullName { get; set; } = "";
    public bool Disabled { get; set; }
    public bool PasswordRequired { get; set; }
    public string Sid { get; set; } = "";
    public bool IsAdministrator { get; set; }
}

public class SharedFolderInfo
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string Type { get; set; } = "";
    public string Description { get; set; } = "";
}

public class AuditSummary
{
    public int DiskCount { get; set; }
    public int VolumeCount { get; set; }
    public int GpuCount { get; set; }
    public int StartupItemCount { get; set; }
    public int ScheduledTaskCount { get; set; }
    public int DriverCount { get; set; }
    public int UnsignedDriverCount { get; set; }
    public int UpdateCount { get; set; }
    public int UserCount { get; set; }
    public int AdminCount { get; set; }
    public int EnabledUserCount { get; set; }
    public int ShareCount { get; set; }
    public bool SmartPredictFailure { get; set; }
}

// ==================== СЕРВИС СБОРА ====================
public class FullSystemAuditService
{
    public Task<FullSystemAudit> CollectAsync() => Task.Run(Collect);

    public FullSystemAudit Collect()
    {
        var audit = new FullSystemAudit { CollectedAt = DateTime.Now };
        Safe(() => CollectComputer(audit));
        Safe(() => CollectBios(audit));
        Safe(() => CollectDisks(audit));
        Safe(() => CollectGpus(audit));
        Safe(() => CollectNetwork(audit));
        Safe(() => CollectStartup(audit));
        Safe(() => CollectScheduledTasks(audit));
        Safe(() => CollectDrivers(audit));
        Safe(() => CollectUpdates(audit));
        Safe(() => CollectAccounts(audit));
        Safe(() => CollectShares(audit));
        Safe(() => BuildSummary(audit));
        return audit;
    }

    private static void Safe(Action action)
    {
        try { action(); }
        catch { /* раздел недоступен без прав — пропускаем молча */ }
    }

    // ---------- helpers ----------
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

    private static double Gb(double bytes) => Math.Round(bytes / 1073741824d, 1);

    private static DateTime ParseCimDate(string cim)
    {
        try { return ManagementDateTimeConverter.ToDateTime(cim); }
        catch { return DateTime.MinValue; }
    }

    // ---------- разделы ----------
    private static void CollectComputer(FullSystemAudit a)
    {
        var os = Wql("SELECT Caption, OSArchitecture, TotalVisibleMemorySize, FreePhysicalMemory, LastBootUpTime FROM Win32_OperatingSystem").FirstOrDefault();
        var cpu = Wql("SELECT Name, NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor").FirstOrDefault();
        var cs = Wql("SELECT Manufacturer, Model, SystemType FROM Win32_ComputerSystem").FirstOrDefault();
        a.Computer = new ComputerInfo
        {
            Name = Environment.MachineName,
            Os = os != null ? S(os, "Caption") : Environment.OSVersion.ToString(),
            OsArchitecture = os != null ? S(os, "OSArchitecture") : "",
            Cpu = cpu != null ? S(cpu, "Name") : "",
            CpuCores = cpu != null ? (int)D(cpu, "NumberOfCores") : 0,
            CpuLogical = cpu != null ? (int)D(cpu, "NumberOfLogicalProcessors") : 0,
            TotalRamGb = os != null ? Gb(D(os, "TotalVisibleMemorySize") * 1024) : 0,
            FreeRamGb = os != null ? Gb(D(os, "FreePhysicalMemory") * 1024) : 0,
            Manufacturer = cs != null ? S(cs, "Manufacturer") : "",
            Model = cs != null ? S(cs, "Model") : "",
            SystemType = cs != null ? S(cs, "SystemType") : "",
            LastBoot = os != null ? ParseCimDate(S(os, "LastBootUpTime")) : DateTime.MinValue
        };
    }

    private static void CollectBios(FullSystemAudit a)
    {
        var bios = Wql("SELECT Manufacturer, SMBIOSBIOSVersion, ReleaseDate, SerialNumber FROM Win32_BIOS").FirstOrDefault();
        var board = Wql("SELECT Manufacturer, Product, SerialNumber FROM Win32_BaseBoard").FirstOrDefault();
        a.Bios = new BiosInfo
        {
            Manufacturer = bios != null ? S(bios, "Manufacturer") : "",
            Version = bios != null ? S(bios, "SMBIOSBIOSVersion") : "",
            ReleaseDate = bios != null ? S(bios, "ReleaseDate") : "",
            Serial = bios != null ? S(bios, "SerialNumber") : "",
            BoardManufacturer = board != null ? S(board, "Manufacturer") : "",
            BoardProduct = board != null ? S(board, "Product") : "",
            BoardSerial = board != null ? S(board, "SerialNumber") : ""
        };
    }

    private static void CollectDisks(FullSystemAudit a)
    {
        foreach (var d in Wql("SELECT Model, Size, MediaType, InterfaceType, SerialNumber, Status FROM Win32_DiskDrive"))
            a.Disks.Add(new DiskInfo
            {
                Model = S(d, "Model"), SizeGb = Gb(D(d, "Size")), MediaType = S(d, "MediaType"),
                Interface = S(d, "InterfaceType"), Serial = S(d, "SerialNumber"), Status = S(d, "Status")
            });

        foreach (var v in Wql("SELECT DeviceId, Size, FreeSpace, FileSystem, VolumeName FROM Win32_LogicalDisk WHERE DriveType=3"))
            a.Volumes.Add(new VolumeInfo
            {
                DeviceId = S(v, "DeviceId"), VolumeName = S(v, "VolumeName"), FileSystem = S(v, "FileSystem"),
                SizeGb = Gb(D(v, "Size")), FreeGb = Gb(D(v, "FreeSpace"))
            });

        foreach (var s in Wql("SELECT Active, PredictFailure FROM MSStorageDriver_FailurePredictionStatus", @"root\wmi"))
            a.SmartStatus.Add(new SmartInfo { Active = S(s, "Active") == "True", PredictFailure = S(s, "PredictFailure") == "True" });
    }

    private static void CollectGpus(FullSystemAudit a)
    {
        foreach (var g in Wql("SELECT Name, AdapterRAM, DriverVersion, Status, CurrentHorizontalResolution, CurrentVerticalResolution FROM Win32_VideoController"))
            a.Gpus.Add(new GpuInfo
            {
                Name = S(g, "Name"), VramGb = Gb(D(g, "AdapterRAM")), DriverVersion = S(g, "DriverVersion"),
                Status = S(g, "Status"), Resolution = $"{S(g, "CurrentHorizontalResolution")}x{S(g, "CurrentVerticalResolution")}"
            });
    }

    private static void CollectNetwork(FullSystemAudit a)
    {
        foreach (var n in Wql("SELECT Description, MACAddress, IPAddress, DHCPEnabled, DHCPServer FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled=True"))
            a.NetworkAdapters.Add(new NetworkAdapterInfo
            {
                Description = S(n, "Description"),
                Mac = S(n, "MACAddress"),
                IpAddresses = n["IPAddress"] is string[] ips ? string.Join(", ", ips) : "",
                DhcpEnabled = S(n, "DHCPEnabled") == "True",
                DhcpServer = S(n, "DHCPServer")
            });
    }

    private static void CollectStartup(FullSystemAudit a)
    {
        AddRunKey(a, Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "HKLM Run");
        AddRunKey(a, Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run", "HKLM Run (x86)");
        AddRunKey(a, Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "HKCU Run");
        AddStartupFolder(a, Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Startup (пользователь)");
        AddStartupFolder(a, Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), "Startup (все пользователи)");
    }

    private static void AddRunKey(FullSystemAudit a, RegistryKey root, string path, string source)
    {
        using var key = root.OpenSubKey(path);
        if (key == null) return;
        foreach (var name in key.GetValueNames())
            a.StartupItems.Add(new StartupItemInfo { Name = name, Command = key.GetValue(name)?.ToString() ?? "", Source = source });
    }

    private static void AddStartupFolder(FullSystemAudit a, string folder, string source)
    {
        if (!System.IO.Directory.Exists(folder)) return;
        foreach (var f in System.IO.Directory.GetFiles(folder))
            a.StartupItems.Add(new StartupItemInfo { Name = System.IO.Path.GetFileName(f), Command = f, Source = source });
    }

    private static void CollectScheduledTasks(FullSystemAudit a)
    {
        var psi = new System.Diagnostics.ProcessStartInfo("schtasks", "/query /FO CSV /NH")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var p = System.Diagnostics.Process.Start(psi);
        if (p == null) return;
        string output = p.StandardOutput.ReadToEnd();
        p.WaitForExit(15000);
        foreach (var raw in output.Split('\n'))
        {
            var parts = SplitCsv(raw.Trim());
            if (parts.Count >= 3)
                a.ScheduledTasks.Add(new ScheduledTaskInfo { TaskName = parts[0], NextRun = parts[1], Status = parts[2] });
            if (a.ScheduledTasks.Count >= 300) break;
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

    private static void CollectDrivers(FullSystemAudit a)
    {
        foreach (var d in Wql("SELECT DeviceName, Manufacturer, DriverVersion, DriverDate, IsSigned FROM Win32_PnPSignedDriver"))
        {
            var name = S(d, "DeviceName");
            if (string.IsNullOrEmpty(name)) continue;
            a.Drivers.Add(new DriverInfo
            {
                DeviceName = name, Manufacturer = S(d, "Manufacturer"), Version = S(d, "DriverVersion"),
                DriverDate = S(d, "DriverDate"), IsSigned = S(d, "IsSigned") != "False"
            });
        }
    }

    private static void CollectUpdates(FullSystemAudit a)
    {
        foreach (var u in Wql("SELECT HotFixID, Description, InstalledOn FROM Win32_QuickFixEngineering"))
            a.InstalledUpdates.Add(new WindowsUpdateInfo
            {
                HotFixId = S(u, "HotFixID"), Description = S(u, "Description"), InstalledOn = S(u, "InstalledOn")
            });
    }

    private static void CollectAccounts(FullSystemAudit a)
    {
        var admins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var gu in Wql("SELECT GroupComponent, PartComponent FROM Win32_GroupUser"))
        {
            var group = gu["GroupComponent"]?.ToString() ?? "";
            var part = gu["PartComponent"]?.ToString() ?? "";
            if (group.IndexOf("Administrators", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                var m = Regex.Match(part, "Name=\"([^\"]+)\"");
                if (m.Success) admins.Add(m.Groups[1].Value);
            }
        }

        foreach (var u in Wql("SELECT Name, FullName, Disabled, PasswordRequired, SID FROM Win32_UserAccount WHERE LocalAccount=True"))
            a.UserAccounts.Add(new UserAccountInfo
            {
                Name = S(u, "Name"), FullName = S(u, "FullName"),
                Disabled = S(u, "Disabled") == "True", PasswordRequired = S(u, "PasswordRequired") == "True",
                Sid = S(u, "SID"), IsAdministrator = admins.Contains(S(u, "Name"))
            });
    }

    private static void CollectShares(FullSystemAudit a)
    {
        foreach (var sh in Wql("SELECT Name, Path, Type, Description FROM Win32_Share"))
            a.SharedFolders.Add(new SharedFolderInfo
            {
                Name = S(sh, "Name"), Path = S(sh, "Path"),
                Type = ShareTypeName(S(sh, "Type")), Description = S(sh, "Description")
            });
    }

    private static string ShareTypeName(string t) => t switch
    {
        "0" => "Диск", "1" => "Принтер", "2" => "Устройство", "3" => "IPC",
        "2147483648" => "Служебная", _ => "Другая"
    };

    private static void BuildSummary(FullSystemAudit a)
    {
        a.Summary = new AuditSummary
        {
            DiskCount = a.Disks.Count,
            VolumeCount = a.Volumes.Count,
            GpuCount = a.Gpus.Count,
            StartupItemCount = a.StartupItems.Count,
            ScheduledTaskCount = a.ScheduledTasks.Count,
            DriverCount = a.Drivers.Count,
            UnsignedDriverCount = a.Drivers.Count(d => !d.IsSigned),
            UpdateCount = a.InstalledUpdates.Count,
            UserCount = a.UserAccounts.Count,
            AdminCount = a.UserAccounts.Count(u => u.IsAdministrator && !u.Disabled),
            EnabledUserCount = a.UserAccounts.Count(u => !u.Disabled),
            ShareCount = a.SharedFolders.Count,
            SmartPredictFailure = a.SmartStatus.Any(s => s.PredictFailure)
        };
    }

    // ==================== HTML-ОТЧЁТ ====================
    public string BuildHtmlReport(FullSystemAudit a)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html lang='ru'><head><meta charset='utf-8'><title>Полный отчёт о системе</title><style>");
        sb.AppendLine("body{font-family:'Segoe UI',Arial,sans-serif;margin:24px;color:#222}h1{font-size:22px}h2{font-size:17px;margin-top:26px;border-bottom:2px solid #4a6da7;padding-bottom:4px;color:#2c4a7c}table{border-collapse:collapse;width:100%;margin-top:8px;font-size:13px}th,td{border:1px solid #ccd;padding:5px 8px;text-align:left;vertical-align:top}th{background:#eef2f8}.warn{color:#b00;font-weight:600}.ok{color:#070}p.meta{color:#666}");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h1>Полный отчёт о системе: {Esc(a.Computer.Name)}</h1>");
        sb.AppendLine($"<p class='meta'>Собран: {a.CollectedAt:dd.MM.yyyy HH:mm:ss}</p>");

        Table(sb, "Компьютер и ОС", new[] { "Параметр", "Значение" }, new[]
        {
            new[] { "Имя компьютера", a.Computer.Name },
            new[] { "ОС", $"{a.Computer.Os} ({a.Computer.OsArchitecture})" },
            new[] { "Процессор", $"{a.Computer.Cpu} ({a.Computer.CpuCores} ядра / {a.Computer.CpuLogical} потоков)" },
            new[] { "Память", $"{a.Computer.TotalRamGb} ГБ всего, {a.Computer.FreeRamGb} ГБ свободно" },
            new[] { "Производитель / модель", $"{a.Computer.Manufacturer} / {a.Computer.Model}" },
            new[] { "Тип системы", a.Computer.SystemType },
            new[] { "Последняя загрузка", a.Computer.LastBoot == DateTime.MinValue ? "н/д" : a.Computer.LastBoot.ToString("dd.MM.yyyy HH:mm:ss") }
        });

        Table(sb, "BIOS и материнская плата", new[] { "Параметр", "Значение" }, new[]
        {
            new[] { "BIOS", $"{a.Bios.Manufacturer} {a.Bios.Version}" },
            new[] { "Дата BIOS", a.Bios.ReleaseDate },
            new[] { "Серийный номер ПК", a.Bios.Serial },
            new[] { "Материнская плата", $"{a.Bios.BoardManufacturer} {a.Bios.BoardProduct}" },
            new[] { "Серийный номер платы", a.Bios.BoardSerial }
        });

        Table(sb, "Физические диски", new[] { "Модель", "Объём, ГБ", "Носитель", "Интерфейс", "Статус" },
            a.Disks.Select(d => new[] { d.Model, d.SizeGb.ToString("0.#"), d.MediaType, d.Interface, d.Status }));

        Table(sb, "Логические тома", new[] { "Диск", "Метка", "ФС", "Объём, ГБ", "Свободно, ГБ" },
            a.Volumes.Select(v => new[] { v.DeviceId, v.VolumeName, v.FileSystem, v.SizeGb.ToString("0.#"), v.FreeGb.ToString("0.#") }));

        if (a.SmartStatus.Count > 0)
        {
            var bad = a.SmartStatus.Any(s => s.PredictFailure);
            sb.AppendLine($"<h2>SMART</h2><p class='{(bad ? "warn" : "ok")}'>{(bad ? "ЕСТЬ ПРЕДСКАЗАНИЕ ОТКАЗА ДИСКА!" : "Прогноз отказа дисков не выявлен.")}</p>");
        }

        Table(sb, "Видеокарты", new[] { "Модель", "VRAM, ГБ", "Драйвер", "Разрешение", "Статус" },
            a.Gpus.Select(g => new[] { g.Name, g.VramGb.ToString("0.#"), g.DriverVersion, g.Resolution, g.Status }));

        Table(sb, "Сетевые адаптеры", new[] { "Адаптер", "MAC", "IP", "DHCP", "DHCP-сервер" },
            a.NetworkAdapters.Select(n => new[] { n.Description, n.Mac, n.IpAddresses, n.DhcpEnabled ? "вкл" : "выкл", n.DhcpServer }));

        Table(sb, $"Автозагрузка ({a.StartupItems.Count})", new[] { "Имя", "Команда", "Источник" },
            a.StartupItems.Select(s => new[] { s.Name, s.Command, s.Source }));

        Table(sb, $"Запланированные задачи ({a.ScheduledTasks.Count}, показаны первые 100)", new[] { "Задача", "След. запуск", "Статус" },
            a.ScheduledTasks.Take(100).Select(t => new[] { t.TaskName, t.NextRun, t.Status }));

        Table(sb, $"Драйверы ({a.Drivers.Count}, показаны первые 100; неподписанных: {a.Summary.UnsignedDriverCount})", new[] { "Устройство", "Производитель", "Версия", "Подпись" },
            a.Drivers.Take(100).Select(d => new[] { d.DeviceName, d.Manufacturer, d.Version, d.IsSigned ? "есть" : "НЕТ" }));

        Table(sb, $"Обновления Windows ({a.InstalledUpdates.Count})", new[] { "KB", "Описание", "Дата" },
            a.InstalledUpdates.OrderByDescending(u => u.InstalledOn).Select(u => new[] { u.HotFixId, u.Description, u.InstalledOn }));

        Table(sb, "Учётные записи", new[] { "Имя", "Полное имя", "Админ", "Пароль обязателен", "Отключена" },
            a.UserAccounts.Select(u => new[] { u.Name, u.FullName, u.IsAdministrator ? "ДА" : "-", u.PasswordRequired ? "да" : "нет", u.Disabled ? "да" : "нет" }));

        Table(sb, "Общие папки", new[] { "Имя", "Путь", "Тип", "Описание" },
            a.SharedFolders.Select(s => new[] { s.Name, s.Path, s.Type, s.Description }));

        Table(sb, "Сводка", new[] { "Показатель", "Значение" }, new[]
        {
            new[] { "Дисков / томов", $"{a.Summary.DiskCount} / {a.Summary.VolumeCount}" },
            new[] { "Видеокарт", a.Summary.GpuCount.ToString() },
            new[] { "Автозагрузка", a.Summary.StartupItemCount.ToString() },
            new[] { "Задач в планировщике", a.Summary.ScheduledTaskCount.ToString() },
            new[] { "Драйверов (неподписанных)", $"{a.Summary.DriverCount} ({a.Summary.UnsignedDriverCount})" },
            new[] { "Обновлений KB", a.Summary.UpdateCount.ToString() },
            new[] { "Пользователей (активных / админов)", $"{a.Summary.EnabledUserCount} / {a.Summary.AdminCount}" },
            new[] { "Общих папок", a.Summary.ShareCount.ToString() },
            new[] { "SMART-риск", a.Summary.SmartPredictFailure ? "ДА — требуется замена диска" : "не выявлен" }
        });

        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    private static void Table(StringBuilder sb, string title, string[] headers, IEnumerable<string[]> rows)
    {
        sb.AppendLine($"<h2>{Esc(title)}</h2><table><tr>");
        foreach (var h in headers) sb.Append($"<th>{Esc(h)}</th>");
        sb.AppendLine("</tr>");
        foreach (var r in rows)
        {
            sb.Append("<tr>");
            foreach (var c in r) sb.Append($"<td>{Esc(c)}</td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</table>");
    }

    private static string Esc(string s) => System.Net.WebUtility.HtmlEncode(s ?? "");
}