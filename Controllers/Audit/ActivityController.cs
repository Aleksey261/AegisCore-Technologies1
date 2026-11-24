using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AegisCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AegisCore.Controllers.Audit;

[Authorize]
[Area("Audit")]
[Route("audit/activity")]
public class ActivityController : Controller
{
    private static readonly List<Dictionary<string, string>> ActivitySections = new()
    {
        new() { { "id", "responsible" }, { "title", "Ответственные лица" }, { "icon", "fas fa-user-shield" }, { "bg", "#eef2ff" }, { "color", "#4f46e5" }, { "desc", "Назначение ответственных лиц не влечет за собой изменения безопасности информации" } },
        new() { { "id", "commissions" }, { "title", "Комиссии" }, { "icon", "fas fa-users" }, { "bg", "#fef3c7" }, { "color", "#d97706" }, { "desc", "Учет проведенных мероприятий по вопросам обеспечения безопасности информации" } },
        new() { { "id", "documents" }, { "title", "Документы" }, { "icon", "fas fa-file-alt" }, { "bg", "#fce4ec" }, { "color", "#dc2626" }, { "desc", "Автоматическое формирование необходимых документов по вопросам обеспечения безопасности информации" } },
        new() { { "id", "warranty" }, { "title", "Гарантия" }, { "icon", "fas fa-shield-alt" }, { "bg", "#e0f2fe" }, { "color", "#0284c7" }, { "desc", "Автоматическое формирование сведений об организации гарантийных сроков на ТС, ЗВ, ИНН, сроки технической поддержки на СЗИ и СКЗИ, а также о внесении доп. изменений" } },
        new() { { "id", "notifications" }, { "title", "Уведомления" }, { "icon", "fas fa-bell" }, { "bg", "#f3e8ff" }, { "color", "#7c3aed" }, { "desc", "Автоматическое формирование информационных писем и сведений в системе операторов, осуществляющих обработку ПДн" } },
        new() { { "id", "situations" }, { "title", "Ситуации" }, { "icon", "fas fa-exclamation-triangle" }, { "bg", "#fef2f2" }, { "color", "#dc2626" }, { "desc", "Мастер действий по обеспечению безопасности информации" } },
        new() { { "id", "monitoring" }, { "title", "Мониторинг филиалов" }, { "icon", "fas fa-sitemap" }, { "bg", "#ecfdf5" }, { "color", "#059669" }, { "desc", "Мониторинг деятельности филиалов в области информационной безопасности" } }
    };

    // ============================================================
    // ГЛАВНАЯ СТРАНИЦА РАЗДЕЛА
    // ============================================================
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "activity";
        return View("~/Areas/Audit/Views/Activity/Activity.cshtml", ActivitySections);
    }

    // ============================================================
    // 1. ОТВЕТСТВЕННЫЕ ЛИЦА
    // ============================================================
    [HttpGet("responsible")]
    public IActionResult Responsible(string? search, int page = 1, int size = 10)
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "activity";
        ViewBag.ActiveSubSection = "responsible";
        ViewBag.Search = search;
        ViewBag.CurrentPage = page;
        ViewBag.PageSize = size;
        
        var employees = GetEmployees();
        ViewBag.Employees = employees;
        
        var items = FilterResponsible(GetMockResponsible(), search);
        ViewBag.TotalPages = (int)Math.Ceiling((double)items.Count / size);
        ViewBag.TotalItems = items.Count;
        
        return View("~/Areas/Audit/Views/Activity/Responsible.cshtml", Paginate(items, page, size));
    }

    // ============================================================
    // 2. КОМИССИИ
    // ============================================================
    [HttpGet("commissions")]
    public IActionResult Commissions(string? search, int page = 1, int size = 10)
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "activity";
        ViewBag.ActiveSubSection = "commissions";
        ViewBag.Search = search;
        ViewBag.CurrentPage = page;
        ViewBag.PageSize = size;
        
        var employees = GetEmployees();
        ViewBag.Employees = employees;
        
        var items = FilterCommissions(GetMockCommissions(), search);
        ViewBag.TotalPages = (int)Math.Ceiling((double)items.Count / size);
        ViewBag.TotalItems = items.Count;
        
        return View("~/Areas/Audit/Views/Activity/Commissions.cshtml", Paginate(items, page, size));
    }

    // ============================================================
    // 3. ДОКУМЕНТЫ
    // ============================================================
    [HttpGet("documents")]
    public IActionResult Documents(string? search, int page = 1, int size = 10)
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "activity";
        ViewBag.ActiveSubSection = "documents";
        ViewBag.Search = search;
        ViewBag.CurrentPage = page;
        ViewBag.PageSize = size;
        
        var docs = FilterDocuments(GetMockDocuments(), search);
        ViewBag.Stats = GetDocumentStats(docs);
        ViewBag.TotalPages = (int)Math.Ceiling((double)docs.Count / size);
        ViewBag.TotalItems = docs.Count;
        
        return View("~/Areas/Audit/Views/Activity/Documents.cshtml", Paginate(docs, page, size));
    }

    // ============================================================
    // 4. ГАРАНТИЯ
    // ============================================================
    [HttpGet("warranty")]
    public IActionResult Warranty(string? search, int page = 1, int size = 10)
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "activity";
        ViewBag.ActiveSubSection = "warranty";
        ViewBag.Search = search;
        ViewBag.CurrentPage = page;
        ViewBag.PageSize = size;
        
        var items = FilterWarranty(GetMockWarranty(), search);
        ViewBag.TotalPages = (int)Math.Ceiling((double)items.Count / size);
        ViewBag.TotalItems = items.Count;
        
        return View("~/Areas/Audit/Views/Activity/Warranty.cshtml", Paginate(items, page, size));
    }

    // ============================================================
    // 5. УВЕДОМЛЕНИЯ
    // ============================================================
    [HttpGet("notifications")]
    public IActionResult Notifications(string? tab, string? registryNumber, string? search, int page = 1, int size = 10)
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "activity";
        ViewBag.ActiveSubSection = "notifications";
        ViewBag.ActiveSubtab = tab ?? "main";
        ViewBag.RegistryNumber = registryNumber;
        ViewBag.HasRegistry = !string.IsNullOrEmpty(registryNumber);
        ViewBag.Search = search;
        ViewBag.CurrentPage = page;
        ViewBag.PageSize = size;
        
        var items = FilterNotifications(GetMockNotifications(), search);
        ViewBag.TotalPages = (int)Math.Ceiling((double)items.Count / size);
        ViewBag.TotalItems = items.Count;
        
        return View("~/Areas/Audit/Views/Activity/Notifications.cshtml", Paginate(items, page, size));
    }

    // ============================================================
    // 6. СИТУАЦИИ
    // ============================================================
    [HttpGet("situations")]
    public IActionResult Situations(string? view, string? search)
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "activity";
        ViewBag.ActiveSubSection = "situations";
        ViewBag.ViewMode = view ?? "grid";
        ViewBag.Search = search;
        ViewBag.Situations = GetMockSituations();
        ViewBag.History = GetMockSituationHistory();
        return View("~/Areas/Audit/Views/Activity/Situations.cshtml");
    }

    // ============================================================
    // 7. МОНИТОРИНГ ФИЛИАЛОВ
    // ============================================================
    [HttpGet("monitoring")]
    public IActionResult Monitoring()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "activity";
        ViewBag.ActiveSubSection = "monitoring";
        
        // Тестовые данные для демонстрации интерфейса
        var model = new BranchMonitoringViewModel
        {
            Branches = new List<BranchInfo>
            {
                new BranchInfo { 
                    Id = "1", 
                    Name = "Филиал в г. Москва", 
                    Address = "г. Москва, ул. Примерная, д. 1", 
                    IsActive = true, 
                    EmployeesCount = 15, 
                    PremisesCount = 8, 
                    ReadinessPercent = 92, 
                    IncidentsCount = 0, 
                    LastInspectionDate = new DateTime(2023, 10, 1) 
                },
                new BranchInfo { 
                    Id = "2", 
                    Name = "Филиал в г. Санкт-Петербург", 
                    Address = "г. Санкт-Петербург, Невский пр., д. 10", 
                    IsActive = true, 
                    EmployeesCount = 10, 
                    PremisesCount = 5, 
                    ReadinessPercent = 78, 
                    IncidentsCount = 1, 
                    LastInspectionDate = new DateTime(2023, 9, 28) 
                },
                new BranchInfo { 
                    Id = "3", 
                    Name = "Филиал в г. Екатеринбург", 
                    Address = "г. Екатеринбург, ул. Ленина, д. 50", 
                    IsActive = false, 
                    EmployeesCount = 8, 
                    PremisesCount = 4, 
                    ReadinessPercent = 45, 
                    IncidentsCount = 0, 
                    LastInspectionDate = new DateTime(2023, 9, 15) 
                }
            }
        };

        return View("~/Areas/Audit/Views/Activity/Monitoring.cshtml", model);
    }

    // ============================================================
    // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
    // ============================================================
    
    private List<T> Paginate<T>(List<T> list, int page, int size)
    {
        if (list == null || list.Count == 0) return new List<T>();
        int start = (page - 1) * size;
        int end = Math.Min(start + size, list.Count);
        return start < end ? list.GetRange(start, end - start) : new List<T>();
    }

    private List<dynamic> GetEmployees()
    {
        return new List<dynamic>
        {
            new { Id = 1, Fio = "Иванов Иван Иванович", Position = "Начальник отдела", Department = "Отдел безопасности" },
            new { Id = 2, Fio = "Петров Петр Петрович", Position = "Ведущий специалист", Department = "IT-отдел" },
            new { Id = 3, Fio = "Сидорова Анна Сергеевна", Position = "Специалист", Department = "Кадровый отдел" },
            new { Id = 4, Fio = "Козлов Дмитрий Алексеевич", Position = "Инженер", Department = "Отдел безопасности" },
            new { Id = 5, Fio = "Смирнова Елена Викторовна", Position = "Главный специалист", Department = "Юридический отдел" }
        };
    }

    // ============================================================
    // МОК-ДАННЫЕ
    // ============================================================
    private List<Dictionary<string, string>> GetMockResponsible() => new()
    {
        new() { { "id", "1" }, { "responsibilityType", "Ответственный за ввод в эксплуатацию ОИН" }, { "employee", "Иванов Иван Иванович" }, { "appointmentDate", "15.06.2022" }, { "position", "Начальник отдела" } },
        new() { { "id", "2" }, { "responsibilityType", "Ответственный за организацию обработки ПДн" }, { "employee", "Сидорова Анна Сергеевна" }, { "appointmentDate", "01.02.2023" }, { "position", "Специалист" } },
        new() { { "id", "3" }, { "responsibilityType", "Ответственный за обработку ПДн" }, { "employee", "Петров Петр Петрович" }, { "appointmentDate", "10.01.2023" }, { "position", "Ведущий специалист" } },
        new() { { "id", "4" }, { "responsibilityType", "Ответственный за организацию работ по технической защите конфиденциальной информации" }, { "employee", "Козлов Дмитрий Алексеевич" }, { "appointmentDate", "20.03.2023" }, { "position", "Инженер" } }
    };

    private List<Dictionary<string, string>> FilterResponsible(List<Dictionary<string, string>> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(i => 
            i["employee"].ToLower().Contains(s) || 
            i["responsibilityType"].ToLower().Contains(s) ||
            i["position"].ToLower().Contains(s)
        ).ToList();
    }

    private List<Dictionary<string, string>> GetMockCommissions() => new()
    {
        new() { { "id", "1" }, { "name", "Комиссия по аттестации объектов информатизации" }, { "date", "15.06.2023" }, { "status", "Проведена" }, { "members", "Иванов И.И., Петров П.П." } },
        new() { { "id", "2" }, { "name", "Комиссия по расследованию инцидентов" }, { "date", "10.07.2023" }, { "status", "Запланирована" }, { "members", "Сидорова А.С., Козлов Д.А." } }
    };

    private List<Dictionary<string, string>> FilterCommissions(List<Dictionary<string, string>> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(i => 
            i["name"].ToLower().Contains(s) || 
            i["status"].ToLower().Contains(s) ||
            i["members"].ToLower().Contains(s)
        ).ToList();
    }

    private List<Dictionary<string, string>> GetMockDocuments() => new()
    {
        new() { { "id", "1" }, { "name", "Инструкция ответственного за техническую защиту ПДн" }, { "status", "approved" }, { "date", "14.09.2023" }, { "version", "1.0" } },
        new() { { "id", "2" }, { "name", "Инструкция пользователя СКЗИ" }, { "status", "approved" }, { "date", "14.09.2023" }, { "version", "2.1" } },
        new() { { "id", "3" }, { "name", "Политика обработки персональных данных" }, { "status", "needs_approval" }, { "date", "20.10.2023" }, { "version", "3.0" } },
        new() { { "id", "4" }, { "name", "Регламент реагирования на инциденты" }, { "status", "modified" }, { "date", "05.11.2023" }, { "version", "1.2" } }
    };

    private List<Dictionary<string, string>> FilterDocuments(List<Dictionary<string, string>> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(i => 
            i["name"].ToLower().Contains(s) || 
            i["status"].ToLower().Contains(s)
        ).ToList();
    }

    private Dictionary<string, long> GetDocumentStats(List<Dictionary<string, string>> docs)
    {
        var statuses = new[] { "approved", "needs_approval", "modified", "error", "not_generated" };
        var result = new Dictionary<string, long>();
        foreach (var status in statuses)
        {
            result[status] = docs.Count(d => d["status"] == status);
        }
        return result;
    }

    private List<Dictionary<string, string>> GetMockWarranty() => new()
    {
        new() { { "id", "1" }, { "name", "Антивирус Касперского" }, { "type", "СЗИ" }, { "startDate", "23.02.2023" }, { "endDate", "23.02.2024" }, { "status", "Активна" } },
        new() { { "id", "2" }, { "name", "КриптоПро CSP 5.0" }, { "type", "СКЗИ" }, { "startDate", "03.11.2022" }, { "endDate", "03.11.2024" }, { "status", "Активна" } },
        new() { { "id", "3" }, { "name", "Сервер HP ProLiant DL380" }, { "type", "ТС" }, { "startDate", "15.01.2021" }, { "endDate", "15.01.2023" }, { "status", "Истекла" } }
    };

    private List<Dictionary<string, string>> FilterWarranty(List<Dictionary<string, string>> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(i => 
            i["name"].ToLower().Contains(s) || 
            i["type"].ToLower().Contains(s) ||
            i["status"].ToLower().Contains(s)
        ).ToList();
    }

    private List<Dictionary<string, string>> GetMockNotifications() => new()
    {
        new() { { "id", "1" }, { "type", "Уведомление об обработке ПДн" }, { "sendDate", "30.04.2023" }, { "status", "Отправлено" }, { "recipient", "Роскомнадзор" } },
        new() { { "id", "2" }, { "type", "Информационное письмо о внесении изменений" }, { "sendDate", "15.05.2023" }, { "status", "Создано" }, { "recipient", "Оператор ПДн" } },
        new() { { "id", "3" }, { "type", "Уведомление о начале обработки ПДн" }, { "sendDate", "01.06.2023" }, { "status", "Отправлено" }, { "recipient", "Роскомнадзор" } }
    };

    private List<Dictionary<string, string>> FilterNotifications(List<Dictionary<string, string>> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(i => 
            i["type"].ToLower().Contains(s) || 
            i["status"].ToLower().Contains(s) ||
            i["recipient"].ToLower().Contains(s)
        ).ToList();
    }

    private List<Dictionary<string, string>> GetMockSituations() => new()
    {
        new() { { "id", "1" }, { "title", "Обновление СЗИ" }, { "icon", "💡" }, { "description", "Обновление средств защиты информации" }, { "steps", "3" } },
        new() { { "id", "2" }, { "title", "Ввод нового помещения в эксплуатацию" }, { "icon", "💡" }, { "description", "Добавление нового помещения для обработки ПДн" }, { "steps", "5" } },
        new() { { "id", "3" }, { "title", "Назначение ответственного лица" }, { "icon", "💡" }, { "description", "Назначение лица, ответственного за обработку ПДн" }, { "steps", "4" } },
        new() { { "id", "4" }, { "title", "Проведение внутренней проверки" }, { "icon", "📋" }, { "description", "Проведение внутренней проверки по ИБ" }, { "steps", "6" } }
    };

    private List<Dictionary<string, string>> GetMockSituationHistory() => new()
    {
        new() { { "id", "1" }, { "name", "Обновление СЗИ" }, { "object", "Антивирус Касперского" }, { "date", "15.09.2023" }, { "status", "Завершено" }, { "executor", "Петров П.П." } },
        new() { { "id", "2" }, { "name", "Ввод нового помещения" }, { "object", "Кабинет 305" }, { "date", "10.08.2023" }, { "status", "В процессе" }, { "executor", "Иванов И.И." } }
    };

    private List<Dictionary<string, string>> GetMockBranches() => new()
    {
        new() { { "id", "1" }, { "name", "Филиал в г. Москва" }, { "status", "Активен" }, { "lastCheck", "01.10.2023" }, { "employees", "15" }, { "incidents", "0" } },
        new() { { "id", "2" }, { "name", "Филиал в г. Санкт-Петербург" }, { "status", "Активен" }, { "lastCheck", "28.09.2023" }, { "employees", "10" }, { "incidents", "1" } },
        new() { { "id", "3" }, { "name", "Филиал в г. Екатеринбург" }, { "status", "Неактивен" }, { "lastCheck", "15.09.2023" }, { "employees", "8" }, { "incidents", "0" } }
    };

    private List<Dictionary<string, string>> FilterBranches(List<Dictionary<string, string>> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(i => 
            i["name"].ToLower().Contains(s) || 
            i["status"].ToLower().Contains(s)
        ).ToList();
    }
}