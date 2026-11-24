using AegisCore.Models;
using Microsoft.ML;

namespace AegisCore.Services;

public class ComplianceResult
{
    public List<ComplianceMetric> Metrics { get; set; } = new();
    public double OverallComplianceScore { get; set; }
}

public class ComplianceCalculator
{
    private readonly MLPipeline _mlPipeline;
    private readonly List<RegulatoryRequirement> _regulatoryRequirements;

    public ComplianceCalculator(MLPipeline mlPipeline)
    {
        _mlPipeline = mlPipeline;
        _regulatoryRequirements = InitializeRequirements();
    }

    public ComplianceResult CalculateCompliance(List<VulnerabilityScanResult> scanResults, string systemId, string modelName = "LightGBM")
    {
        var metrics = new List<ComplianceMetric>();
        var predictionEngine = _mlPipeline.CreatePredictionEngine(modelName);

        foreach (var requirement in _regulatoryRequirements)
        {
            var securityData = ConvertToSecurityData(scanResults, requirement);
            var prediction = predictionEngine.Predict(securityData);

            metrics.Add(new ComplianceMetric
            {
                RequirementId = requirement.Id,
                SystemId = systemId,
                AssessmentDate = DateTime.UtcNow,
                IsCompliant = prediction.PredictedLabel,
                ConfidenceScore = prediction.Score,
                Evidence = GenerateEvidence(scanResults, requirement)
            });
        }

        var compliantCount = metrics.Count(m => m.IsCompliant);
        var overallScore = metrics.Count > 0 ? (double)compliantCount / metrics.Count * 100 : 0;

        return new ComplianceResult 
        { 
            Metrics = metrics,
            OverallComplianceScore = overallScore
        };
    }

    private SecurityData ConvertToSecurityData(List<VulnerabilityScanResult> scanResults, RegulatoryRequirement requirement)
    {
        // ИСПРАВЛЕНО: Явное указание 1f и 0f для типа float
        return new SecurityData
        {
            PortOpen = scanResults.Any(s => !string.IsNullOrEmpty(s.Description) && s.Description.Contains("порт", StringComparison.OrdinalIgnoreCase)) ? 1f : 0f,
            WeakPassword = scanResults.Any(s => s.Severity == "Critical") ? 1f : 0f,
            OutdatedSoftware = scanResults.Count(s => s.Severity == "High") > 5 ? 1f : 0f,
            UnencryptedTraffic = scanResults.Any(s => !string.IsNullOrEmpty(s.Description) && s.Description.Contains("шифрование", StringComparison.OrdinalIgnoreCase)) ? 1f : 0f,
            Label = false
        };
    }

    private string GenerateEvidence(List<VulnerabilityScanResult> scanResults, RegulatoryRequirement requirement)
    {
        var relevantResults = scanResults.Where(s => 
            !string.IsNullOrEmpty(s.Description) && 
            s.Description.Contains(requirement.KeyWord, StringComparison.OrdinalIgnoreCase)).ToList();
        
        if (relevantResults.Count > 0)
        {
            return $"Выявлено {relevantResults.Count} нарушений по требованию {requirement.Id}: " +
                   string.Join("; ", relevantResults.Select(r => r.Description.Length > 80 ? r.Description[..80] + "..." : r.Description));
        }
        
        return $"Нарушений по требованию {requirement.Id} не выявлено";
    }

    private List<RegulatoryRequirement> InitializeRequirements()
    {
        return new List<RegulatoryRequirement>
        {
            new() { Id = "ФСТЭК_21_п.5", KeyWord = "аутентификация" },
            new() { Id = "ФСТЭК_21_п.10", KeyWord = "шифрование" },
            new() { Id = "ФСТЭК_21_п.15", KeyWord = "антивирус" }
        };
    }
}

public class RegulatoryRequirement
{
    public string Id { get; set; } = string.Empty;
    public string KeyWord { get; set; } = string.Empty;
}