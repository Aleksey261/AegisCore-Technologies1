using System.ComponentModel.DataAnnotations;

namespace AegisCore.Models;

// Новый класс для ML-задач (не конфликтует с существующим ScanResult)
public class VulnerabilityScanResult
{
    public int Id { get; set; }
    public string TargetHost { get; set; } = string.Empty;
    public DateTime ScanDate { get; set; }
    public string VulnerabilityId { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Remediation { get; set; } = string.Empty;
}

public class ComplianceMetric
{
    public int Id { get; set; }
    public string RequirementId { get; set; } = string.Empty;
    public string SystemId { get; set; } = string.Empty;
    public DateTime AssessmentDate { get; set; }
    public bool IsCompliant { get; set; }
    public double ConfidenceScore { get; set; }
    public string Evidence { get; set; } = string.Empty;
}

public class SecurityIncident
{
    public int Id { get; set; }
    public DateTime DetectedAt { get; set; }
    public string IncidentType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string AffectedSystem { get; set; } = string.Empty;
    public string MlModelUsed { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}