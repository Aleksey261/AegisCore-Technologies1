using System.Text.RegularExpressions;
using System.Xml.Linq;
using AegisCore.Models;

namespace AegisCore.Services;

public class FstecImportService
{
    private readonly JsonDataStore _store;
    public FstecImportService(JsonDataStore store) => _store = store;

    public async Task<Dictionary<string, object>> ImportVulnerabilitiesAsync(IFormFile file)
    {
        var imported = new List<Vulnerability>();
        var name = file.FileName.ToLowerInvariant();
        await using var stream = file.OpenReadStream();
        if (name.EndsWith(".xml")) imported = ImportXml(stream);
        else if (name.EndsWith(".csv")) imported = ImportCsv(stream);
        else throw new InvalidOperationException("Поддерживаются XML и CSV файлы.");

        var all = _store.Vulnerabilities;
        var added = 0;
        var updated = 0;
        foreach (var item in imported)
        {
            var existing = all.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v.BduId) && v.BduId.Equals(item.BduId, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                all.Add(item);
                added++;
            }
            else
            {
                existing.CveId = item.CveId;
                existing.Title = item.Title;
                existing.Description = item.Description;
                existing.Product = item.Product;
                existing.Vendor = item.Vendor;
                existing.Version = item.Version;
                existing.Severity = item.Severity;
                existing.Cvss = item.Cvss;
                updated++;
            }
        }
        _store.SaveVulnerabilities(all);
        return new Dictionary<string, object> { ["total"] = imported.Count, ["added"] = added, ["updated"] = updated };
    }

    private static List<Vulnerability> ImportXml(Stream stream)
    {
        var list = new List<Vulnerability>();
        var doc = XDocument.Load(stream, LoadOptions.None);
        var textBlocks = doc.Descendants().Where(e => e.HasElements == false).Select(e => e.Value).Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
        var cveRegex = new Regex(@"CVE-\d{4}-\d{4,7}", RegexOptions.IgnoreCase);
        var bduCounter = 1;
        foreach (var value in textBlocks)
        {
            var cve = cveRegex.Match(value).Value.ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(cve)) continue;
            list.Add(new Vulnerability
            {
                BduId = $"XML-{bduCounter++:000000}",
                CveId = cve,
                Title = value.Length > 180 ? value[..180] : value,
                Description = value,
                Product = ExtractProduct(value),
                Severity = GuessSeverity(value),
                Source = "FSTEC XML"
            });
        }
        return list.GroupBy(v => v.CveId).Select(g => g.First()).ToList();
    }

    private static List<Vulnerability> ImportCsv(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var lines = new List<string>();
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (!string.IsNullOrWhiteSpace(line)) lines.Add(line);
        }
        var list = new List<Vulnerability>();
        foreach (var line in lines.Skip(1))
        {
            var parts = SplitCsv(line);
            if (parts.Length == 0) continue;
            var joined = string.Join(" ", parts);
            list.Add(new Vulnerability
            {
                BduId = parts.ElementAtOrDefault(0) ?? $"CSV-{Guid.NewGuid():N}",
                CveId = Regex.Match(joined, @"CVE-\d{4}-\d{4,7}", RegexOptions.IgnoreCase).Value.ToUpperInvariant(),
                Title = parts.ElementAtOrDefault(1) ?? joined[..Math.Min(joined.Length, 120)],
                Description = joined,
                Product = ExtractProduct(joined),
                Severity = GuessSeverity(joined),
                Source = "FSTEC CSV"
            });
        }
        return list;
    }

    private static string[] SplitCsv(string line) => line.Split(';').Length > line.Split(',').Length ? line.Split(';') : line.Split(',');
    private static string ExtractProduct(string text)
    {
        var lower = text.ToLowerInvariant();
        if (lower.Contains("office")) return "Microsoft Office";
        if (lower.Contains("outlook")) return "Microsoft Outlook";
        if (lower.Contains("excel")) return "Microsoft Excel";
        if (lower.Contains("word")) return "Microsoft Word";
        if (lower.Contains("adobe")) return "Adobe Reader";
        if (lower.Contains("windows")) return "Microsoft Windows";
        if (lower.Contains("openssh")) return "OpenSSH";
        return "Unknown product";
    }
    private static string GuessSeverity(string text)
    {
        var lower = text.ToLowerInvariant();
        if (lower.Contains("critical") || lower.Contains("критич")) return "Критический";
        if (lower.Contains("high") || lower.Contains("высок")) return "Высокий";
        if (lower.Contains("low") || lower.Contains("низк")) return "Низкий";
        return "Средний";
    }
}
