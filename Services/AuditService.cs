using AegisCore.Models;
using System.Text.Json;

namespace AegisCore.Services;

public class AuditService
{
    private readonly string _dataDir;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };
    private readonly object _lock = new();

    public AuditService(IWebHostEnvironment env)
    {
        _dataDir = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(_dataDir);
        Seed();
    }

    // ============================================================
    // ОРГАНИЗАЦИЯ
    // ============================================================

    public OrganizationData GetOrganization()
    {
        return LoadSingle<OrganizationData>("organization.json") ?? new OrganizationData();
    }

    public void SaveOrganization(OrganizationData org)
    {
        org.UpdatedAt = DateTime.Now;
        SaveSingle("organization.json", org);
    }

    // ============================================================
    // ПОДРАЗДЕЛЕНИЯ
    // ============================================================

    public List<Department> GetDepartments() => Load<Department>("departments.json");
    public void SaveDepartments(List<Department> items) => Save("departments.json", items);

    public void AddDepartment(Department dept)
    {
        var departments = GetDepartments();
        if (string.IsNullOrEmpty(dept.Id)) dept.Id = Guid.NewGuid().ToString();
        departments.Add(dept);
        SaveDepartments(departments);
    }

    public void UpdateDepartment(Department dept)
    {
        var departments = GetDepartments();
        var existing = departments.FirstOrDefault(d => d.Id == dept.Id);
        if (existing != null)
        {
            existing.Name = dept.Name;
            SaveDepartments(departments);
        }
    }

    public void DeleteDepartment(string id)
    {
        var departments = GetDepartments();
        departments.RemoveAll(d => d.Id == id);
        SaveDepartments(departments);
    }

    // ============================================================
    // ПОМЕЩЕНИЯ
    // ============================================================

    public List<Room> GetRooms() => Load<Room>("rooms.json");
    public void SaveRooms(List<Room> items) => Save("rooms.json", items);

    public void AddRoom(Room room)
    {
        var rooms = GetRooms();
        if (string.IsNullOrEmpty(room.Id)) room.Id = Guid.NewGuid().ToString();
        rooms.Add(room);
        SaveRooms(rooms);
    }

    public void UpdateRoom(Room room)
    {
        var rooms = GetRooms();
        var existing = rooms.FirstOrDefault(r => r.Id == room.Id);
        if (existing != null)
        {
            existing.Number = room.Number;
            existing.Description = room.Description;
            existing.Department = room.Department;
            existing.DateIn = room.DateIn;
            existing.DateOut = room.DateOut;
            SaveRooms(rooms);
        }
    }

    public void DeleteRoom(string id)
    {
        var rooms = GetRooms();
        rooms.RemoveAll(r => r.Id == id);
        SaveRooms(rooms);
    }

    // ============================================================
    // СОТРУДНИКИ
    // ============================================================

    public List<Employee> GetEmployees() => Load<Employee>("employees.json");
    public void SaveEmployees(List<Employee> items) => Save("employees.json", items);

    public void AddEmployee(Employee employee)
    {
        var employees = GetEmployees();
        if (string.IsNullOrEmpty(employee.Id)) employee.Id = Guid.NewGuid().ToString();
        employees.Add(employee);
        SaveEmployees(employees);
    }

    public void UpdateEmployee(Employee employee)
    {
        var employees = GetEmployees();
        var existing = employees.FirstOrDefault(e => e.Id == employee.Id);
        if (existing != null)
        {
            existing.Fio = employee.Fio;
            existing.Position = employee.Position;
            existing.Department = employee.Department;
            existing.DateStart = employee.DateStart;
            existing.DateEnd = employee.DateEnd;
            existing.Phone = employee.Phone;
            existing.Email = employee.Email;
            SaveEmployees(employees);
        }
    }

    public void DeleteEmployee(string id)
    {
        var employees = GetEmployees();
        employees.RemoveAll(e => e.Id == id);
        SaveEmployees(employees);
    }

    // ============================================================
    // ДОСТУПЫ
    // ============================================================

    public List<AccessRecord> GetAccesses() => Load<AccessRecord>("accesses.json");
    public void SaveAccesses(List<AccessRecord> items) => Save("accesses.json", items);

    public void AddAccess(AccessRecord access)
    {
        var accesses = GetAccesses();
        if (string.IsNullOrEmpty(access.Id)) access.Id = Guid.NewGuid().ToString();
        accesses.Add(access);
        SaveAccesses(accesses);
    }

    public void UpdateAccess(AccessRecord access)
    {
        var accesses = GetAccesses();
        var existing = accesses.FirstOrDefault(a => a.Id == access.Id);
        if (existing != null)
        {
            existing.EmployeeId = access.EmployeeId;
            existing.EmployeeFio = access.EmployeeFio;
            existing.Department = access.Department;
            existing.AccessLevel = access.AccessLevel;
            existing.IssueDate = access.IssueDate;
            existing.PdnISPDn = access.PdnISPDn;
            existing.PdnPaper = access.PdnPaper;
            existing.ConfInfo = access.ConfInfo;
            existing.SkziRoom = access.SkziRoom;
            existing.SkziWork = access.SkziWork;
            SaveAccesses(accesses);
        }
    }

    public void DeleteAccess(string id)
    {
        var accesses = GetAccesses();
        accesses.RemoveAll(a => a.Id == id);
        SaveAccesses(accesses);
    }

    // ============================================================
    // ФИЛИАЛЫ
    // ============================================================

    public List<Branch> GetBranches() => Load<Branch>("branches.json");
    public void SaveBranches(List<Branch> items) => Save("branches.json", items);

    public void AddBranch(Branch branch)
    {
        var branches = GetBranches();
        if (string.IsNullOrEmpty(branch.Id)) branch.Id = Guid.NewGuid().ToString();
        branches.Add(branch);
        SaveBranches(branches);
    }

    public void UpdateBranch(Branch branch)
    {
        var branches = GetBranches();
        var existing = branches.FirstOrDefault(b => b.Id == branch.Id);
        if (existing != null)
        {
            existing.Name = branch.Name;
            existing.Address = branch.Address;
            existing.Director = branch.Director;
            SaveBranches(branches);
        }
    }

    public void DeleteBranch(string id)
    {
        var branches = GetBranches();
        branches.RemoveAll(b => b.Id == id);
        SaveBranches(branches);
    }

    // ============================================================
    // КАЛЕНДАРЬ
    // ============================================================

    public List<CalendarEvent> GetCalendarEvents() => Load<CalendarEvent>("calendar_events.json");
    public void SaveCalendarEvents(List<CalendarEvent> items) => Save("calendar_events.json", items);

    // ============================================================
    // ЖУРНАЛЫ
    // ============================================================

    public List<JournalEntry> GetJournals(string type) => Load<JournalEntry>($"journal_{type}.json");
    public void SaveJournals(string type, List<JournalEntry> items) => Save($"journal_{type}.json", items);

    // ============================================================
    // ИНФРАСТРУКТУРА
    // ============================================================

    public List<InfoSystem> GetInfoSystems() => Load<InfoSystem>("info_systems.json");
    public void SaveInfoSystems(List<InfoSystem> items) => Save("info_systems.json", items);

    public List<Storage> GetStorages() => Load<Storage>("storages.json");
    public void SaveStorages(List<Storage> items) => Save("storages.json", items);

    public List<TechnicalAsset> GetTechnicalAssets() => Load<TechnicalAsset>("technical_assets.json");
    public void SaveTechnicalAssets(List<TechnicalAsset> items) => Save("technical_assets.json", items);

    public List<Szi> GetSzi() => Load<Szi>("szi.json");
    public void SaveSzi(List<Szi> items) => Save("szi.json", items);

    public List<Skzi> GetSkzi() => Load<Skzi>("skzi.json");
    public void SaveSkzi(List<Skzi> items) => Save("skzi.json", items);

    public List<Identifier> GetIdentifiers() => Load<Identifier>("identifiers.json");
    public void SaveIdentifiers(List<Identifier> items) => Save("identifiers.json", items);

    public List<Signature> GetSignatures() => Load<Signature>("signatures.json");
    public void SaveSignatures(List<Signature> items) => Save("signatures.json", items);

    public List<Media> GetMedia() => Load<Media>("media.json");
    public void SaveMedia(List<Media> items) => Save("media.json", items);

    public List<Software> GetSoftware() => Load<Software>("software.json");
    public void SaveSoftware(List<Software> items) => Save("software.json", items);

    public List<Asutp> GetAsutp() => Load<Asutp>("asutp.json");
    public void SaveAsutp(List<Asutp> items) => Save("asutp.json", items);

    public List<Its> GetIts() => Load<Its>("its.json");
    public void SaveIts(List<Its> items) => Save("its.json", items);

    // ============================================================
    // ✅ НОВОЕ: СПРАВОЧНИКИ ДЛЯ ИНФРАСТРУКТУРЫ
    // ============================================================

    // Справочник организаций (для СЗИ/СКЗИ)
    public List<OrganizationReference> GetOrganizations() => Load<OrganizationReference>("organizations_ref.json");
    public void SaveOrganizations(List<OrganizationReference> items) => Save("organizations_ref.json", items);

    // Комплектации СЗИ
    public List<string> GetConfigurations() => Load<string>("configurations.json");
    public void SaveConfigurations(List<string> items) => Save("configurations.json", items);

    // Сопроводительные письма СКЗИ
    public List<string> GetCoverLetters() => Load<string>("cover_letters.json");
    public void SaveCoverLetters(List<string> items) => Save("cover_letters.json", items);

    // Наименования СКЗИ
    public List<string> GetSkziNames() => Load<string>("skzi_names.json");
    public void SaveSkziNames(List<string> items) => Save("skzi_names.json", items);

    // Типы технических средств
    public List<string> GetTechnicalAssetTypes() => Load<string>("technical_asset_types.json");
    public void SaveTechnicalAssetTypes(List<string> items) => Save("technical_asset_types.json", items);

    // Типы хранилищ
    public List<string> GetStorageTypes() => Load<string>("storage_types.json");
    public void SaveStorageTypes(List<string> items) => Save("storage_types.json", items);

    // Типы идентификаторов
    public List<string> GetIdentifierTypes() => Load<string>("identifier_types.json");
    public void SaveIdentifierTypes(List<string> items) => Save("identifier_types.json", items);

    // Типы носителей
    public List<string> GetMediaTypes() => Load<string>("media_types.json");
    public void SaveMediaTypes(List<string> items) => Save("media_types.json", items);

    // Типы ПО
    public List<string> GetSoftwareTypes() => Load<string>("software_types.json");
    public void SaveSoftwareTypes(List<string> items) => Save("software_types.json", items);

    // Номера ключевой информации (для электронных подписей)
    public List<string> GetKeyInfoNumbers() => Load<string>("key_info_numbers.json");
    public void SaveKeyInfoNumbers(List<string> items) => Save("key_info_numbers.json", items);

    // ОСИ (для электронных подписей)
    public List<string> GetOSIList() => Load<string>("osi_list.json");
    public void SaveOSIList(List<string> items) => Save("osi_list.json", items);

    // ============================================================
    // ✅ НОВОЕ: РАБОТА С ДИРЕКТОРИЯМИ И ФАЙЛАМИ ДОКУМЕНТОВ
    // ============================================================

    // Получить корневую директорию данных
    public string GetDataDirectory() => _dataDir;

    // Получить директорию для документов конкретного объекта
    public string GetDocumentsDirectory(string objectId)
    {
        var dir = Path.Combine(_dataDir, "documents", objectId);
        Directory.CreateDirectory(dir);
        return dir;
    }

    // Получить путь к файлу утверждённого документа
    public string? GetApprovedDocumentPath(string objectId, string documentId)
    {
        var dir = GetDocumentsDirectory(objectId);
        if (!Directory.Exists(dir)) return null;
        
        var files = Directory.GetFiles(dir, $"{documentId}_*");
        return files.Length > 0 ? files[0] : null;
    }

    // ============================================================
    // ОБЪЕКТЫ ИНФОРМАТИЗАЦИИ
    // ============================================================

    public List<InformatizationObject> GetInformatizationObjects()
        => Load<InformatizationObject>("informatization_objects.json");

    public void SaveInformatizationObjects(List<InformatizationObject> items)
        => Save("informatization_objects.json", items);

    public void AddInformatizationObject(InformatizationObject obj)
    {
        var objects = GetInformatizationObjects();
        if (string.IsNullOrEmpty(obj.Id)) obj.Id = Guid.NewGuid().ToString();
        obj.CreatedAt = DateTime.Now;
        objects.Add(obj);
        SaveInformatizationObjects(objects);
    }

    public void UpdateInformatizationObject(InformatizationObject obj)
    {
        var objects = GetInformatizationObjects();
        var existing = objects.FirstOrDefault(o => o.Id == obj.Id);
        if (existing != null)
        {
            obj.CreatedAt = existing.CreatedAt;
            obj.UpdatedAt = DateTime.Now;
            var index = objects.IndexOf(existing);
            objects[index] = obj;
            SaveInformatizationObjects(objects);
        }
    }

    public void DeleteInformatizationObject(string id)
    {
        var objects = GetInformatizationObjects();
        objects.RemoveAll(o => o.Id == id);
        SaveInformatizationObjects(objects);
    }

    public InformatizationObject? GetInformatizationObject(string id)
    {
        return GetInformatizationObjects().FirstOrDefault(o => o.Id == id);
    }

    // Получить СЗИ с неистёкшим сроком действия сертификата
    public List<Szi> GetValidSzi()
    {
        return GetSzi().Where(s => !s.InstallByDate.HasValue || s.InstallByDate > DateTime.Now).ToList();
    }

    // Получить СКЗИ с неистёкшим сроком действия
    public List<Skzi> GetValidSkzi()
    {
        return GetSkzi().Where(s => !s.IssueDate.HasValue || s.IssueDate.Value.AddYears(1) > DateTime.Now).ToList();
    }

    // ============================================================
    // АКТИВНОСТЬ И СТАТИСТИКА
    // ============================================================

    public List<AuditActivity> GetRecentActivities(int count = 10)
    {
        var activities = Load<AuditActivity>("audit_activities.json");
        return activities.OrderByDescending(a => a.Date).Take(count).ToList();
    }

    public void LogActivity(AuditActivity activity)
    {
        var list = Load<AuditActivity>("audit_activities.json");
        list.Add(activity);
        Save("audit_activities.json", list);
    }

    public AuditDashboardStats GetAuditStats()
    {
        return new AuditDashboardStats(
            Departments: GetDepartments().Count,
            Employees: GetEmployees().Count,
            Rooms: GetRooms().Count,
            Journals: 23,
            Activities: GetRecentActivities(100).Count
        );
    }

    // ============================================================
    // ПРИВАТНЫЕ МЕТОДЫ
    // ============================================================

    private List<T> Load<T>(string file)
    {
        lock (_lock)
        {
            var path = Path.Combine(_dataDir, file);
            if (!File.Exists(path)) return new List<T>();
            var json = File.ReadAllText(path);
            return string.IsNullOrWhiteSpace(json)
                ? new List<T>()
                : JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
        }
    }

    private T? LoadSingle<T>(string file) where T : class
    {
        lock (_lock)
        {
            var path = Path.Combine(_dataDir, file);
            if (!File.Exists(path)) return null;
            var json = File.ReadAllText(path);
            return string.IsNullOrWhiteSpace(json)
                ? null
                : JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
    }

    private void Save<T>(string file, List<T> items)
    {
        lock (_lock)
        {
            var path = Path.Combine(_dataDir, file);
            File.WriteAllText(path, JsonSerializer.Serialize(items, _jsonOptions));
        }
    }

    private void SaveSingle<T>(string file, T item)
    {
        lock (_lock)
        {
            var path = Path.Combine(_dataDir, file);
            File.WriteAllText(path, JsonSerializer.Serialize(item, _jsonOptions));
        }
    }

    // ============================================================
    // SEED - Начальные данные
    // ============================================================

    private void Seed()
    {
        if (!File.Exists(Path.Combine(_dataDir, "organization.json")))
        {
            SaveSingle("organization.json", new OrganizationData
            {
                OrgType = "commercial",
                FullName = "ООО \"АэгисКор Технологии\"",
                ShortName = "АэгисКор",
                Inn = "7701234567",
                Kpp = "770101001",
                Ogrn = "1027700123456",
                Address = "г. Москва, ул. Тверская, д. 1",
                AddressIndex = "125009",
                PostalAddress = "г. Москва, а/я 123",
                PostalIndex = "125009",
                Phone = "+7 (495) 123-45-67",
                Email = "info@aegiscore.ru",
                Director = "Емельянов Алексей Вячеславович",
                IsCrypto = false
            });
        }

        if (!File.Exists(Path.Combine(_dataDir, "departments.json")))
        {
            SaveDepartments(new List<Department>
            {
                new() { Name = "Генеральная дирекция" },
                new() { Name = "Отдел информационной безопасности" },
                new() { Name = "IT-отдел" }
            });
        }

        if (!File.Exists(Path.Combine(_dataDir, "rooms.json")))
        {
            SaveRooms(new List<Room>
            {
                new() { Number = "101", Description = "Серверная", Department = "IT-отдел", DateIn = new DateTime(2023, 1, 15) },
                new() { Number = "205", Description = "Кабинет директора", Department = "Генеральная дирекция", DateIn = new DateTime(2020, 3, 1) }
            });
        }

        if (!File.Exists(Path.Combine(_dataDir, "employees.json")))
        {
            SaveEmployees(new List<Employee>
            {
                new() { Fio = "Емельянов Алексей Вячеславович", Position = "Генеральный директор", Department = "Генеральная дирекция", DateStart = new DateTime(2020, 1, 10), Phone = "+7(495)123-45-01", Email = "a.emelyanov@aegiscore.ru" },
                new() { Fio = "Иванов Иван Иванович", Position = "Специалист по ИБ", Department = "Отдел информационной безопасности", DateStart = new DateTime(2021, 5, 15), Phone = "+7(495)123-45-02", Email = "i.ivanov@aegiscore.ru" }
            });
        }

        if (!File.Exists(Path.Combine(_dataDir, "accesses.json")))
        {
            SaveAccesses(new List<AccessRecord>
            {
                new() { EmployeeId = "1", EmployeeFio = "Емельянов Алексей Вячеславович", Department = "Генеральная дирекция", AccessLevel = "Полный", IssueDate = new DateTime(2020, 1, 10), PdnISPDn = true, PdnPaper = true, ConfInfo = true, SkziRoom = true, SkziWork = true }
            });
        }

        if (!File.Exists(Path.Combine(_dataDir, "branches.json")))
        {
            SaveBranches(new List<Branch>
            {
                new() { Name = "Филиал в Санкт-Петербурге", Address = "г. СПб, Невский пр., д. 100", Director = "Сидоров А.А." }
            });
        }

        if (!File.Exists(Path.Combine(_dataDir, "calendar_events.json")))
        {
            SaveCalendarEvents(new List<CalendarEvent>
            {
                new() { Title = "Плановая проверка СЗИ", StartDate = DateTime.Now.AddDays(5), Type = "testing", Completed = false }
            });
        }

        if (!File.Exists(Path.Combine(_dataDir, "info_systems.json")))
        {
            SaveInfoSystems(new List<InfoSystem>
            {
                new() { Name = "1С:Бухгалтерия", Type = "ИСПДн", SecurityClass = "К2", SecurityLevel = "УЗ1" },
                new() { Name = "Система документооборота", Type = "ИСПДн", SecurityClass = "К3", SecurityLevel = "УЗ2" }
            });
        }

        // ✅ НОВОЕ: Начальные данные для справочников
        if (!File.Exists(Path.Combine(_dataDir, "organizations_ref.json")))
        {
            SaveOrganizations(new List<OrganizationReference>
            {
                new() { Name = "ООО \"КриптоПро\"", FullName = "Общество с ограниченной ответственностью \"КриптоПро\"", Address = "г. Москва", Inn = "7710712345", Phone = "+7(495)123-45-67", Email = "info@cryptopro.ru" },
                new() { Name = "АО \"РусКрипто\"", FullName = "Акционерное общество \"РусКрипто\"", Address = "г. Москва", Inn = "7707123456", Phone = "+7(495)987-65-43", Email = "info@ruscrypto.ru" }
            });
        }

        if (!File.Exists(Path.Combine(_dataDir, "configurations.json")))
        {
            SaveConfigurations(new List<string>
            {
                "Базовая комплектация",
                "Расширенная комплектация",
                "Профессиональная комплектация"
            });
        }

        if (!File.Exists(Path.Combine(_dataDir, "cover_letters.json")))
        {
            SaveCoverLetters(new List<string>
            {
                "Письмо №123 от 15.01.2024",
                "Письмо №456 от 20.02.2024",
                "Письмо №789 от 10.03.2024"
            });
        }

        if (!File.Exists(Path.Combine(_dataDir, "skzi_names.json")))
        {
            SaveSkziNames(new List<string>
            {
                "КриптоПро CSP",
                "ViPNet CSP",
                "Signal-COM CSP"
            });
        }

        if (!File.Exists(Path.Combine(_dataDir, "technical_asset_types.json")))
        {
            SaveTechnicalAssetTypes(new List<string>
            {
                "Сервер",
                "Рабочая станция",
                "Ноутбук",
                "Планшет",
                "Принтер",
                "МФУ",
                "Маршрутизатор",
                "Коммутатор",
                "Сетевой экран"
            });
        }

        if (!File.Exists(Path.Combine(_dataDir, "storage_types.json")))
        {
            SaveStorageTypes(new List<string>
            {
                "Сейф",
                "Металлический шкаф",
                "Запираемый шкаф",
                "Ящик",
                "Специальное хранилище"
            });
        }

        if (!File.Exists(Path.Combine(_dataDir, "identifier_types.json")))
        {
            SaveIdentifierTypes(new List<string>
            {
                "USB-токен",
                "Смарт-карта",
                "Электронный ключ",
                "RFID-метка",
                "Биометрический идентификатор"
            });
        }

        if (!File.Exists(Path.Combine(_dataDir, "media_types.json")))
        {
            SaveMediaTypes(new List<string>
            {
                "Жёсткий диск (HDD)",
                "SSD",
                "USB-флешка",
                "CD/DVD",
                "Карта памяти",
                "Дискета"
            });
        }

        if (!File.Exists(Path.Combine(_dataDir, "software_types.json")))
        {
            SaveSoftwareTypes(new List<string>
            {
                "Операционная система",
                "СУБД",
                "Средство защиты информации",
                "Офисное ПО",
                "Антивирус",
                "Прикладное ПО",
                "Системное ПО"
            });
        }

        if (!File.Exists(Path.Combine(_dataDir, "key_info_numbers.json")))
        {
            SaveKeyInfoNumbers(new List<string>
            {
                "КИ-001-2024",
                "КИ-002-2024",
                "КИ-003-2024"
            });
        }

        if (!File.Exists(Path.Combine(_dataDir, "osi_list.json")))
        {
            SaveOSIList(new List<string>
            {
                "ОСИ-001",
                "ОСИ-002",
                "ОСИ-003"
            });
        }

        // ✅ НОВОЕ: Начальные данные для объектов информатизации
        if (!File.Exists(Path.Combine(_dataDir, "informatization_objects.json")))
        {
            SaveInformatizationObjects(new List<InformatizationObject>
            {
                new()
                {
                    Name = "Автоматизированная система управления персоналом",
                    InventoryNumber = "ОИ-001",
                    Location = "г. Москва, офис 305",
                    Classification = "ИСПДн",
                    SecurityLevel = "УЗ1",
                    ComplianceStatus = "Соответствует",
                    Purpose = "Автоматизация кадрового учёта"
                }
            });
        }

        if (!File.Exists(Path.Combine(_dataDir, "audit_activities.json")))
        {
            Save("audit_activities.json", new List<AuditActivity>
            {
                new() { Date = DateTime.Now.AddHours(-2), Event = "Проверка журнала доступа", User = "admin", Status = "OK" }
            });
        }

        var journalTypes = new[] { "accessrequests", "antivirus", "backups", "carriers", "default",
            "dspdistribution", "electronicid", "emergency", "incomingdsp", "instruction",
            "internalchecks", "modification", "outgoingdsp", "pdnrequests", "pdntransfer",
            "penals", "prepareddocs", "sealing", "skzi", "sziinstall", "szitesting", "technical", "universal" };

        foreach (var type in journalTypes)
        {
            if (!File.Exists(Path.Combine(_dataDir, $"journal_{type}.json")))
            {
                Save($"journal_{type}.json", new List<JournalEntry>());
            }
        }
    }
}