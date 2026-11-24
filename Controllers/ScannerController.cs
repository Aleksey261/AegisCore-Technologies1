using AegisCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegisCore.Controllers;

[Authorize]
public class ScannerController : Controller
{
    private readonly JsonDataStore _store;
    private readonly ScannerService _scanner;
    private readonly AssessmentService _assessment;
    private readonly ReportService _report;

    public ScannerController(JsonDataStore store, ScannerService scanner, AssessmentService assessment, ReportService report)
    {
        _store = store;
        _scanner = scanner;
        _assessment = assessment;
        _report = report;
    }

    [HttpGet("/scanner")]
    [HttpGet("/scanner/index")]
    public IActionResult Index() => View();

    [HttpGet("/scanner/scans")]
    public IActionResult Scans() => View(_store.ScanJobs.OrderByDescending(j => j.CreatedAt).ToList());

    [HttpGet("/scanner/vulnerabilities")]
    public IActionResult Vulnerabilities() => View(_store.Vulnerabilities.OrderBy(v => v.Product).ToList());

    [HttpGet("/scanner/reports")]
    public IActionResult Reports() => Content(_report.BuildHtmlReport(), "text/html; charset=utf-8");

    [HttpGet("/scanner/report/pdf")]
    public IActionResult ReportPdf(string? scanRunId = null) => File(_report.BuildPdfReport(scanRunId), "application/pdf", $"aegis-report-{DateTime.Now:yyyyMMddHHmmss}.pdf");

    [HttpPost("/scanner/start")]
    public async Task<IActionResult> Start(string target = "127.0.0.1", string scanMode = "C1")
    {
        var job = await _scanner.StartScanAsync(target, scanMode);
        return Json(new { ok = true, job.JobId, job.Status, job.PortsScanned, job.VulnerabilitiesFound });
    }

    [HttpPost("/scanner/assess")]
    public IActionResult Assess()
    {
        var findings = _assessment.AssessLatestScan();
        return Json(new { ok = true, total = findings.Count, critical = findings.Count(f => f.Risk == "Критический"), high = findings.Count(f => f.Risk == "Высокий") });
    }
}
