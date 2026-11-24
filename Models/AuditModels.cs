namespace AegisCore.Models;

// === МОДЕЛИ АУДИТА ===

public class AuditActivity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Date { get; set; } = DateTime.Now;
    public string Event { get; set; } = "";
    public string User { get; set; } = "";
    public string Status { get; set; } = "OK";
}

public class CalendarEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public bool Completed { get; set; }
}

public class JournalEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string JournalType { get; set; } = "";
    public DateTime EntryDate { get; set; } = DateTime.Now;
    public string? CreatedBy { get; set; }
    public Dictionary<string, string> Fields { get; set; } = new();
}

// Статистика для дашборда аудита
public record AuditDashboardStats(int Departments, int Employees, int Rooms, int Journals, int Activities);