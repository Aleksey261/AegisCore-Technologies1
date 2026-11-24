namespace AegisCore.Services;

public class SystemMonitoringSettings
{
    public bool IsAutoMonitoringEnabled { get; set; } = false;
    public int IntervalMinutes { get; set; } = 60; // По умолчанию 1 час
    public DateTime? LastScanTime { get; set; }
    public string? LastScanResult { get; set; }
    public string? LastScanFile { get; set; }
    public int TotalScans { get; set; } = 0;

    public DateTime? NextScanTime => IsAutoMonitoringEnabled && LastScanTime.HasValue
        ? LastScanTime.Value.AddMinutes(IntervalMinutes)
        : null;
}