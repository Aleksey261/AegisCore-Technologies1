using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AegisCore.Services;

public class LocalScanAdapter
{
    public List<SecurityData> ConvertToSecurityData(SystemInfo info)
    {
        var dataList = new List<SecurityData>();
        
        // Признаки остаются float (1f или 0f)
        float portOpen = info.OpenPorts.Any(p => p.Port == 21 || p.Port == 23 || p.Port == 135 || p.Port == 445 || p.Port == 3389) ? 1f : 0f;
        float weakPassword = info.OpenPorts.Any(p => p.Port == 21 || p.Port == 23 || p.Port == 3389) ? 1f : 0f;
        float outdated = info.Vulnerabilities.Any(v => v.Type == "Устаревшее ПО" && v.Severity == "High") ? 1f : 0f;
        float unencrypted = info.OpenPorts.Any(p => p.Port == 21 || p.Port == 23 || p.Port == 80) ? 1f : 0f;
        
        var mainRecord = new SecurityData
        {
            PortOpen = portOpen,
            WeakPassword = weakPassword,
            OutdatedSoftware = outdated,
            UnencryptedTraffic = unencrypted,
            Label = CalculateOverallRisk(info) // <-- ИСПРАВЛЕНО: метод теперь возвращает bool
        };
        dataList.Add(mainRecord);
        
        foreach (var vuln in info.Vulnerabilities)
        {
            var record = new SecurityData
            {
                PortOpen = vuln.Type.Contains("порт", StringComparison.OrdinalIgnoreCase) ? 1f : 0f,
                WeakPassword = vuln.Type.Contains("аутентификация", StringComparison.OrdinalIgnoreCase) ? 1f : 0f,
                OutdatedSoftware = vuln.Type.Contains("устаревшее", StringComparison.OrdinalIgnoreCase) ? 1f : 0f,
                UnencryptedTraffic = vuln.Type.Contains("шифрование", StringComparison.OrdinalIgnoreCase) ? 1f : 0f,
                Label = vuln.Severity == "Critical" || vuln.Severity == "High" // <-- ИСПРАВЛЕНО: чистое bool выражение, без ? 1f : 0f
            };
            dataList.Add(record);
        }
        
        foreach (var port in info.OpenPorts)
        {
            var record = new SecurityData
            {
                PortOpen = 1f,
                WeakPassword = (port.Port == 21 || port.Port == 23 || port.Port == 3389) ? 1f : 0f,
                OutdatedSoftware = 0f,
                UnencryptedTraffic = (port.Port == 21 || port.Port == 23 || port.Port == 80) ? 1f : 0f,
                Label = port.Port == 23 || port.Port == 445 // <-- ИСПРАВЛЕНО: чистое bool выражение, без ? 1f : 0f
            };
            dataList.Add(record);
        }
        
        foreach (var ni in info.NetworkInterfaces)
        {
            dataList.Add(new SecurityData 
            { 
                PortOpen = 0f, 
                WeakPassword = 0f, 
                OutdatedSoftware = 0f, 
                UnencryptedTraffic = 0f, 
                Label = false // <-- ИСПРАВЛЕНО: false вместо 0f
            });
        }
        
        foreach (var proc in info.RunningProcesses.Take(50))
        {
            dataList.Add(new SecurityData 
            { 
                PortOpen = 0f, 
                WeakPassword = 0f, 
                OutdatedSoftware = 0f, 
                UnencryptedTraffic = 0f, 
                Label = false // <-- ИСПРАВЛЕНО: false вместо 0f
            });
        }
        
        return dataList;
    }

    // ИСПРАВЛЕНО: метод теперь возвращает bool, а не float
    private bool CalculateOverallRisk(SystemInfo info)
    {
        int riskScore = 0;
        if (!info.FirewallEnabled) riskScore += 3;
        if (!info.AntivirusPresent) riskScore += 3;
        if (!info.WindowsUpdateEnabled) riskScore += 2;
        riskScore += info.OpenPorts.Count(p => p.Port == 23 || p.Port == 445) * 2;
        riskScore += info.OpenPorts.Count(p => p.Port == 21 || p.Port == 3389);
        riskScore += info.Vulnerabilities.Count(v => v.Severity == "Critical");
        riskScore += info.Vulnerabilities.Count(v => v.Severity == "High") / 2;
        
        return riskScore >= 4; // Возвращаем true или false напрямую
    }

    public string GenerateReport(SystemInfo info)
    {
        var report = new StringBuilder();
        report.AppendLine($"=== ОТЧЕТ О СОСТОЯНИИ СИСТЕМЫ: {info.ComputerName} ===");
        report.AppendLine($"ОС: {info.OSVersion}");
        report.AppendLine($"CPU: {info.CPUName}");
        report.AppendLine($"RAM: {info.TotalRAM_GB} ГБ");
        report.AppendLine($"Открытых портов: {info.OpenPorts.Count}");
        report.AppendLine($"Уязвимостей: {info.Vulnerabilities.Count}");
        report.AppendLine($"Брандмауэр: {(info.FirewallEnabled ? "Включен" : "Отключен")}");
        report.AppendLine($"Антивирус: {(info.AntivirusPresent ? info.AntivirusName : "Не установлен")}");
        return report.ToString();
    }
}