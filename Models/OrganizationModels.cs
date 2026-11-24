namespace AegisCore.Models;

// === МОДЕЛИ ОРГАНИЗАЦИИ ===

public class OrganizationData
{
    public string Id { get; set; } = "default";
    public string OrgType { get; set; } = "commercial";
    public string FullName { get; set; } = "";
    public string ShortName { get; set; } = "";
    public string Inn { get; set; } = "";
    public string? Kpp { get; set; }
    public string Ogrn { get; set; } = "";
    public string Address { get; set; } = "";
    public string? AddressIndex { get; set; }
    public string PostalAddress { get; set; } = "";
    public string? PostalIndex { get; set; }
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Director { get; set; }
    public bool IsCrypto { get; set; } = false;
    public DateTime? UpdatedAt { get; set; }
}

public class Department
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class Room
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Number { get; set; } = "";
    public string? Description { get; set; }
    public string? Department { get; set; }
    public DateTime? DateIn { get; set; }
    public DateTime? DateOut { get; set; }
}

public class Employee
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Fio { get; set; } = "";
    public string? Position { get; set; }
    public string? Department { get; set; }
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class AccessRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EmployeeId { get; set; } = "";
    public string EmployeeFio { get; set; } = "";
    
    // ✅ ДОБАВЛЕНЫ недостающие свойства
    public string Department { get; set; } = "";
    public string AccessLevel { get; set; } = "";
    public DateTime? IssueDate { get; set; }
    
    // Старые свойства (сохраняем для обратной совместимости)
    public bool PdnISPDn { get; set; }
    public bool PdnPaper { get; set; }
    public bool ConfInfo { get; set; }
    public bool SkziRoom { get; set; }
    public bool SkziWork { get; set; }
}

public class Branch
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string? Address { get; set; }
    public string? Director { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}