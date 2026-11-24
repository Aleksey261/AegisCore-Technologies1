namespace AegisCore.Models;

// ============================================================
// МОДЕЛИ ИНФРАСТРУКТУРЫ
// ============================================================

// ============================================================
// ИНФОРМАЦИОННЫЕ СИСТЕМЫ (п. 4.2.2.9 документации)
// ============================================================

public class InfoSystem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // === Основные сведения (Рисунок 68) ===
    public string Name { get; set; } = "";
    public string RegistrationNumber { get; set; } = "";
    public string Type { get; set; } = "";              // ИСПДн, ГИС, КИИ
    public string Role { get; set; } = "";              // Роль системы
    public string Purpose { get; set; } = "";           // Назначение
    public string Goals { get; set; } = "";             // Цели
    public string Tasks { get; set; } = "";             // Задачи
    public DateTime? CreatedAt { get; set; }
    
    // === Классификация ИС (Рисунок 69) ===
    public string SecurityClass { get; set; } = "";     // Класс защищённости (К1-К4)
    public string SecurityLevel { get; set; } = "";     // Уровень защищённости ПДн (УЗ1-УЗ4)
    public string SignificanceLevel { get; set; } = ""; // Уровень значимости информации
    public string Scale { get; set; } = "";             // Масштаб
    public string DataCategory { get; set; } = "";      // Категория ПДн
    public string ProcessingType { get; set; } = "";    // Вид обработки
    public int SubjectsCount { get; set; }              // Количество субъектов
    public string ThreatType { get; set; } = "";        // Тип угроз
    
    // === Аттестация (Рисунок 70) ===
    public string AttestationNumber { get; set; } = "";
    public DateTime? AttestationDate { get; set; }
    public DateTime? AttestationValidUntil { get; set; }
    public string AttestationBody { get; set; } = "";
    public string ComplianceStatus { get; set; } = "";
    
    // === БД и ЦОД (Рисунок 71) ===
    public List<DatabaseLocation> DatabaseLocations { get; set; } = new();
    
    // === Перечень ПО в ИС (Рисунок 73) ===
    public List<string> SoftwareIds { get; set; } = new();
    
    // === Структурные элементы (Рисунок 75) ===
    public List<StructuralElement> StructuralElements { get; set; } = new();
    
    // === Группы пользователей (Рисунок 77) ===
    public List<UserGroup> UserGroups { get; set; } = new();
    
    // === Схемы (Рисунок 79) ===
    public List<ObjectScheme> Schemes { get; set; } = new();
    
    // === Документы (Рисунок 81) ===
    public List<ObjectDocument> Documents { get; set; } = new();
}

// Местонахождение БД и ЦОД
public class DatabaseLocation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string InfoSystemId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string Type { get; set; } = "";              // own / rented
    public string OrganizationName { get; set; } = "";
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
}

// Структурный элемент ИС/АСУ ТП/ОИ
public class StructuralElement
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ParentId { get; set; } = "";          // ID ИС, АСУ ТП или ОИ
    public string ParentType { get; set; } = "";        // "info-system" / "asutp" / "object"
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Description { get; set; } = "";
    public string Connections { get; set; } = "";
}

// Группа пользователей ИС/АСУ ТП/ОИ
public class UserGroup
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ParentId { get; set; } = "";
    public string ParentType { get; set; } = "";
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";              // user / admin
    public List<string> EmployeeIds { get; set; } = new();
    public string AccessLevel { get; set; } = "";
}

// Схема работы системы
public class ObjectScheme
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ParentId { get; set; } = "";
    public string ParentType { get; set; } = "";
    public string Name { get; set; } = "";
    public string SchemeType { get; set; } = "";        // structural / flow
    public string? FilePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

// Документ объекта (ИС, АСУ ТП, ОИ)
public class ObjectDocument
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ParentId { get; set; } = "";
    public string ParentType { get; set; } = "";
    public string DocumentType { get; set; } = "";      // techpasport, order, classification_act, security_level_act, tz, tp, tech_process, access_system, threat_model, etc.
    public string Name { get; set; } = "";
    public string Status { get; set; } = "not_generated";   // not_generated / generated / uploaded
    public string? FilePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ApprovedAt { get; set; }
    public string? DocumentNumber { get; set; }         // Номер документа
    public bool IsMainDocument { get; set; } = false;   // true для формируемых документов (техпаспорт, приказ, акты), false для утверждаемых (ТЗ, ТП и т.д.)
    public string? GeneratedFileName { get; set; }      // Имя сформированного документа
    public string? ApprovedFileName { get; set; }       // Имя утверждённого документа
}

// ============================================================
// АСУ ТП (п. 4.2.2.10 документации)
// ============================================================

public class Asutp
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // === Основные сведения (Рисунок 85) ===
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Location { get; set; } = "";
    public string ProcessType { get; set; } = "";
    public string Description { get; set; } = "";
    public string Purpose { get; set; } = "";
    public string Goals { get; set; } = "";
    public string Tasks { get; set; } = "";
    public DateTime? CommissioningDate { get; set; }
    public DateTime? PurchaseDate { get; set; }      // Дата приобретения
    public DateTime? WriteOffDate { get; set; }      // Дата списания
    
    // === Классификация АСУ (Рисунок 86) ===
    public string SecurityClass { get; set; } = "";
    public string SignificanceLevel { get; set; } = "";
    public string Scale { get; set; } = "";
    
    // === Аттестация (Рисунок 87) ===
    public string AttestationNumber { get; set; } = "";
    public DateTime? AttestationDate { get; set; }
    public DateTime? AttestationValidUntil { get; set; }
    public string AttestationBody { get; set; } = "";
    public string ComplianceStatus { get; set; } = "";
    
    // === Перечень ПО в АСУ (Рисунок 88) ===
    public List<string> SoftwareIds { get; set; } = new();
    
    // === Структурные элементы (Рисунок 90) ===
    public List<StructuralElement> StructuralElements { get; set; } = new();
    
    // === Группы пользователей (Рисунок 92) ===
    public List<UserGroup> UserGroups { get; set; } = new();
    
    // === Схемы (Рисунок 94) ===
    public List<ObjectScheme> Schemes { get; set; } = new();
    
    // === Документы (Рисунок 96) ===
    public List<ObjectDocument> Documents { get; set; } = new();
}

// ============================================================
// ИТС (п. 4.2.2.11 документации)
// ============================================================

public class Its
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";              // LAN, WAN, VPN, etc.
    public string Description { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public DateTime? EntryDate { get; set; }            // Дата ввода в эксплуатацию
    public DateTime? WriteOffDate { get; set; }         // Дата списания
    public DateTime? CreatedAt { get; set; }
}

// ============================================================
// СЗИ (п. 4.2.2.1 документации)
// ============================================================

public class Szi
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public string ConformityMark { get; set; } = "";
    public string LicenseNumber { get; set; } = "";
    public int MaxInstallations { get; set; } = 1;
    public DateTime? ReceiptDate { get; set; }
    public DateTime? InstallByDate { get; set; }
    public string? ReceivedFromId { get; set; }
    public string? ReceivedFrom { get; set; }
    public string? Configuration { get; set; }
    public DateTime? DestructionDate { get; set; }
}

// ============================================================
// СКЗИ (п. 4.2.2.2 документации)
// ============================================================

public class Skzi
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public string CertificateNumber { get; set; } = "";
    public DateTime? IssueDate { get; set; }
    
    // Поля по документации (Рисунок 28)
    public string? CertificateValidity { get; set; }    // Срок действия сертификата
    public string? DistributionNumber { get; set; }     // Номер дистрибутива
    public string? CopyNumber { get; set; }             // Номер экземпляра
    public string? CoverLetter { get; set; }            // Сопроводительное письмо
    public string? Owner { get; set; }                  // ФИО/Организация
    public DateTime? ReceiptDate { get; set; }          // Дата получения
    public int LicenseCount { get; set; } = 1;          // Количество лицензий
    public string? Configuration { get; set; }          // Комплектация
    
    // Документы СКЗИ
    public List<SkziDocument> Documents { get; set; } = new();
}

public class SkziDocument
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SkziId { get; set; } = "";
    public string Name { get; set; } = "";
    public string DocumentType { get; set; } = "";
    public string? Number { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? FilePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

// ============================================================
// ХРАНИЛИЩА (п. 4.2.2.4 документации)
// ============================================================

public class Storage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string InventoryNumber { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public string Capacity { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime? PurchaseDate { get; set; }
    public int WarrantyYears { get; set; }
    public DateTime? WriteOffDate { get; set; }
}

// ============================================================
// ТЕХНИЧЕСКИЕ СРЕДСТВА (п. 4.2.2.3 документации)
// ============================================================

public class TechnicalAsset
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public string InventoryNumber { get; set; } = "";
    public DateTime? PurchaseDate { get; set; }
    public int WarrantyYears { get; set; }
    public DateTime? WriteOffDate { get; set; }
}

// ============================================================
// ЭЛЕКТРОННЫЕ ИДЕНТИФИКАТОРЫ (п. 4.2.2.5 документации)
// ============================================================

public class Identifier
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public string Owner { get; set; } = "";
    public DateTime? IssueDate { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public int WarrantyYears { get; set; }
    public DateTime? WriteOffDate { get; set; }
}

// ============================================================
// ЭЛЕКТРОННЫЕ ПОДПИСИ (п. 4.2.2.6 документации)
// ============================================================

public class Signature
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // Основные поля (Рисунок 52)
    public string KeyCarrier { get; set; } = "";           // Ключевой носитель
    public string KeyInfoNumber { get; set; } = "";        // Номер ключ. информации
    public string Skzi { get; set; } = "";                 // СКЗИ
    public string CopyNumber { get; set; } = "";           // Номер экземпляра
    public string CoverLetter { get; set; } = "";          // Сопроводительное письмо
    public string ReceivedFromType { get; set; } = "";     // Тип (от кого получено)
    public string Owner { get; set; } = "";                // ФИО/Организация
    public DateTime? ReceiptDate { get; set; }             // Дата получения
    public string OwnerFIO { get; set; } = "";             // ФИО владельца
    public string UsedFor { get; set; } = "";              // Используется для
    public DateTime? DestructionDate { get; set; }         // Дата удаления
    public string DestroyedByFIO { get; set; } = "";       // ФИО произведшего удаление
    
    // Документы
    public List<SkziDocument> Documents { get; set; } = new();
}

// ============================================================
// ✅ МАШИННЫЕ НОСИТЕЛИ (п. 4.2.2.7 документации) — ОБНОВЛЕНО
// ============================================================

public class Media
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string InventoryNumber { get; set; } = "";      // ✅ Регистрационный номер
    public string SerialNumber { get; set; } = "";
    public string Capacity { get; set; } = "";             // ✅ Емкость
    public string Description { get; set; } = "";          // ✅ Описание
    public DateTime? PurchaseDate { get; set; }
    public int WarrantyYears { get; set; }                 // ✅ Гарантия (лет)
    public DateTime? WriteOffDate { get; set; }            // ✅ Дата списания
}

// ============================================================
// ПРОГРАММНОЕ ОБЕСПЕЧЕНИЕ (п. 4.2.2.8 документации)
// ============================================================

public class Software
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public string Type { get; set; } = "";              // Тип ПО
    public string RegistrationNumber { get; set; } = ""; // ✅ Регистрационный номер
    public string Manufacturer { get; set; } = "";
    public string LicenseType { get; set; } = "";
    public string LicenseNumber { get; set; } = "";
    public DateTime? PurchaseDate { get; set; }          // ✅ Дата приобретения
    public DateTime? ValidUntil { get; set; }            // ✅ Действует до
    public DateTime? InstallDate { get; set; }
}