using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AegisCore.Models;
using System.Collections.Generic;

namespace AegisCore.Controllers.Audit;

[Authorize]
[Area("Audit")]
[Route("audit/protection")]
public class ProtectionController : Controller
{
    // ============================================================
    // ГЛАВНАЯ СТРАНИЦА РАЗДЕЛА
    // ============================================================
    [HttpGet("")]
    public IActionResult Index()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "protection";
        return View();
    }
    
    // ============================================================
    // 1. ПДн (Персональные данные) - 10 вкладок
    // ============================================================
    
    [HttpGet("personal-data")]
    public IActionResult PersonalData()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "protection";
        ViewBag.ActiveSubSection = "personal-data";
        return View("PersonalData/Index");
    }
    
    [HttpGet("personal-data/categories")]
    public IActionResult PersonalDataCategories()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "protection";
        ViewBag.ActiveSubSection = "personal-data";
        return View("PersonalData/Categories");
    }
    
    [HttpGet("personal-data/legal-basis")]
    public IActionResult PersonalDataLegalBasis()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "protection";
        ViewBag.ActiveSubSection = "personal-data";
        return View("PersonalData/LegalBasis");
    }
    
    [HttpGet("personal-data/purposes")]
    public IActionResult PersonalDataPurposes()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "protection";
        ViewBag.ActiveSubSection = "personal-data";
        return View("PersonalData/Purposes");
    }
    
    [HttpGet("personal-data/conditions")]
    public IActionResult PersonalDataConditions()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "protection";
        ViewBag.ActiveSubSection = "personal-data";
        return View("PersonalData/Conditions");
    }
    
    [HttpGet("personal-data/internal")]
    public IActionResult PersonalDataInternal()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "protection";
        ViewBag.ActiveSubSection = "personal-data";
        return View("PersonalData/Internal");
    }
    
    [HttpGet("personal-data/third-parties")]
    public IActionResult PersonalDataThirdParties()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "protection";
        ViewBag.ActiveSubSection = "personal-data";
        return View("PersonalData/ThirdParties");
    }
    
    [HttpGet("personal-data/non-automated")]
    public IActionResult PersonalDataNonAutomated()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "protection";
        ViewBag.ActiveSubSection = "personal-data";
        return View("PersonalData/NonAutomated");
    }
    
    [HttpGet("personal-data/automated")]
    public IActionResult PersonalDataAutomated()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "protection";
        ViewBag.ActiveSubSection = "personal-data";
        return View("PersonalData/Automated");
    }
    
    [HttpGet("personal-data/transboundary")]
    public IActionResult PersonalDataTransboundary()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "protection";
        ViewBag.ActiveSubSection = "personal-data";
        return View("PersonalData/Transboundary");
    }
    
    // ============================================================
    // 2. КИИ (Критическая информационная инфраструктура)
    // ============================================================
    
    [HttpGet("critical-infrastructure")]
    public IActionResult CriticalInfrastructure()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "protection";
        ViewBag.ActiveSubSection = "critical-infrastructure";
        return View("CriticalInfrastructure/Index");
    }

    [HttpGet("critical-infrastructure/business-processes")]
    public IActionResult CriticalInfrastructureBusinessProcesses()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "protection";
        ViewBag.ActiveSubSection = "critical-infrastructure";
        return View("CriticalInfrastructure/BusinessProcesses");
    }
    
    [HttpGet("critical-infrastructure/systems")]
    public IActionResult CriticalInfrastructureSystems(string? search, string? sort, string? order, int page = 1, int pageSize = 10)
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "protection";
        ViewBag.ActiveSubSection = "critical-infrastructure";

        var model = new CriticalInfrastructureViewModel
        {
            Search = search ?? "",
            Sort = sort ?? "name",
            SortOrder = order ?? "asc",
            CurrentPage = page,
            PageSize = pageSize,
            Objects = new List<CiiObject>(),
            TotalPages = 1,
            TotalItems = 0
        };

        return View("CriticalInfrastructure/Systems", model);
    }

    [HttpGet("critical-infrastructure/systems/create")]
    public IActionResult CreateSystem()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "protection";
        ViewBag.ActiveSubSection = "critical-infrastructure";
        return View("CriticalInfrastructure/Create");
    }

    [HttpGet("critical-infrastructure/systems/{id}")]
    public IActionResult SystemDetails(int id)
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "protection";
        ViewBag.ActiveSubSection = "critical-infrastructure";
        ViewBag.ObjectId = id;
        return View("CriticalInfrastructure/Details");
    }
    
    // ============================================================
    // 3. Прочая КИ (Коммерческая и служебная тайна)
    // ============================================================
    
    [HttpGet("other-confidential")]
    public IActionResult OtherConfidential()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "protection";
        ViewBag.ActiveSubSection = "other-confidential";
        
        var employees = GetEmployees();
        ViewBag.Employees = employees;
        
        return View("OtherConfidential/Index");
    }
    
    [HttpGet("other-confidential/access")]
    public IActionResult OtherConfidentialAccess()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "protection";
        ViewBag.ActiveSubSection = "other-confidential";
        
        var employees = GetEmployees();
        ViewBag.Employees = employees;
        
        return View("OtherConfidential/Access");
    }
    
    [HttpGet("other-confidential/rooms")]
    public IActionResult OtherConfidentialRooms()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "protection";
        ViewBag.ActiveSubSection = "other-confidential";
        
        var employees = GetEmployees();
        ViewBag.Employees = employees;
        
        return View("OtherConfidential/Rooms");
    }
    
    [HttpGet("other-confidential/service")]
    public IActionResult OtherConfidentialService()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "protection";
        ViewBag.ActiveSubSection = "other-confidential";
        return View("OtherConfidential/Service");
    }
    
    // ============================================================
    // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
    // ============================================================
    
    private List<dynamic> GetEmployees()
    {
        // TODO: Заменить на реальный запрос к БД
        // var employees = _context.Employees.Select(e => new { e.Id, e.Fio, e.Position, e.Department, e.HasAccess }).ToList();
        
        return new List<dynamic>
        {
            new { Id = 1, Fio = "Иванов Иван Иванович", Position = "Начальник отдела", Department = "Отдел безопасности", HasAccess = true },
            new { Id = 2, Fio = "Петров Петр Петрович", Position = "Ведущий специалист", Department = "IT-отдел", HasAccess = false },
            new { Id = 3, Fio = "Сидорова Анна Сергеевна", Position = "Специалист", Department = "Кадровый отдел", HasAccess = true }
        };
    }
}