using Microsoft.AspNetCore.Mvc;

namespace AegisCore.Controllers.Audit;

[Area("Audit")]
[Route("audit/ml-analysis")]
public class MLAnalysisDashboardController : Controller
{
    [HttpGet("dashboard")]
    public IActionResult Dashboard()
    {
        // Явно указываем путь к View, чтобы файл лежал в логичной папке MLAnalysis
        return View("~/Areas/Audit/Views/MLAnalysis/Dashboard.cshtml");
    }
}