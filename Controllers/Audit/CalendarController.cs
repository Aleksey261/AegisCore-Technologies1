using AegisCore.Models;
using AegisCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegisCore.Controllers.Audit;

[Authorize]
[Area("Audit")]
[Route("audit/calendar")]
public class CalendarController : Controller
{
    private readonly AuditService _audit;

    public CalendarController(AuditService audit)
    {
        _audit = audit;
    }

    [HttpGet("")]
    public IActionResult Index(DateTime? date, string? type, string? view)
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "calendar";
        
        var viewDate = date ?? DateTime.Now;
        ViewBag.ViewDate = viewDate;
        ViewBag.SelectedType = type;
        ViewBag.CurrentMode = view ?? "month";  // ИСПРАВЛЕНО: было ViewMode, стало CurrentMode
        
        var events = _audit.GetCalendarEvents();
        ViewBag.EventTypes = GetEventTypes();
        ViewBag.AllEvents = events;
        
        return View("~/Areas/Audit/Views/Calendar/Calendar.cshtml", events);
    }

    [HttpGet("api/events")]
    public IActionResult GetCalendarEventsApi(int? year, int? month)
    {
        var events = _audit.GetCalendarEvents();

        if (year.HasValue && month.HasValue)
        {
            events = events.Where(e =>
                e.StartDate.Year == year.Value &&
                e.StartDate.Month == month.Value
            ).ToList();
        }

        return Json(events.Select(e => new
        {
            id = e.Id,
            title = e.Title,
            startDate = e.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
            endDate = e.EndDate?.ToString("yyyy-MM-ddTHH:mm:ss"),
            type = e.Type,
            description = e.Description,
            completed = e.Completed
        }));
    }

    [HttpPost("event/save")]
    [ValidateAntiForgeryToken]
    public IActionResult SaveCalendarEvent(CalendarEventForm form)
    {
        var events = _audit.GetCalendarEvents();

        if (!string.IsNullOrWhiteSpace(form.Id))
        {
            var existingEvent = events.FirstOrDefault(e => e.Id == form.Id);
            if (existingEvent != null)
            {
                existingEvent.Title = form.Title;
                existingEvent.StartDate = DateTime.TryParse(form.StartDate, out var start) ? start : DateTime.Now;
                existingEvent.EndDate = DateTime.TryParse(form.EndDate, out var end) ? end : (DateTime?)null;
                existingEvent.Type = form.Type;
                existingEvent.Description = form.Description;
                existingEvent.Completed = form.Completed;
            }
        }
        else
        {
            events.Add(new CalendarEvent
            {
                Id = Guid.NewGuid().ToString(),
                Title = form.Title,
                StartDate = DateTime.TryParse(form.StartDate, out var start) ? start : DateTime.Now,
                EndDate = DateTime.TryParse(form.EndDate, out var end) ? end : (DateTime?)null,
                Type = form.Type,
                Description = form.Description,
                Completed = form.Completed
            });
        }

        _audit.SaveCalendarEvents(events);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = true });
        }

        return RedirectToAction("Index");
    }

    [HttpGet("event/edit/{id}")]
    public IActionResult EditCalendarEvent(string id)
    {
        var events = _audit.GetCalendarEvents();
        var eventToEdit = events.FirstOrDefault(e => e.Id == id);

        if (eventToEdit == null)
            return NotFound();

        return Json(new
        {
            id = eventToEdit.Id,
            title = eventToEdit.Title,
            startDate = eventToEdit.StartDate.ToString("yyyy-MM-dd"),
            endDate = eventToEdit.EndDate?.ToString("yyyy-MM-dd"),
            type = eventToEdit.Type,
            description = eventToEdit.Description,
            completed = eventToEdit.Completed
        });
    }

    [HttpPost("api/event/toggle/{id}")]
    [IgnoreAntiforgeryToken]
    public IActionResult ToggleCalendarEvent(string id)
    {
        var events = _audit.GetCalendarEvents();
        var eventToToggle = events.FirstOrDefault(e => e.Id == id);

        if (eventToToggle == null)
            return Json(new { success = false, error = "Событие не найдено" });

        eventToToggle.Completed = !eventToToggle.Completed;
        _audit.SaveCalendarEvents(events);

        return Json(new { success = true, completed = eventToToggle.Completed });
    }

    [HttpPost("event/delete/{id}")]
    [IgnoreAntiforgeryToken]
    public IActionResult DeleteCalendarEvent(string id)
    {
        var events = _audit.GetCalendarEvents().Where(e => e.Id != id).ToList();
        _audit.SaveCalendarEvents(events);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new { success = true });
        }

        return RedirectToAction("Index");
    }

    private Dictionary<string, string> GetEventTypes() => new()
    {
        { "planning", "Планерка" },
        { "backup", "Создание резервных копий" },
        { "testing", "Тестирование СЗИ" },
        { "incoming", "Входящие документы ДСП" },
        { "outgoing", "Исходящие документы ДСП" },
        { "skzi_oki", "Учет СКЗИ (ОКИ)" },
        { "skzi_okz", "Учет СКЗИ (ОКЗ)" },
        { "szi", "СЗИ" },
        { "szi_installation", "Установка СЗИ" },
        { "meeting", "Заседание" }
    };
}