using System.Text.Json;
using AegisCore.Models;

namespace AegisCore.Services;

public class JsonDataStore
{
    private readonly string _dataDir;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly object _lock = new();

    public JsonDataStore(IWebHostEnvironment env)
    {
        _dataDir = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(_dataDir);
        Seed();
    }

    // ============================================================
    // СВОЙСТВА (чтение данных)
    // ============================================================

    public List<ScanTarget> ScanTargets => Load<ScanTarget>("scan_targets.json");
    public List<ScanJob> ScanJobs => Load<ScanJob>("scan_jobs.json");
    public List<ScanResult> ScanResults => Load<ScanResult>("scan_results.json");
    public List<Vulnerability> Vulnerabilities => Load<Vulnerability>("vulnerabilities.json");
    public List<FstecThreat> Threats => Load<FstecThreat>("threats.json");
    public List<IndustrialSecurityItem> IndustrialItems => Load<IndustrialSecurityItem>("industrial_security.json");
    public List<ExportControlItem> ExportControlItems => Load<ExportControlItem>("export_control.json");
    public List<PcScanItem> PcScanItems => Load<PcScanItem>("pc_scan_items.json");
    public List<AssessmentFinding> AssessmentFindings => Load<AssessmentFinding>("assessment_findings.json");

    // ============================================================
    // ПОСТАВЩИКИ СЗИ
    // ============================================================

    public List<SziVendor> GetSziVendors() => Load<SziVendor>("szi_vendors.json");
    public void SaveSziVendors(List<SziVendor> items) => Save("szi_vendors.json", items);

    // ============================================================
    // МЕТОДЫ СОХРАНЕНИЯ
    // ============================================================

    public void SaveScanTargets(List<ScanTarget> items) => Save("scan_targets.json", items);
    public void SaveScanJobs(List<ScanJob> items) => Save("scan_jobs.json", items);
    public void SaveScanResults(List<ScanResult> items) => Save("scan_results.json", items);
    public void SaveVulnerabilities(List<Vulnerability> items) => Save("vulnerabilities.json", items);
    public void SaveThreats(List<FstecThreat> items) => Save("threats.json", items);
    public void SaveIndustrialItems(List<IndustrialSecurityItem> items) => Save("industrial_security.json", items);
    public void SaveExportControlItems(List<ExportControlItem> items) => Save("export_control.json", items);
    public void SavePcScanItems(List<PcScanItem> items) => Save("pc_scan_items.json", items);
    public void SaveAssessmentFindings(List<AssessmentFinding> items) => Save("assessment_findings.json", items);

    // ============================================================
    // СТАТИСТИКА
    // ============================================================

    public DashboardStats GetStats() => new(
        ScanTargets.Count,
        ScanJobs.Count,
        Vulnerabilities.Count,
        AssessmentFindings.Count);

    // ============================================================
    // ПРИВАТНЫЕ МЕТОДЫ
    // ============================================================

    private List<T> Load<T>(string file)
    {
        lock (_lock)
        {
            var path = Path.Combine(_dataDir, file);
            if (!File.Exists(path)) return new List<T>();
            var json = File.ReadAllText(path);
            return string.IsNullOrWhiteSpace(json) ? new List<T>() : JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
        }
    }

    private void Save<T>(string file, List<T> items)
    {
        lock (_lock)
        {
            var path = Path.Combine(_dataDir, file);
            File.WriteAllText(path, JsonSerializer.Serialize(items, _jsonOptions));
        }
    }

    // ============================================================
    // ИНИЦИАЛИЗАЦИЯ ДАННЫХ
    // ============================================================

    private void Seed()
    {
        // Цели сканирования
        if (!File.Exists(Path.Combine(_dataDir, "scan_targets.json")))
        {
            SaveScanTargets(new List<ScanTarget>
            {
                new() { Name = "Localhost", TargetValue = "127.0.0.1", Type = "HOST", ScanMode = "C1" }
            });
        }

        // Уязвимости (БДУ ФСТЭК)
        if (!File.Exists(Path.Combine(_dataDir, "vulnerabilities.json")))
        {
            SaveVulnerabilities(new List<Vulnerability>
            {
                new() { BduId = "BDU:2023-00001", CveId = "CVE-2023-23397", Title = "Уязвимость Microsoft Outlook", Product = "Microsoft Outlook", Vendor = "Microsoft", Severity = "Критический", Cvss = 9.8, Description = "Пример записи для сопоставления установленного ПО." },
                new() { BduId = "BDU:2017-00001", CveId = "CVE-2017-11882", Title = "Уязвимость Microsoft Office Equation Editor", Product = "Microsoft Office", Vendor = "Microsoft", Severity = "Высокий", Cvss = 7.8, Description = "Пример записи для проверки Office." },
                new() { BduId = "BDU:2024-00001", CveId = "CVE-2024-0000", Title = "Уязвимость устаревшего Adobe Reader", Product = "Adobe Reader", Vendor = "Adobe", Severity = "Высокий", Cvss = 8.1, Description = "Пример записи для проверки Adobe Reader." }
            });
        }

        // Угрозы ФСТЭК
        if (!File.Exists(Path.Combine(_dataDir, "threats.json")))
        {
            SaveThreats(new List<FstecThreat>
            {
                new() { Code = "УБИ.001", Name = "Несанкционированный доступ", Severity = "Высокий", SourceModule = "БДУ", Description = "Попытки доступа к системе без разрешения." },
                new() { Code = "УБИ.002", Name = "Эксплуатация сетевых служб", Severity = "Средний", SourceModule = "БДУ", Description = "Использование открытых сетевых служб для атаки." }
            });
        }

        // Промышленная безопасность
        if (!File.Exists(Path.Combine(_dataDir, "industrial_security.json")))
        {
            SaveIndustrialItems(new List<IndustrialSecurityItem>
            {
                new() { AssetType = "SCADA/HMI", Name = "Незащищённый интерфейс управления", RiskLevel = "Высокий", Description = "Признаки компонентов АСУ ТП или промышленного ПО.", RecommendedControl = "Сегментация сети, контроль доступа, журналирование." },
                new() { AssetType = "PLC/OPC", Name = "Промышленный протокол в сети", RiskLevel = "Высокий", Description = "Обнаружены признаки Modbus/OPC/Siemens S7.", RecommendedControl = "Ограничить доступ и включить мониторинг." }
            });
        }

        // Экспортный контроль
        if (!File.Exists(Path.Combine(_dataDir, "export_control.json")))
        {
            SaveExportControlItems(new List<ExportControlItem>());
        }

        // Пустые списки
        if (!File.Exists(Path.Combine(_dataDir, "pc_scan_items.json")))
            SavePcScanItems(new List<PcScanItem>());

        if (!File.Exists(Path.Combine(_dataDir, "assessment_findings.json")))
            SaveAssessmentFindings(new List<AssessmentFinding>());

        if (!File.Exists(Path.Combine(_dataDir, "scan_jobs.json")))
            SaveScanJobs(new List<ScanJob>());

        if (!File.Exists(Path.Combine(_dataDir, "scan_results.json")))
            SaveScanResults(new List<ScanResult>());

        // ✅ НОВОЕ: Справочник поставщиков СЗИ
        if (!File.Exists(Path.Combine(_dataDir, "szi_vendors.json")))
        {
            SaveSziVendors(new List<SziVendor>
            {
                new()
                {
                    ShortName = "ООО «Поставщик СЗИ»",
                    FullName = "Общество с ограниченной ответственностью «Поставщик средств защиты информации»",
                    Address = "г. Москва, ул. Примерная, д. 1",
                    Inn = "7701234567",
                    Phone = "+7 (495) 123-45-67",
                    Email = "info@vendor.ru"
                },
                new()
                {
                    ShortName = "АО «ИнфоТех»",
                    FullName = "Акционерное общество «Информационные технологии»",
                    Address = "г. Санкт-Петербург, Невский пр., д. 100",
                    Inn = "7801234567",
                    Phone = "+7 (812) 987-65-43",
                    Email = "sales@infotech.ru"
                },
                new()
                {
                    ShortName = "ПАО «КриптоПро»",
                    FullName = "Публичное акционерное общество «КриптоПро»",
                    Address = "г. Москва, ул. Сущёвский Вал, д. 18",
                    Inn = "7717081700",
                    Phone = "+7 (495) 744-11-55",
                    Email = "info@cryptopro.ru"
                }
            });
        }
    }
}