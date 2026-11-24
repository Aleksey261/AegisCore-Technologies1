using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ML;
using Microsoft.ML.Data;
using AegisCore.Models;
using AegisCore.Services;

namespace AegisCore.Controllers.Audit;

[ApiController]
[Route("api/ml-analysis")]
public class MLAnalysisController : ControllerBase
{
    private readonly ComplianceCalculator _calculator;
    private readonly MLPipeline _pipeline;
    private readonly SystemCollectorService _systemCollector;
    private readonly LocalScanAdapter _localAdapter;
    private readonly SystemMonitoringSettings _settings;
    private readonly SystemMonitoringBackgroundService _monitoringService;
    private static string _selectedModel = "LightGBM";

    private const string CicDatasetPath = @"J:\CIC-IDS2017\MachineLearningCVE\Friday-WorkingHours-Afternoon-DDos.pcap_ISCX.csv";

    public MLAnalysisController(
        ComplianceCalculator calculator, 
        MLPipeline pipeline,
        SystemCollectorService systemCollector,
        LocalScanAdapter localAdapter,
        SystemMonitoringSettings settings,
        SystemMonitoringBackgroundService monitoringService)
    {
        _calculator = calculator;
        _pipeline = pipeline;
        _systemCollector = systemCollector;
        _localAdapter = localAdapter;
        _settings = settings;
        _monitoringService = monitoringService;
    }

    [HttpGet("models")]
    public IActionResult GetAvailableModels()
    {
        var models = _pipeline.GetAvailableModels().Select(m => new
        {
            key = m.Key,
            name = m.Value.ModelName,
            description = m.Value.Description
        });
        return Ok(models);
    }

    [HttpPost("train")]
    public IActionResult TrainModel([FromBody] TrainRequest? request)
    {
        try
        {
            var modelName = request?.ModelName ?? "LightGBM";
            var (trainingData, _) = GetTrainingData();

            var metrics = _pipeline.TrainModel(modelName, trainingData);
            _selectedModel = modelName;

            return Ok(new { 
                success = true, 
                message = $"Модель {modelName} успешно обучена",
                metrics = new {
                    modelName = metrics.ModelName,
                    accuracy = Math.Round(metrics.Accuracy * 100, 2),
                    precision = Math.Round(metrics.Precision * 100, 2),
                    recall = Math.Round(metrics.Recall * 100, 2),
                    f1Score = Math.Round(metrics.F1Score * 100, 2),
                    auc = Math.Round(metrics.Auc * 100, 2)
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpPost("train-all")]
    public IActionResult TrainAllModels()
    {
        try
        {
            Console.WriteLine("=== ЗАПУСК ПОЛНОГО ЦИКЛА ОБУЧЕНИЯ ===");

            var (trainingData, dataSourceName) = GetTrainingData();

            var allMetrics = _pipeline.TrainAllModels(trainingData);

            var results = allMetrics.Select(m => new {
                modelName = m.Value.ModelName,
                accuracy = Math.Round(m.Value.Accuracy * 100, 2),
                precision = Math.Round(m.Value.Precision * 100, 2),
                recall = Math.Round(m.Value.Recall * 100, 2),
                f1Score = Math.Round(m.Value.F1Score * 100, 2),
                auc = Math.Round(m.Value.Auc * 100, 2),
                // Новые данные для визуализации на фронтенде
                trainingTimeMs = m.Value.TrainingTimeMs,
                truePositives = m.Value.TruePositives,
                trueNegatives = m.Value.TrueNegatives,
                falsePositives = m.Value.FalsePositives,
                falseNegatives = m.Value.FalseNegatives,
                totalEvaluated = m.Value.TotalEvaluatedRecords
            });

            Console.WriteLine("=== ОБУЧЕНИЕ ЗАВЕРШЕНО УСПЕШНО ===");

            return Ok(new { 
                success = true, 
                dataSource = dataSourceName,
                totalRecords = trainingData.Count,
                models = results 
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"КРИТИЧЕСКАЯ ОШИБКА: {ex.Message}");
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpPost("train-local")]
    public async Task<IActionResult> TrainOnLocalSystem()
    {
        try
        {
            var systemInfo = await _systemCollector.CollectSystemInfoAsync();
            var localData = _localAdapter.ConvertToSecurityData(systemInfo);
            var syntheticData = GenerateRealisticTrainingData();
            var combinedData = localData.Concat(syntheticData).ToList();

            var allMetrics = _pipeline.TrainAllModels(combinedData);

            var results = allMetrics.Select(m => new {
                modelName = m.Value.ModelName,
                accuracy = Math.Round(m.Value.Accuracy * 100, 2),
                precision = Math.Round(m.Value.Precision * 100, 2),
                recall = Math.Round(m.Value.Recall * 100, 2),
                f1Score = Math.Round(m.Value.F1Score * 100, 2),
                auc = Math.Round(m.Value.Auc * 100, 2)
            });

            var report = _localAdapter.GenerateReport(systemInfo);

            return Ok(new { 
                success = true,
                systemName = systemInfo.ComputerName,
                osVersion = systemInfo.OSVersion,
                vulnerabilitiesFound = systemInfo.Vulnerabilities.Count,
                openPorts = systemInfo.OpenPorts.Count,
                installedSoftware = systemInfo.InstalledSoftware.Count,
                localRecordsCount = localData.Count,
                totalRecordsCount = combinedData.Count,
                models = results,
                report = report
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpGet("system-info")]
    public async Task<IActionResult> GetSystemInfo()
    {
        try
        {
            var systemInfo = await _systemCollector.CollectSystemInfoAsync();
            var report = _localAdapter.GenerateReport(systemInfo);

            return Ok(new {
                computerName = systemInfo.ComputerName,
                osVersion = systemInfo.OSVersion,
                cpu = systemInfo.CPUName,
                ramGB = systemInfo.TotalRAM_GB,
                networkInterfaces = systemInfo.NetworkInterfaces.Count,
                openPorts = systemInfo.OpenPorts.Count,
                activeConnections = systemInfo.ActiveConnections.Count,
                installedSoftware = systemInfo.InstalledSoftware.Count,
                runningProcesses = systemInfo.RunningProcesses.Count,
                services = systemInfo.Services.Count,
                firewallEnabled = systemInfo.FirewallEnabled,
                antivirusPresent = systemInfo.AntivirusPresent,
                antivirusName = systemInfo.AntivirusName,
                windowsUpdateEnabled = systemInfo.WindowsUpdateEnabled,
                vulnerabilities = systemInfo.Vulnerabilities.Select(v => new {
                    type = v.Type,
                    severity = v.Severity,
                    description = v.Description,
                    recommendation = v.Recommendation
                }).ToList(),
                report = report
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpPost("assess/{systemId}")]
    public IActionResult AssessSystem(string systemId, [FromBody] AssessRequest? request)
    {
        var modelName = request?.ModelName ?? _selectedModel;

        try
        {
            var scanResults = GetRealisticScanResults();
            var result = _calculator.CalculateCompliance(scanResults, systemId, modelName);
            var metrics = _pipeline.GetMetrics(modelName);

            return Ok(new {
                systemId = systemId,
                modelName = modelName,
                assessmentDate = DateTime.UtcNow,
                overallComplianceScore = Math.Round(result.OverallComplianceScore, 1),
                metrics = result.Metrics.Select(m => new {
                    requirementId = m.RequirementId,
                    isCompliant = m.IsCompliant,
                    confidenceScore = Math.Round(Math.Max(0, Math.Min(1, m.ConfidenceScore)) * 100, 1),
                    evidence = m.Evidence
                }).ToList(),
                modelMetrics = metrics != null ? new {
                    accuracy = Math.Round(metrics.Accuracy * 100, 2),
                    precision = Math.Round(metrics.Precision * 100, 2),
                    recall = Math.Round(metrics.Recall * 100, 2),
                    f1Score = Math.Round(metrics.F1Score * 100, 2),
                    auc = Math.Round(metrics.Auc * 100, 2)
                } : null
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("compare")]
    public IActionResult CompareModels()
    {
        var allMetrics = _pipeline.GetAllMetrics();

        if (allMetrics.Count == 0)
            return BadRequest(new { error = "Сначала обучите модели" });

        var results = allMetrics.Select(m => new {
            modelName = m.Value.ModelName,
            accuracy = Math.Round(m.Value.Accuracy * 100, 2),
            precision = Math.Round(m.Value.Precision * 100, 2),
            recall = Math.Round(m.Value.Recall * 100, 2),
            f1Score = Math.Round(m.Value.F1Score * 100, 2),
            auc = Math.Round(m.Value.Auc * 100, 2)
        });

        return Ok(results);
    }

    // ====== МОНИТОРИНГ API ======

    [HttpGet("monitoring")]
    public IActionResult GetMonitoringStatus()
    {
        return Ok(new {
            isEnabled = _settings.IsAutoMonitoringEnabled,
            intervalMinutes = _settings.IntervalMinutes,
            lastScanTime = _settings.LastScanTime,
            lastScanResult = _settings.LastScanResult,
            lastScanFile = _settings.LastScanFile,
            nextScanTime = _settings.NextScanTime,
            totalScans = _settings.TotalScans
        });
    }

    [HttpPost("monitoring")]
    public IActionResult UpdateMonitoring([FromBody] MonitoringUpdateRequest? request)
    {
        if (request == null)
            return BadRequest(new { error = "Нет данных" });

        if (request.IsAutoMonitoringEnabled.HasValue)
            _settings.IsAutoMonitoringEnabled = request.IsAutoMonitoringEnabled.Value;

        if (request.IntervalMinutes.HasValue)
        {
            var allowed = new[] { 1, 5, 15, 30, 60, 360, 1440 };
            if (!allowed.Contains(request.IntervalMinutes.Value))
                return BadRequest(new { error = "Недопустимый интервал" });
            _settings.IntervalMinutes = request.IntervalMinutes.Value;
        }

        return Ok(new { 
            success = true, 
            message = "Настройки мониторинга обновлены",
            isEnabled = _settings.IsAutoMonitoringEnabled,
            intervalMinutes = _settings.IntervalMinutes
        });
    }

    [HttpPost("scan-now")]
    public async Task<IActionResult> ScanNow()
    {
        try
        {
            await _monitoringService.PerformScanAsync();

            return Ok(new {
                success = true,
                message = "Сканирование завершено",
                lastScanTime = _settings.LastScanTime,
                lastScanResult = _settings.LastScanResult,
                lastScanFile = _settings.LastScanFile
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpGet("scans")]
    public IActionResult GetScansList([FromQuery] int limit = 25)
    {
        var scansDir = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "AutoScans");
        if (!Directory.Exists(scansDir))
            return Ok(new List<object>());

        var allowedLimits = new[] { 10, 25, 50, 100 };
        if (!allowedLimits.Contains(limit))
            limit = 25;

        var files = Directory.GetFiles(scansDir, "scan_*.json")
            .OrderByDescending(f => f)
            .Take(limit)
            .Select(f => new {
                fileName = Path.GetFileName(f),
                createdAt = System.IO.File.GetCreationTimeUtc(f),
                size = new System.IO.FileInfo(f).Length
            })
            .ToList();

        return Ok(new { files, total = files.Count, limit });
    }

    [HttpGet("scans/{fileName}")]
    public IActionResult GetScanDetails(string fileName)
    {
        var scansDir = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "AutoScans");
        var filePath = Path.Combine(scansDir, fileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound(new { error = "Файл не найден" });

        var json = System.IO.File.ReadAllText(filePath);
        return Content(json, "application/json");
    }

    // ====== ИСТОЧНИК ДАННЫХ ДЛЯ ОБУЧЕНИЯ ======
    private (List<SecurityData> Data, string SourceName) GetTrainingData(int maxRecords = 150000)
    {
        if (System.IO.File.Exists(CicDatasetPath))
        {
            Console.WriteLine("Найден датасет CIC-IDS2017. Начинается загрузка и маппинг...");
            var data = LoadAndMapCicIds2017Data(CicDatasetPath, maxRecords);
            Console.WriteLine($"Загружено {data.Count} записей для обучения.");
            return (data, "CIC-IDS2017 (Real Dataset)");
        }

        Console.WriteLine("Датасет не найден. Используется синтетическая генерация...");
        return (GenerateRealisticTrainingData(), "Synthetic (Fallback)");
    }

    // ====== ЗАГРУЗКА И МАППИНГ CIC-IDS2017 ======
    private List<SecurityData> LoadAndMapCicIds2017Data(string filePath, int maxRecords = 100000)
    {
        var data = new List<SecurityData>();
        var random = new Random(42);
        int skipped = 0;

        var riskyPorts = new HashSet<string> { "21", "22", "23", "135", "445", "3389", "1433", "3306" };
        var unencryptedPorts = new HashSet<string> { "21", "23", "80" };

        try
        {
            var lines = System.IO.File.ReadLines(filePath);
            var headerLine = lines.FirstOrDefault();
            if (string.IsNullOrEmpty(headerLine))
            {
                Console.WriteLine("[ML] Пустой файл датасета — синтетические данные.");
                return GenerateRealisticTrainingData();
            }

            // КРИТИЧНО: имена колонок CIC-IDS2017 идут с ведущими пробелами
            var headers = ParseCsvLine(headerLine).Select(h => h.Trim()).ToList();
            int ColIdx(string n) => headers.IndexOf(n);

            int destPortIdx = ColIdx("Destination Port");
            int labelIdx = ColIdx("Label");
            int flowDurationIdx = ColIdx("Flow Duration");
            int totalFwdIdx = ColIdx("Total Fwd Packets");
            int totalBwdIdx = ColIdx("Total Backward Packets");
            int lenFwdIdx = ColIdx("Total Length of Fwd Packets");
            int lenBwdIdx = ColIdx("Total Length of Bwd Packets");
            int fwdMeanIdx = ColIdx("Fwd Packet Length Mean");
            int bwdMeanIdx = ColIdx("Bwd Packet Length Mean");
            int flowBytesIdx = ColIdx("Flow Bytes/s");
            int flowPacketsIdx = ColIdx("Flow Packets/s");
            int flowIatIdx = ColIdx("Flow IAT Mean");
            int pktMeanIdx = ColIdx("Packet Length Mean");
            int pktStdIdx = ColIdx("Packet Length Std");
            int synIdx = ColIdx("SYN Flag Count");
            int ackIdx = ColIdx("ACK Flag Count");
            int avgPktIdx = ColIdx("Average Packet Size");
            int winFwdIdx = ColIdx("Init_Win_bytes_forward");
            int winBwdIdx = ColIdx("Init_Win_bytes_backward");

            if (labelIdx == -1 || destPortIdx == -1)
            {
                Console.WriteLine("[ML] В шапке CSV нет Label/Destination Port — синтетические данные.");
                return GenerateRealisticTrainingData();
            }

            float Num(List<string> f, int idx)
            {
                if (idx < 0 || idx >= f.Count) return 0f;
                return float.TryParse(f[idx].Trim(), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var v) ? v : 0f;
            }

            int processedCount = 0;
            foreach (var line in lines.Skip(1))
            {
                if (processedCount >= maxRecords) break;
                if (random.Next(10) != 0) continue; // ~10% выборка

                var fields = ParseCsvLine(line);
                if (fields.Count <= labelIdx) continue;

                var label = fields[labelIdx].Trim();
                bool isAttack = !label.Equals("BENIGN", StringComparison.OrdinalIgnoreCase);

                var destPort = destPortIdx < fields.Count ? fields[destPortIdx].Trim() : "0";

                float flowBytes = Num(fields, flowBytesIdx);
                float flowPackets = Num(fields, flowPacketsIdx);
                if (float.IsNaN(flowBytes) || float.IsInfinity(flowBytes) ||
                    float.IsNaN(flowPackets) || float.IsInfinity(flowPackets))
                {
                    skipped++;
                    continue; // ML.NET не принимает NaN/Infinity
                }

                data.Add(new SecurityData
                {
                    // Флаги — только из порта, БЕЗ утечки метки
                    PortOpen = riskyPorts.Contains(destPort) ? 1f : 0f,
                    UnencryptedTraffic = unencryptedPorts.Contains(destPort) ? 1f : 0f,
                    WeakPassword = 0f,     // из сетевых потоков неизвестно
                    OutdatedSoftware = 0f, // из сетевых потоков неизвестно

                    DestinationPort = Num(fields, destPortIdx),
                    FlowDuration = Num(fields, flowDurationIdx),
                    TotalFwdPackets = Num(fields, totalFwdIdx),
                    TotalBackwardPackets = Num(fields, totalBwdIdx),
                    TotalLengthFwdPackets = Num(fields, lenFwdIdx),
                    TotalLengthBwdPackets = Num(fields, lenBwdIdx),
                    FwdPacketLengthMean = Num(fields, fwdMeanIdx),
                    BwdPacketLengthMean = Num(fields, bwdMeanIdx),
                    FlowBytesPerSec = flowBytes,
                    FlowPacketsPerSec = flowPackets,
                    FlowIatMean = Num(fields, flowIatIdx),
                    PacketLengthMean = Num(fields, pktMeanIdx),
                    PacketLengthStd = Num(fields, pktStdIdx),
                    SynFlagCount = Num(fields, synIdx),
                    AckFlagCount = Num(fields, ackIdx),
                    AveragePacketSize = Num(fields, avgPktIdx),
                    InitWinBytesForward = Num(fields, winFwdIdx),
                    InitWinBytesBackward = Num(fields, winBwdIdx),
                    Label = isAttack
                });
                processedCount++;
            }

            Console.WriteLine($"[ML] CIC-IDS2017: загружено {data.Count} РЕАЛЬНЫХ записей (пропущено {skipped} с NaN/Infinity).");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ML] Ошибка чтения датасета: {ex.Message}. Синтетические данные.");
            return GenerateRealisticTrainingData();
        }

        return data.OrderBy(x => random.Next()).ToList();
    }

    // Простой и надежный парсер CSV-строки, корректно обрабатывающий запятые внутри кавычек
    private List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var currentField = new StringBuilder();

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }
        result.Add(currentField.ToString());
        return result;
    }

    // Синтетический fallback: используется ТОЛЬКО если датасет недоступен
    private List<SecurityData> GenerateRealisticTrainingData()
    {
        var data = new List<SecurityData>();
        var random = new Random(42);

        for (int i = 0; i < 10000; i++)
        {
            float portOpen, weakPassword, outdated, unencrypted;
            int scenario = random.Next(100);

            if (scenario < 50)
            {
                portOpen = random.NextDouble() > 0.95f ? 1f : 0f;
                weakPassword = random.NextDouble() > 0.98f ? 1f : 0f;
                outdated = random.NextDouble() > 0.90f ? 1f : 0f;
                unencrypted = random.NextDouble() > 0.95f ? 1f : 0f;
            }
            else if (scenario < 80)
            {
                portOpen = random.NextDouble() > 0.60f ? 1f : 0f;
                weakPassword = random.NextDouble() > 0.70f ? 1f : 0f;
                outdated = random.NextDouble() > 0.65f ? 1f : 0f;
                unencrypted = random.NextDouble() > 0.60f ? 1f : 0f;
            }
            else
            {
                portOpen = random.NextDouble() > 0.20f ? 1f : 0f;
                weakPassword = random.NextDouble() > 0.30f ? 1f : 0f;
                outdated = random.NextDouble() > 0.40f ? 1f : 0f;
                unencrypted = random.NextDouble() > 0.30f ? 1f : 0f;
            }

            bool isAttack = scenario >= 50
                ? random.NextDouble() > 0.2
                : random.NextDouble() > 0.9;

            if (random.NextDouble() < 0.1) isAttack = !isAttack; // 10% шума в метках

            data.Add(new SecurityData
            {
                PortOpen = portOpen,
                WeakPassword = weakPassword,
                OutdatedSoftware = outdated,
                UnencryptedTraffic = unencrypted,
                Label = isAttack
            });
        }

        return data;
    }

    private List<VulnerabilityScanResult> GetRealisticScanResults()
    {
        return new List<VulnerabilityScanResult>
        {
            new() { 
                TargetHost = "192.168.1.10", 
                ScanDate = DateTime.UtcNow,
                VulnerabilityId = "CVE-2024-1234", 
                Severity = "Critical", 
                Description = "Открыт порт 22 (SSH) без аутентификации. Слабый пароль администратора.",
                Remediation = "Настройте аутентификацию по ключам, обновите пароль"
            },
            new() { 
                TargetHost = "192.168.1.20", 
                ScanDate = DateTime.UtcNow,
                VulnerabilityId = "CVE-2024-5678", 
                Severity = "High", 
                Description = "Устаревшая версия OpenSSL 1.0.2 (уязвимость к атакам на шифрование)",
                Remediation = "Обновите OpenSSL до версии 3.0 или выше"
            },
            new() { 
                TargetHost = "192.168.1.30", 
                ScanDate = DateTime.UtcNow,
                VulnerabilityId = "CVE-2024-9012", 
                Severity = "Medium", 
                Description = "HTTP трафик передаётся без шифрования (отсутствует HTTPS)",
                Remediation = "Настройте SSL/TLS сертификаты"
            },
            new() { 
                TargetHost = "192.168.1.40", 
                ScanDate = DateTime.UtcNow,
                VulnerabilityId = "BDU:2024-00123", 
                Severity = "High", 
                Description = "Порт 3389 (RDP) открыт для внешнего доступа. Слабая аутентификация.",
                Remediation = "Ограничьте доступ по RDP через firewall, используйте VPN"
            },
            new() { 
                TargetHost = "192.168.1.50", 
                ScanDate = DateTime.UtcNow,
                VulnerabilityId = null, 
                Severity = "Low", 
                Description = "Система обновлена, критических уязвимостей не найдено",
                Remediation = "Продолжайте регулярно обновлять систему"
            }
        };
    }
}

public class TrainRequest
{
    public string? ModelName { get; set; }
}

public class AssessRequest
{
    public string? ModelName { get; set; }
}

public class MonitoringUpdateRequest
{
    public bool? IsAutoMonitoringEnabled { get; set; }
    public int? IntervalMinutes { get; set; }
}