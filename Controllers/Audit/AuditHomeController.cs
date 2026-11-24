using AegisCore.Models;
using AegisCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegisCore.Controllers.Audit;

[Authorize]
[Area("Audit")]
public class AuditHomeController : Controller
{
    private readonly AuditService _audit;

    public AuditHomeController(AuditService audit)
    {
        _audit = audit;
    }

    [Route("audit")]        
    [Route("audit/index")]
    public IActionResult Index()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "";
        var stats = _audit.GetAuditStats();
        var activities = _audit.GetRecentActivities(5);
        return View("~/Areas/Audit/Views/Index.cshtml", new AuditIndexModel { Stats = stats, Activities = activities });
    }
}