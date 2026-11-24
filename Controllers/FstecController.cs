using AegisCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegisCore.Controllers;

[Authorize]
public class FstecController : Controller
{
    private readonly JsonDataStore _store;
    private readonly FstecImportService _import;
    private readonly AssessmentService _assessment;
    private readonly ReportService _report;
    private readonly MlClientService _ml;

    public FstecController(JsonDataStore store, FstecImportService import, AssessmentService assessment, ReportService report, MlClientService ml)
    {
        _store = store;
        _import = import;
        _assessment = assessment;
        _report = report;
        _ml = ml;
    }

    [HttpGet("/fstec")]
    [HttpGet("/fstec/index")]
    public IActionResult Index() => View();

    [HttpGet("/api/fstec/vulnerabilities")]
    public IActionResult Vulnerabilities() => Json(_store.Vulnerabilities);

    [HttpGet("/api/fstec/threats")]
    public IActionResult Threats() => Json(_store.Threats);

    [HttpGet("/api/fstec/industrial")]
    public IActionResult Industrial() => Json(_store.IndustrialItems);

    [HttpGet("/api/fstec/export-control")]
    public IActionResult ExportControl() => Json(_store.ExportControlItems);

    [HttpPost("/api/fstec/import")]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest(new { error = "Файл не передан" });
        var result = await _import.ImportVulnerabilitiesAsync(file);
        return Json(result);
    }

    [HttpPost("/api/fstec/assess")]
    public IActionResult Assess() => Json(new { findings = _assessment.AssessLatestScan() });

    [HttpGet("/api/fstec/report/html")]
    public IActionResult ReportHtml() => Content(_report.BuildHtmlReport(), "text/html; charset=utf-8");

    [HttpGet("/api/fstec/report/pdf")]
    public IActionResult ReportPdf() => File(_report.BuildPdfReport(), "application/pdf", $"fstec-report-{DateTime.Now:yyyyMMddHHmmss}.pdf");

    [HttpPost("/api/fstec/ml/analyze")]
    public async Task<IActionResult> AnalyzeWithMl() => Content(await _ml.AnalyzeAsync(new { findings = _store.AssessmentFindings.Take(100) }), "application/json");
}
