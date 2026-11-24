using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace AegisCore.Controllers.Reports;

[Route("Reports")]
public class ReportsController : Controller
{
    // GET /Reports и /Reports?section=...
    [HttpGet]
    public IActionResult Index(string section = "main")
    {
        ViewBag.ActiveModule = "reports";
        ViewBag.ActiveSection = section;
        return View();
    }

    // GET /Reports/list
    [HttpGet("list")]
    public IActionResult GetList()
    {
        var reports = new List<ReportFileInfo>();
        Collect(reports, "Reports", "Базовый аудит");
        Collect(reports, "WindowsReports", "Расширенный аудит");
        return Ok(reports.OrderByDescending(r => r.CreatedAt).ToList());
    }

    // GET /Reports/download?folder=...&file=...
    [HttpGet("download")]
    public IActionResult Download([FromQuery] string folder, [FromQuery] string file)
    {
        if (folder != "Reports" && folder != "WindowsReports")
            return BadRequest(new { error = "Недопустимая папка" });

        var safeName = Path.GetFileName(file);
        var path = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", folder, safeName);
        if (!System.IO.File.Exists(path))
            return NotFound(new { error = "Файл не найден" });

        var contentType = path.EndsWith(".html", System.StringComparison.OrdinalIgnoreCase)
            ? "text/html; charset=utf-8"
            : "application/json";
        return PhysicalFile(path, contentType, safeName);
    }

    private static void Collect(List<ReportFileInfo> list, string folder, string typeLabel)
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", folder);
        if (!Directory.Exists(dir)) return;

        foreach (var f in Directory.GetFiles(dir))
        {
            var ext = Path.GetExtension(f).ToLowerInvariant();
            if (ext != ".html" && ext != ".json") continue;

            list.Add(new ReportFileInfo
            {
                Name = Path.GetFileName(f),
                Type = typeLabel,
                Format = ext == ".html" ? "HTML" : "JSON",
                Size = new System.IO.FileInfo(f).Length,
                CreatedAt = System.IO.File.GetCreationTime(f),
                Url = $"/Reports/download?folder={folder}&file={Uri.EscapeDataString(Path.GetFileName(f))}"
            });
        }
    }

    public class ReportFileInfo
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Format { get; set; } = "";
        public long Size { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Url { get; set; } = "";
    }
}