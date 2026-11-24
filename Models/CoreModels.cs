namespace AegisCore.Models;

// === МОДЕЛИ СКАНЕРА ===

public class ScanTarget
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string TargetValue { get; set; } = "";
    public string Type { get; set; } = "HOST";
    public string ScanMode { get; set; } = "C1";
    public int? Timeout { get; set; } = 300;
    public bool Active { get; set; } = true;
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
}

public class ScanJob
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string JobId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string Status { get; set; } = "PENDING";
    public string? ScanMode { get; set; }
    public string? CurrentStep { get; set; }
    public int Progress { get; set; } = 0;
    public int? PortsScanned { get; set; }
    public int? HostsDiscovered { get; set; }
    public int? VulnerabilitiesFound { get; set; }
    public string? ErrorMessage { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ScheduledAt { get; set; }
}

public class ScanResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Host { get; set; } = "";
    public int? Port { get; set; }
    public string? Service { get; set; }
    public string? Protocol { get; set; }
    public string? Product { get; set; }
    public string? Version { get; set; }
    public string? ScanJobId { get; set; }
    public DateTime? ScannedAt { get; set; } = DateTime.Now;
    public string? BduId { get; set; }
    public string? CveId { get; set; }
    public double? CvssScore { get; set; }
    public string? FstecSeverity { get; set; }
    public string? FstecDescription { get; set; }
    public string? VulnStatus { get; set; }
    public string? Remediation { get; set; }
    public string? RemediationStatus { get; set; }
    public bool MatchedToFstec { get; set; }
    public DateTime? MatchedAt { get; set; }
    public string? ExtraInfo { get; set; }
}

public class Vulnerability
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string BduId { get; set; } = "";
    public string? CveId { get; set; }
    public string Title { get; set; } = "";
    public string Product { get; set; } = "";
    public string? Vendor { get; set; }
    public string? Version { get; set; }
    public string? Source { get; set; }
    public string Severity { get; set; } = "Средний";
    public double Cvss { get; set; }
    public string? Description { get; set; }
    public DateTime? PublishedDate { get; set; }
}

public class FstecThreat
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Severity { get; set; } = "Средний";
    public string? SourceModule { get; set; }
    public string? Description { get; set; }
}

public class IndustrialSecurityItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string AssetType { get; set; } = "";
    public string Name { get; set; } = "";
    public string RiskLevel { get; set; } = "Средний";
    public string? Description { get; set; }
    public string? RecommendedControl { get; set; }
}

public class ExportControlItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string? Category { get; set; }
}

public class PcScanItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Category { get; set; } = "";
    public string ItemKey { get; set; } = "";
    public string? ItemValue { get; set; }
    public string? Details { get; set; }
    public string? ScanRunId { get; set; }
}

public class AssessmentFinding
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ScanRunId { get; set; } = "";
    public string Risk { get; set; } = "Средний";
    public string Category { get; set; } = "";
    public string Product { get; set; } = "";
    public string? Host { get; set; }
    public string Title { get; set; } = "";
    public string? Evidence { get; set; }
    public string? Recommendation { get; set; }
    public string? BduId { get; set; }
    public string? CveId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public record DashboardStats(int Targets, int ScanJobs, int Vulnerabilities, int Findings);