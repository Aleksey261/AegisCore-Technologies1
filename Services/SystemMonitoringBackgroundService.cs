using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace AegisCore.Services;

public class SystemMonitoringBackgroundService : BackgroundService
{
    private readonly SystemCollectorService _collector;
    private readonly LocalScanAdapter _adapter;
    private readonly SystemMonitoringSettings _settings;
    private readonly ILogger<SystemMonitoringBackgroundService> _logger;
    private readonly string _dataDirectory;

    public SystemMonitoringBackgroundService(
        SystemCollectorService collector,
        LocalScanAdapter adapter,
        SystemMonitoringSettings settings,
        IWebHostEnvironment env,
        ILogger<SystemMonitoringBackgroundService> logger)
    {
        _collector = collector;
        _adapter = adapter;
        _settings = settings;
        _logger = logger;
        
        _dataDirectory = Path.Combine(env.ContentRootPath, "App_Data", "AutoScans");
        if (!Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🔄 Служба мониторинга запущена. Авто-сканирование: " +
            (_settings.IsAutoMonitoringEnabled ? "ВКЛ" : "ВЫКЛ") +
            $", интервал: {_settings.IntervalMinutes} мин.");

        // Ждем 10 секунд после старта, чтобы приложение полностью поднялось
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_settings.IsAutoMonitoringEnabled)
            {
                // Если авто-мониторинг выключен, спим 30 секунд и проверяем снова
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                continue;
            }

            // Проверяем, не пора ли сканировать
            var now = DateTime.UtcNow;
            var nextScan = _settings.NextScanTime;
            
            if (nextScan == null || now >= nextScan)
            {
                await PerformScanAsync(stoppingToken);
            }

            // Короткая проверка каждые 30 секунд (чтобы быстро реагировать на включение/выключение)
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    public async Task PerformScanAsync(CancellationToken stoppingToken = default)
    {
        try
        {
            _logger.LogInformation("🔍 Сбор данных с локальной системы...");
            
            var systemInfo = await _collector.CollectSystemInfoAsync();
            var securityData = _adapter.ConvertToSecurityData(systemInfo);
            
            bool isHighRisk = securityData.Any(d => d.Label);
            string riskLevel = isHighRisk ? "ВЫСОКИЙ" : "НОРМА";
            
            var fileName = $"scan_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(_dataDirectory, fileName);
            
            var reportData = new 
            {
                Timestamp = DateTime.UtcNow,
                ComputerName = systemInfo.ComputerName,
                OSVersion = systemInfo.OSVersion,
                RiskLevel = riskLevel,
                VulnerabilitiesFound = systemInfo.Vulnerabilities.Count,
                OpenPorts = systemInfo.OpenPorts.Count,
                FirewallEnabled = systemInfo.FirewallEnabled,
                AntivirusPresent = systemInfo.AntivirusPresent,
                WindowsUpdateEnabled = systemInfo.WindowsUpdateEnabled,
                Vulnerabilities = systemInfo.Vulnerabilities.Select(v => new {
                    v.Type, v.Severity, v.Description, v.Recommendation
                }),
                RawData = securityData
            };

            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true, 
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
            };
            await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(reportData, options), stoppingToken);
            
            // Обновляем настройки
            _settings.LastScanTime = DateTime.UtcNow;
            _settings.LastScanResult = riskLevel;
            _settings.LastScanFile = fileName;
            _settings.TotalScans++;
            
            _logger.LogInformation($"✅ Сбор завершен. Риск: {riskLevel}. Файл: {fileName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при сборе данных");
        }
    }
}