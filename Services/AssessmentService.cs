using AegisCore.Models;

namespace AegisCore.Services;

public class AssessmentService
{
    private readonly JsonDataStore _store;

    public AssessmentService(JsonDataStore store) => _store = store;

    public List<AssessmentFinding> AssessLatestScan()
    {
        var job = _store.ScanJobs.OrderByDescending(j => j.StartedAt ?? j.CreatedAt).FirstOrDefault();
        var scanRunId = job?.JobId ?? "MANUAL";
        var findings = new List<AssessmentFinding>();
        var items = _store.PcScanItems.Where(i => i.ScanRunId == scanRunId).ToList();
        var vulns = _store.Vulnerabilities;
        var scanResults = _store.ScanResults.Where(r => r.ScanJobId == scanRunId).ToList();

        foreach (var sw in items.Where(i => i.Category == "Software"))
        {
            var matches = vulns.Where(v => MatchProduct(sw.ItemKey, v.Product) || MatchProduct(sw.ItemKey, v.Title)).Take(25);
            foreach (var v in matches)
            {
                findings.Add(new AssessmentFinding
                {
                    ScanRunId = scanRunId,
                    Risk = NormalizeRisk(v.Severity),
                    Category = "Уязвимость",
                    Product = sw.ItemKey,
                    Title = v.Title,
                    // ✅ ИСПРАВЛЕНО: добавлены ?? "" для nullable-полей
                    Evidence = $"ПО: {sw.ItemKey}; версия: {sw.ItemValue ?? ""}; запись: {v.BduId ?? ""} {v.CveId ?? ""}",
                    Recommendation = "Проверить применимость, обновить ПО, установить патчи производителя или удалить устаревший компонент.",
                    BduId = v.BduId,
                    CveId = v.CveId
                });
            }
        }

        foreach (var port in scanResults.Where(r => r.Port is not null))
        {
            if (port.Port is 23 or 445 or 3389 or 5985)
            {
                findings.Add(new AssessmentFinding
                {
                    ScanRunId = scanRunId,
                    // ✅ ИСПРАВЛЕНО: добавлен ?? "Средний" для nullable FstecSeverity
                    Risk = port.FstecSeverity ?? "Средний",
                    Category = "Угроза",
                    Product = port.Service ?? "",
                    Title = $"Открытый сетевой сервис: {port.Service}",
                    // ✅ ИСПРАВЛЕНО: добавлены ?? "" для nullable полей
                    Evidence = $"{port.Host ?? ""}:{port.Port}/{port.Protocol ?? ""}",
                    Recommendation = "Ограничить доступ, включить аудит, оставить открытым только при подтверждённой необходимости."
                });
            }
            if (port.Port is 502 or 102 or 4840 or 44818)
            {
                findings.Add(new AssessmentFinding
                {
                    ScanRunId = scanRunId,
                    Risk = "Высокий",
                    Category = "АСУ ТП",
                    Product = port.Service ?? "",
                    Title = $"Признак промышленного протокола: {port.Service}",
                    // ✅ ИСПРАВЛЕНО: добавлены ?? "" для nullable полей
                    Evidence = $"{port.Host ?? ""}:{port.Port}/{port.Protocol ?? ""}",
                    Recommendation = "Изолировать промышленный сегмент, проверить ACL, включить мониторинг и журналирование."
                });
            }
        }

        foreach (var proc in items.Where(i => i.Category == "Process" || i.Category == "Software"))
        {
            var text = (proc.ItemKey + " " + (proc.ItemValue ?? "") + " " + (proc.Details ?? "")).ToLowerInvariant();
            if (new[] { "scada", "wincc", "codesys", "kepware", "opc", "modbus", "siemens", "plc", "hmi" }.Any(text.Contains))
            {
                findings.Add(new AssessmentFinding
                {
                    ScanRunId = scanRunId,
                    Risk = "Высокий",
                    Category = "АСУ ТП",
                    Product = proc.ItemKey,
                    Title = "Обнаружен признак ПО АСУ ТП",
                    Evidence = $"{proc.Category}: {proc.ItemKey} {proc.ItemValue ?? ""}",
                    Recommendation = "Проверить принадлежность к промышленному контуру и применить меры защиты АСУ ТП."
                });
            }
        }

        var all = _store.AssessmentFindings.Where(f => f.ScanRunId != scanRunId).ToList();
        all.AddRange(findings);
        _store.SaveAssessmentFindings(all);
        return findings;
    }

    private static bool MatchProduct(string inventoryName, string product)
    {
        if (string.IsNullOrWhiteSpace(product) || product == "Unknown product") return false;
        var inv = inventoryName.ToLowerInvariant();
        var prod = product.ToLowerInvariant();
        return inv.Contains(prod) || prod.Split(' ', StringSplitOptions.RemoveEmptyEntries).Any(t => t.Length > 3 && inv.Contains(t));
    }

    private static string NormalizeRisk(string risk) => risk switch
    {
        "Критический" or "Critical" => "Критический",
        "Высокий" or "High" => "Высокий",
        "Низкий" or "Low" => "Низкий",
        _ => "Средний"
    };
}