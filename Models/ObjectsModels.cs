namespace AegisCore.Models;

// ============================================================
// ОБЪЕКТЫ ИНФОРМАТИЗАЦИИ
// ============================================================

public class InformatizationObject
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string InventoryNumber { get; set; } = "";
    public string Location { get; set; } = "";
    public string Classification { get; set; } = "";
    public string SecurityLevel { get; set; } = "";
    public string ComplianceStatus { get; set; } = "";
    public string Purpose { get; set; } = "";
    public string Tasks { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    
    // ✅ НОВЫЕ ПОЛЯ для формы "Основные сведения" (Рисунок 103)
    public DateTime? DateIn { get; set; }                        // Дата ввода в эксплуатацию
    public DateTime? DateOut { get; set; }                       // Дата вывода в эксплуатацию
    public string RoomNumber { get; set; } = "";                 // Помещение №
    public string InformationOwner { get; set; } = "";           // Обладатель информации
    public string Goals { get; set; } = "";                      // Цели
    public string ProcessedData { get; set; } = "";              // Обрабатываемые сведения
    public string PlacementType { get; set; } = "";              // Вид размещения
    public string ProcessingMode { get; set; } = "";             // Режим обработки данных
    public string SystemInteraction { get; set; } = "";          // Взаимодействие с другими системами
    public string SecurityCharacteristics { get; set; } = "";    // Характеристики безопасности
    public bool IsCriticalObject { get; set; } = false;          // Установлено на критически важных объектах
    public bool UsesSkzi { get; set; } = false;                  // Используется СКЗИ
    
    // ✅ Поля для актов ИС и ИСПДн
    public string ISClass { get; set; } = "";                    // Класс защищенности ИС (К1-К4)
    public DateTime? ISActDate { get; set; }                     // Дата акта ИС
    public string ISActNumber { get; set; } = "";                // Номер акта ИС
    public string ISPDnClass { get; set; } = "";                 // Класс защищенности ИСПДн (УЗ1-УЗ4)
    public DateTime? ISPDnActDate { get; set; }                  // Дата акта ИСПДн
    public string ISPDnActNumber { get; set; } = "";             // Номер акта ИСПДн
    
    // ✅ Поля для протокола, заключения, аттестата
    public DateTime? ProtocolDate { get; set; }                  // Дата протокола
    public string ProtocolNumber { get; set; } = "";             // Номер протокола
    public DateTime? ConclusionDate { get; set; }                // Дата заключения
    public string ConclusionNumber { get; set; } = "";           // Номер заключения
    public DateTime? CertificateDate { get; set; }               // Дата аттестата
    public string CertificateNumber { get; set; } = "";          // Номер аттестата
    
    // Связи с другими сущностями
    public List<string> TechnicalAssetIds { get; set; } = new();
    public List<string> SoftwareIds { get; set; } = new();
    public List<string> SziIds { get; set; } = new();
    public List<string> SkziIds { get; set; } = new();
    public List<string> TelecomIds { get; set; } = new();
    public List<string> InfoSystemIds { get; set; } = new();
    
    // История изменений
    public List<ChangeHistoryRecord> ChangeHistory { get; set; } = new();
    public bool TrackChanges { get; set; } = true;
    
    // ✅ Используем ObjectDocument из InfrastructureModels.cs
    public List<ObjectDocument> Documents { get; set; } = new();
    
    // Аттестация
    public string AttestationNumber { get; set; } = "";
    public DateTime? AttestationDate { get; set; }
    public DateTime? AttestationValidUntil { get; set; }
    public string AttestationBody { get; set; } = "";
    
    // БД и ЦОД
    public string DatabaseLocation { get; set; } = "";
    public string DataCenterAddress { get; set; } = "";
    public string DataCenterType { get; set; } = "";
    
    // ✅ Используем StructuralElement, UserGroup, ObjectScheme из InfrastructureModels.cs
    public List<StructuralElement> StructuralElements { get; set; } = new();
    public List<UserGroup> UserGroups { get; set; } = new();
    public List<ObjectScheme> Schemes { get; set; } = new();
}

public class ChangeHistoryRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Date { get; set; } = DateTime.Now;
    public string ChangeType { get; set; } = "";
    public string Description { get; set; } = "";
    public string Reason { get; set; } = "";
    public string ChangedBy { get; set; } = "";
}

public class ObjectsViewModel
{
    public string ActiveTab { get; set; } = "main";
    public string Search { get; set; } = "";
    public string? Sort { get; set; }
    public string? SortOrder { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 0;
    
    public List<InformatizationObject> Objects { get; set; } = new();
    public InformatizationObject? CurrentObject { get; set; }
    
    public List<TechnicalAsset> AvailableTechnicalAssets { get; set; } = new();
    public List<Software> AvailableSoftware { get; set; } = new();
    public List<Szi> AvailableSzi { get; set; } = new();
    public List<Skzi> AvailableSkzi { get; set; } = new();
    public List<InfoSystem> AvailableInfoSystems { get; set; } = new();
    public List<Employee> AvailableEmployees { get; set; } = new();
    public List<Room> AvailableRooms { get; set; } = new();
}