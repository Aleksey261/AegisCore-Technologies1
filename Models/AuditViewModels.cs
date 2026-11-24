namespace AegisCore.Models;

public class AuditIndexModel
{
    public AuditDashboardStats Stats { get; set; } = new(0, 0, 0, 0, 0);
    public List<AuditActivity> Activities { get; set; } = new();
}

public class OrganizationViewModel
{
    public string ActiveTab { get; set; } = "general";
    public string Search { get; set; } = "";
    public string? Sort { get; set; }
    public string? SortOrder { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 0;
    public OrganizationData Organization { get; set; } = new();
    public List<Department> Departments { get; set; } = new();
    public List<Room> Rooms { get; set; } = new();
    public List<Employee> Employees { get; set; } = new();
    public List<AccessRecord> Accesses { get; set; } = new();
    public List<Branch> Branches { get; set; } = new();
}

public class InfrastructureViewModel
{
    public string ActiveTab { get; set; } = "info-systems";
    public string Search { get; set; } = "";
    public string? Sort { get; set; }
    public string? SortOrder { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 0;
    
    public List<InfoSystem> InfoSystems { get; set; } = new();
    public List<Storage> Storages { get; set; } = new();
    public List<TechnicalAsset> TechnicalAssets { get; set; } = new();
    public List<Szi> Szis { get; set; } = new();
    public List<Skzi> Skzis { get; set; } = new();
    public List<Identifier> Identifiers { get; set; } = new();
    public List<Signature> Signatures { get; set; } = new();
    public List<Media> Media { get; set; } = new();
    public List<Software> Software { get; set; } = new();
    public List<Asutp> Asutps { get; set; } = new();
    public List<Its> Its { get; set; } = new();

    public List<OrganizationReference> Organizations { get; set; } = new();
    public List<string> Configurations { get; set; } = new();
    public List<string> CoverLetters { get; set; } = new();
    public List<string> SkziNames { get; set; } = new();
    public List<string> TechnicalAssetTypes { get; set; } = new();
    public List<string> StorageTypes { get; set; } = new();
    public List<string> IdentifierTypes { get; set; } = new();
    public List<string> MediaTypes { get; set; } = new();
    public List<string> SoftwareTypes { get; set; } = new();
    public List<string> KeyInfoNumbers { get; set; } = new();
    public List<string> OSIList { get; set; } = new();
}

public class OrganizationReference
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string? FullName { get; set; }
    public string? Address { get; set; }
    public string? Inn { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class CalendarEventForm
{
    public string? Id { get; set; }
    public string Title { get; set; } = "";
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public bool Completed { get; set; }
}

public class DirectoryItemForm
{
    public string? Id { get; set; }
    public string Type { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Number { get; set; }
    public string? KeyNumber { get; set; }
    public string? IssueDate { get; set; }
    public string? ValidUntil { get; set; }
    public string? Manufacturer { get; set; }
    public string? Inn { get; set; }
    public string? Address { get; set; }
    public string? Date { get; set; }
    public string? Description { get; set; }
}

public class DepartmentDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
}

public class RoomDto
{
    public string? Id { get; set; }
    public string Number { get; set; } = "";
    public string? Description { get; set; }
    public string Department { get; set; } = "";
    public DateTime? DateIn { get; set; }
    public DateTime? DateOut { get; set; }
}

public class ReferenceDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
}

public class EmployeeDto
{
    public string? Id { get; set; }
    public string Fio { get; set; } = "";
    public string Position { get; set; } = "";
    public string Department { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
}

public class InfoSystemDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Role { get; set; } = "";
    public string Purpose { get; set; } = "";
    public string Goals { get; set; } = "";
    public string Tasks { get; set; } = "";
    public string SecurityClass { get; set; } = "";
    public string SecurityLevel { get; set; } = "";
    public string SignificanceLevel { get; set; } = "";
    public string Scale { get; set; } = "";
    public string DataCategory { get; set; } = "";
    public string ProcessingType { get; set; } = "";
    public int SubjectsCount { get; set; }
    public string ThreatType { get; set; } = "";
    public string AttestationNumber { get; set; } = "";
    public string? AttestationDate { get; set; }
    public string? AttestationValidUntil { get; set; }
    public string AttestationBody { get; set; } = "";
    public string ComplianceStatus { get; set; } = "";
}

public class AsutpDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Location { get; set; } = "";
    public string ProcessType { get; set; } = "";
    public string Description { get; set; } = "";
    public string Purpose { get; set; } = "";
    public string Goals { get; set; } = "";
    public string Tasks { get; set; } = "";
    public string? CommissioningDate { get; set; }
    public string SecurityClass { get; set; } = "";
    public string SignificanceLevel { get; set; } = "";
    public string Scale { get; set; } = "";
    public string AttestationNumber { get; set; } = "";
    public string? AttestationDate { get; set; }
    public string? AttestationValidUntil { get; set; }
    public string AttestationBody { get; set; } = "";
    public string ComplianceStatus { get; set; } = "";
}

public class SziDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public string ConformityMark { get; set; } = "";
    public string LicenseNumber { get; set; } = "";
    public int MaxInstallations { get; set; }
    public DateTime? ReceiptDate { get; set; }
    public DateTime? InstallByDate { get; set; }
    public string? ReceivedFromId { get; set; }
    public string? ReceivedFrom { get; set; }
    public string? Configuration { get; set; }
    public DateTime? DestructionDate { get; set; }
}

public class SkziDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public string CertificateNumber { get; set; } = "";
    public DateTime? IssueDate { get; set; }
    public string? CertificateValidity { get; set; }
    public string? DistributionNumber { get; set; }
    public string? CopyNumber { get; set; }
    public string? CoverLetter { get; set; }
    public string? Owner { get; set; }
    public DateTime? ReceiptDate { get; set; }
    public DateTime? ValidUntil { get; set; }
    public int LicenseCount { get; set; }
    public string? Configuration { get; set; }
}

public class SziVendorDto
{
    public string? Id { get; set; }
    public string ShortName { get; set; } = "";
    public string? FullName { get; set; }
    public string? Address { get; set; }
    public string? Inn { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class BranchDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string? Director { get; set; }
}

public class AccessDto
{
    public string? Id { get; set; }
    public string EmployeeId { get; set; } = "";
    public string EmployeeFio { get; set; } = "";
    public bool PdnISPDn { get; set; }
    public bool PdnPaper { get; set; }
    public bool ConfInfo { get; set; }
    public bool SkziRoom { get; set; }
    public bool SkziWork { get; set; }
}

public class SoftwareDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public string Type { get; set; } = "";
    public string LicenseType { get; set; } = "";
    public string LicenseNumber { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public DateTime? InstallDate { get; set; }
}

public class DatabaseLocationDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string Type { get; set; } = "";
    public string OrganizationName { get; set; } = "";
}

public class StructuralElementDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Description { get; set; } = "";
    public string Connections { get; set; } = "";
}

public class UserGroupDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";
    public List<string> EmployeeIds { get; set; } = new();
    public string AccessLevel { get; set; } = "";
}

public class ObjectSchemeDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public string SchemeType { get; set; } = "";
    public string? FilePath { get; set; }
}

public class ObjectDocumentDto
{
    public string? Id { get; set; }
    public string DocumentType { get; set; } = "";
    public string Name { get; set; } = "";
    public string Status { get; set; } = "draft";
    public string? FilePath { get; set; }
}

public class TechnicalAssetDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string InventoryNumber { get; set; } = "";
    public string? SerialNumber { get; set; }
    public string? PurchaseDate { get; set; }
    public string? WriteOffDate { get; set; }
    public string? Location { get; set; }
}

public class StorageDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string? InventoryNumber { get; set; }
    public string? PurchaseDate { get; set; }
    public string? WriteOffDate { get; set; }
    public int WarrantyYears { get; set; }
    public string? Location { get; set; }
}

public class IdentifierDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string? SerialNumber { get; set; }
    public string? PurchaseDate { get; set; }
    public string? WriteOffDate { get; set; }
}

public class SignatureDto
{
    public string? Id { get; set; }
    public string KeyCarrier { get; set; } = "";
    public string? KeyInfoNumber { get; set; }
    public string? OwnerFIO { get; set; }
    public string? ReceiptDate { get; set; }
    public string? DestructionDate { get; set; }
    public string? Note { get; set; }
}

public class MediaDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string? InventoryNumber { get; set; }
    public string? PurchaseDate { get; set; }
    public string? WriteOffDate { get; set; }
    public int? Capacity { get; set; }
}

public class ItsDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public string? Type { get; set; }
    public string? IpAddress { get; set; }
    public string? Description { get; set; }
}

public class OrganizationReferenceDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public string? FullName { get; set; }
    public string? Address { get; set; }
    public string? Inn { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class InformatizationObjectDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = "";
    public string InventoryNumber { get; set; } = "";
    public string Location { get; set; } = "";
    public string Classification { get; set; } = "";
    public string SecurityLevel { get; set; } = "";
    public string ComplianceStatus { get; set; } = "";
    public string Purpose { get; set; } = "";
    public string Tasks { get; set; } = "";
    public string AttestationNumber { get; set; } = "";
    public string? AttestationDate { get; set; }
    public string? AttestationValidUntil { get; set; }
    public string AttestationBody { get; set; } = "";
    public string DatabaseLocation { get; set; } = "";
    public string DataCenterAddress { get; set; } = "";
    public string DataCenterType { get; set; } = "";
    public List<string> TechnicalAssetIds { get; set; } = new();
    public List<string> SoftwareIds { get; set; } = new();
    public List<string> SziIds { get; set; } = new();
    public List<string> SkziIds { get; set; } = new();
    public List<string> TelecomIds { get; set; } = new();
    public List<string> InfoSystemIds { get; set; } = new();
}

public class ChangeHistoryRecordDto
{
    public string? Id { get; set; }
    public string ChangeType { get; set; } = "";
    public string Description { get; set; } = "";
    public string Reason { get; set; } = "";
    public string ChangedBy { get; set; } = "";
    public DateTime? Date { get; set; }
}

public class BranchMonitoringViewModel
{
    public List<BranchInfo> Branches { get; set; } = new List<BranchInfo>();
}

public class BranchInfo
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";        
    public bool IsActive { get; set; }
    public int EmployeesCount { get; set; }
    public int PremisesCount { get; set; }
    public int ReadinessPercent { get; set; }
    public int IncidentsCount { get; set; }
    public DateTime? LastInspectionDate { get; set; }
}

// ============================================================
// ✅ ДОБАВЛЕННЫЙ КЛАСС ДЛЯ СПРАВОЧНИКОВ И РЕЕСТРОВ
// ============================================================
public class DictionaryItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Code { get; set; }        // Универсальное поле для номера/кода
    public string? Number { get; set; }      // Альтернативное поле
    public string? KeyNumber { get; set; }
    public string? IssueDate { get; set; }
    public string? ValidUntil { get; set; }
    public string? Manufacturer { get; set; }
    public string? Inn { get; set; }
    public string? Address { get; set; }
    public string? Date { get; set; }
    public string? Description { get; set; }
    public string? FilePath { get; set; }        // Путь к файлу документа
    public string? FileOriginalName { get; set; } // Оригинальное имя файла
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}