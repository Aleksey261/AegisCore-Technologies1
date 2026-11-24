using AegisCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegisCore.Controllers.Audit;

[Authorize]
[Area("Audit")]
[Route("audit/journals")]
public class JournalsController : Controller
{
    private readonly AuditService _audit;

    public JournalsController(AuditService audit)
    {
        _audit = audit;
    }

    [HttpGet("")]
    [HttpGet("{type?}")]
    public IActionResult Index(string? type)
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "journals";
        ViewBag.ActiveSubSection = type ?? "default";
        ViewBag.JournalType = type ?? "default";
        
        var entries = _audit.GetJournals(type ?? "default");
        return View("~/Areas/Audit/Views/Journals/Journals.cshtml", entries);
    }
}