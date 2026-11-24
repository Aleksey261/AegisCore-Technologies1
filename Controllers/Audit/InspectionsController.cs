using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegisCore.Controllers.Audit;

[Authorize]
[Area("Audit")]
[Route("audit/inspections")]
public class InspectionsController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "inspections";
        ViewBag.ActiveSubSection = "internal";
        return View("~/Areas/Audit/Views/Inspections/Inspections.cshtml");
    }

    [HttpGet("internal")]
    public IActionResult Internal()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "inspections";
        ViewBag.ActiveSubSection = "internal";
        return View("~/Areas/Audit/Views/Inspections/Inspections.cshtml");
    }

    [HttpGet("external")]
    public IActionResult External()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "inspections";
        ViewBag.ActiveSubSection = "external";
        return View("~/Areas/Audit/Views/Inspections/Inspections.cshtml");
    }

    [HttpGet("regulators")]
    public IActionResult Regulators()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "inspections";
        ViewBag.ActiveSubSection = "regulators";
        return View("~/Areas/Audit/Views/Inspections/Inspections.cshtml");
    }
}