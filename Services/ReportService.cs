using System.Text;
using AegisCore.Models;

namespace AegisCore.Services;

public class ReportService
{
    private readonly JsonDataStore _store;
    public ReportService(JsonDataStore store) => _store = store;

    public string BuildHtmlReport(string? scanRunId = null)
    {
        var job = string.IsNullOrWhiteSpace(scanRunId)
            ? _store.ScanJobs.OrderByDescending(j => j.StartedAt ?? j.CreatedAt).FirstOrDefault()
            : _store.ScanJobs.FirstOrDefault(j => j.JobId == scanRunId);
        scanRunId ??= job?.JobId ?? "MANUAL";
        var findings = _store.AssessmentFindings.Where(f => f.ScanRunId == scanRunId).ToList();
        var byRisk = findings.GroupBy(f => f.Risk).ToDictionary(g => g.Key, g => g.Count());
        var byProduct = findings.GroupBy(f => string.IsNullOrWhiteSpace(f.Product) ? "Не определено" : f.Product)
            .Select(g => new { Product = g.Key, Count = g.Count(), Critical = g.Count(x => x.Risk == "Критический"), High = g.Count(x => x.Risk == "Высокий"), Medium = g.Count(x => x.Risk == "Средний"), Low = g.Count(x => x.Risk == "Низкий") })
            .OrderByDescending(x => x.Count).ToList();

        var sb = new StringBuilder();
        sb.AppendLine("<!doctype html><html lang='ru'><head><meta charset='utf-8'><title>Отчёт AegisCore</title>");
        sb.AppendLine("<style>body{font-family:Arial;margin:32px;color:#101828}h1{font-size:28px}.meta{display:grid;grid-template-columns:260px 1fr;gap:8px}.box{border:1px solid #d0d5dd;border-radius:8px;padding:16px;margin:16px 0}table{width:100%;border-collapse:collapse;margin-top:12px}th,td{border:1px solid #d0d5dd;padding:8px;font-size:13px}th{background:#eef4ff}.risk{font-weight:bold}.critical{color:#b42318}.high{color:#b54708}.medium{color:#175cd3}.low{color:#027a48}</style></head><body>");
        sb.AppendLine("<h1>ОТЧЁТ</h1>");
        sb.AppendLine("<div class='meta'>");
        sb.AppendLine($"<b>№ отчёта</b><span>{Guid.NewGuid()}</span>");
        sb.AppendLine("<b>Профиль</b><span>Уязвимости / угрозы / АСУ ТП</span>");
        sb.AppendLine($"<b>Задание</b><span>{scanRunId}</span>");
        sb.AppendLine($"<b>Начало/завершение сканирования</b><span>{job?.StartedAt?.ToLocalTime():g} / {job?.CompletedAt?.ToLocalTime():g}</span>");
        sb.AppendLine($"<b>Формирование отчёта</b><span>{DateTime.Now:g}</span>");
        sb.AppendLine($"<b>Хосты</b><span>[1] {Environment.MachineName}</span>");
        sb.AppendLine("</div>");
        sb.AppendLine("<div class='box'><h2>Распределение по уровням риска</h2><table><tr><th>Риск</th><th>Количество</th></tr>");
        foreach (var risk in new[] { "Критический", "Высокий", "Средний", "Низкий" }) sb.AppendLine($"<tr><td>{risk}</td><td>{byRisk.GetValueOrDefault(risk)}</td></tr>");
        sb.AppendLine($"<tr><th>Всего</th><th>{findings.Count}</th></tr></table></div>");
        sb.AppendLine("<div class='box'><h2>Распределение по хостам</h2><table><tr><th>Хост / Риск</th><th>Критический</th><th>Высокий</th><th>Средний</th><th>Низкий</th><th>Всего</th></tr>");
        sb.AppendLine($"<tr><td>{Environment.MachineName}</td><td>{byRisk.GetValueOrDefault("Критический")}</td><td>{byRisk.GetValueOrDefault("Высокий")}</td><td>{byRisk.GetValueOrDefault("Средний")}</td><td>{byRisk.GetValueOrDefault("Низкий")}</td><td>{findings.Count}</td></tr></table></div>");
        sb.AppendLine("<div class='box'><h2>Распределение по продуктам</h2><table><tr><th>Продукт / Риск</th><th>Критический</th><th>Высокий</th><th>Средний</th><th>Низкий</th><th>Всего</th></tr>");
        foreach (var row in byProduct) sb.AppendLine($"<tr><td>{Esc(row.Product)}</td><td>{row.Critical}</td><td>{row.High}</td><td>{row.Medium}</td><td>{row.Low}</td><td>{row.Count}</td></tr>");
        sb.AppendLine("</table></div>");
        sb.AppendLine("<div class='box'><h2>Уязвимости и признаки угроз</h2><table><tr><th>Хост</th><th>Риск</th><th>Категория</th><th>Название</th><th>Доказательство</th><th>Рекомендация</th></tr>");
        foreach (var f in findings.OrderByDescending(f => RiskOrder(f.Risk)))
        {
            sb.AppendLine($"<tr><td>{Esc(f.Host)}</td><td class='risk'>{Esc(f.Risk)}</td><td>{Esc(f.Category)}</td><td>{Esc(f.Title)}</td><td>{Esc(f.Evidence)}</td><td>{Esc(f.Recommendation)}</td></tr>");
        }
        sb.AppendLine("</table></div></body></html>");
        return sb.ToString();
    }

    public byte[] BuildPdfReport(string? scanRunId = null)
    {
        var job = string.IsNullOrWhiteSpace(scanRunId) ? _store.ScanJobs.OrderByDescending(j => j.StartedAt ?? j.CreatedAt).FirstOrDefault() : _store.ScanJobs.FirstOrDefault(j => j.JobId == scanRunId);
        scanRunId ??= job?.JobId ?? "MANUAL";
        var findings = _store.AssessmentFindings.Where(f => f.ScanRunId == scanRunId).ToList();
        var lines = new List<string>
        {
            "OTCHET AegisCore",
            $"Report ID: {Guid.NewGuid()}",
            "Profile: Vulnerabilities / Threats / ICS",
            $"Job: {scanRunId}",
            $"Generated: {DateTime.Now:g}",
            $"Host: {Environment.MachineName}",
            "",
            "Risk distribution:",
            $"Critical: {findings.Count(f => f.Risk == "Критический")}",
            $"High: {findings.Count(f => f.Risk == "Высокий")}",
            $"Medium: {findings.Count(f => f.Risk == "Средний")}",
            $"Low: {findings.Count(f => f.Risk == "Низкий")}",
            $"Total: {findings.Count}",
            "",
            "Findings:"
        };
        lines.AddRange(findings.Take(200).Select(f => $"[{f.Risk}] {f.Category}: {Ascii(f.Title)} | {Ascii(f.Product)} | {Ascii(f.Evidence)}"));
        return SimplePdfWriter.Create(lines);
    }

    private static string Esc(string? s) => System.Net.WebUtility.HtmlEncode(s ?? string.Empty);
    private static int RiskOrder(string risk) => risk switch { "Критический" => 4, "Высокий" => 3, "Средний" => 2, "Низкий" => 1, _ => 0 };
    private static string Ascii(string? text) => string.Concat((text ?? string.Empty).Select(ch => ch < 128 ? ch : '?'));
}

public static class SimplePdfWriter
{
    public static byte[] Create(IReadOnlyList<string> lines)
    {
        var pages = lines.Chunk(42).ToList();
        var objects = new List<string>();
        objects.Add("<< /Type /Catalog /Pages 2 0 R >>");
        var kids = string.Join(" ", Enumerable.Range(0, pages.Count).Select(i => $"{3 + i * 2} 0 R"));
        objects.Add($"<< /Type /Pages /Kids [{kids}] /Count {pages.Count} >>");
        for (var i = 0; i < pages.Count; i++)
        {
            var pageObj = 3 + i * 2;
            var contentObj = pageObj + 1;
            objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> >> >> /Contents {contentObj} 0 R >>");
            var content = BuildContent(pages[i]);
            objects.Add($"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}\nendstream");
        }
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, Encoding.ASCII, leaveOpen: true);
        writer.WriteLine("%PDF-1.4");
        var offsets = new List<long> { 0 };
        for (var i = 0; i < objects.Count; i++)
        {
            writer.Flush(); offsets.Add(ms.Position);
            writer.WriteLine($"{i + 1} 0 obj"); writer.WriteLine(objects[i]); writer.WriteLine("endobj");
        }
        writer.Flush(); var xref = ms.Position;
        writer.WriteLine("xref"); writer.WriteLine($"0 {objects.Count + 1}"); writer.WriteLine("0000000000 65535 f ");
        foreach (var off in offsets.Skip(1)) writer.WriteLine($"{off:0000000000} 00000 n ");
        writer.WriteLine("trailer"); writer.WriteLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>"); writer.WriteLine("startxref"); writer.WriteLine(xref); writer.WriteLine("%%EOF"); writer.Flush();
        return ms.ToArray();
    }
    private static string BuildContent(IEnumerable<string> lines)
    {
        var sb = new StringBuilder(); sb.AppendLine("BT /F1 10 Tf 40 800 Td");
        foreach (var raw in lines)
        {
            var line = raw.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
            if (line.Length > 105) line = line[..105];
            sb.AppendLine($"({line}) Tj 0 -18 Td");
        }
        sb.AppendLine("ET"); return sb.ToString();
    }
}
