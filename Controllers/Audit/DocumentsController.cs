using AegisCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AegisCore.Controllers.Audit;

[Authorize]
[Area("Audit")]
[Route("audit/documents")]
public class DocumentsController : Controller
{
    private static readonly ConcurrentDictionary<string, List<DictionaryItem>> _mockDataStore = new();
    private readonly IWebHostEnvironment _environment;

    public DocumentsController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet("")]
    public IActionResult Index(string? type, string? search)
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "documents";

        if (string.IsNullOrEmpty(type))
        {
            return View("Documents");
        }

        ViewBag.ActiveDocType = type;
        ViewBag.Search = search;
        ViewBag.Groups = GetDirectoryGroups();
        
        var config = GetDirectoryConfig(type);
        ViewBag.Title = config["title"];
        ViewBag.Columns = config["columns"];
        ViewBag.HasAdditionalFields = config["hasAdditionalFields"];
        
        var items = FilterDirectoryItems(GetDirectoryItems(type), search);
        return View("DocumentsDirectory", items);
    }

    [HttpGet("api/items/{type}")]
    public IActionResult GetItemsApi(string type)
    {
        var items = GetDirectoryItems(type);
        return Json(items);
    }

    [HttpPost("api/save")]
    public IActionResult SaveDirectoryItem([FromBody] DictionaryItemForm form)
    {
        if (string.IsNullOrEmpty(form.Type)) return BadRequest("Тип не указан");

        var items = GetDirectoryItems(form.Type);

        if (!string.IsNullOrWhiteSpace(form.Id))
        {
            var existing = items.FirstOrDefault(i => i.Id == form.Id);
            if (existing != null)
            {
                existing.Name = form.Name ?? existing.Name;
                existing.Code = form.Number ?? existing.Code;
                existing.KeyNumber = form.KeyNumber ?? existing.KeyNumber;
                existing.IssueDate = form.IssueDate ?? existing.IssueDate;
                existing.ValidUntil = form.ValidUntil ?? existing.ValidUntil;
                existing.Manufacturer = form.Manufacturer ?? existing.Manufacturer;
                existing.Inn = form.Inn ?? existing.Inn;
                existing.Address = form.Address ?? existing.Address;
                existing.Date = form.Date ?? existing.Date;
                existing.Description = form.Description ?? existing.Description;
            }
        }
        else
        {
            var newItem = new DictionaryItem
            {
                Id = Guid.NewGuid().ToString(),
                Type = form.Type,
                Name = form.Name ?? "Без названия",
                Code = form.Number,
                KeyNumber = form.KeyNumber,
                IssueDate = form.IssueDate,
                ValidUntil = form.ValidUntil,
                Manufacturer = form.Manufacturer,
                Inn = form.Inn,
                Address = form.Address,
                Date = form.Date,
                Description = form.Description,
                CreatedAt = DateTime.Now
            };
            items.Add(newItem);
        }

        _mockDataStore[form.Type] = items;
        return Json(new { success = true });
    }

    // Загрузка файла (например, для НПА)
    [HttpPost("api/upload-file/{id}")]
    public async Task<IActionResult> UploadFile(string id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, error = "Файл не выбран" });

        var items = GetDirectoryItems("legal-acts");
        var item = items.FirstOrDefault(i => i.Id == id);
        
        if (item == null)
            return NotFound(new { success = false, error = "Запись не найдена" });

        // Создаем папку для хранения файлов НПА
        var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "npa");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        // Если у записи уже был файл, удаляем его
        if (!string.IsNullOrEmpty(item.FilePath))
        {
            var oldFilePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, item.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
            }
        }

        // Генерируем уникальное имя файла
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        // Сохраняем файл
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Сохраняем информацию о файле в запись
        item.FilePath = $"/uploads/npa/{fileName}";
        item.FileOriginalName = file.FileName;
        
        _mockDataStore["legal-acts"] = items;

        return Json(new { success = true, fileName = file.FileName });
    }

    // Скачивание файла
    [HttpGet("api/download-file/{id}")]
    public IActionResult DownloadFile(string id)
    {
        var items = GetDirectoryItems("legal-acts");
        var item = items.FirstOrDefault(i => i.Id == id);
        
        if (item == null || string.IsNullOrEmpty(item.FilePath))
            return NotFound("Файл не найден");

        var filePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, item.FilePath.TrimStart('/'));
        
        if (!System.IO.File.Exists(filePath))
            return NotFound("Файл не найден на сервере");

        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        return File(fileBytes, "application/octet-stream", item.FileOriginalName ?? "document");
    }

    [HttpPost("api/delete/{type}/{id}")]
    public IActionResult DeleteDirectoryItem(string type, string id)
    {
        var items = GetDirectoryItems(type);
        var itemToRemove = items.FirstOrDefault(i => i.Id == id);
        
        if (itemToRemove != null)
        {
            // Если это НПА с файлом, удаляем физический файл
            if (type == "legal-acts" && !string.IsNullOrEmpty(itemToRemove.FilePath))
            {
                var filePath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, itemToRemove.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            
            items.Remove(itemToRemove);
            _mockDataStore[type] = items;
            return Json(new { success = true });
        }

        return Json(new { success = false, error = "Запись не найдена" });
    }

    private List<DictionaryItem> GetDirectoryItems(string type)
    {
        if (_mockDataStore.TryGetValue(type, out var cachedItems))
        {
            return cachedItems;
        }

        var initialData = InitializeMockData(type);
        _mockDataStore[type] = initialData;
        return initialData;
    }

    private List<DictionaryItem> InitializeMockData(string type) => type switch
    {
        "document-types" => new List<DictionaryItem>
        {
            new() { Id = "1", Type = type, Name = "Постановление", CreatedAt = DateTime.Now },
            new() { Id = "2", Type = type, Name = "Приказ", CreatedAt = DateTime.Now },
            new() { Id = "3", Type = type, Name = "Протокол", CreatedAt = DateTime.Now },
            new() { Id = "4", Type = type, Name = "Расписка", CreatedAt = DateTime.Now },
            new() { Id = "5", Type = type, Name = "Распоряжение", CreatedAt = DateTime.Now }
        },
        "positions" => new List<DictionaryItem>
        {
            new() { Id = "1", Type = type, Name = "Администратор безопасности", CreatedAt = DateTime.Now },
            new() { Id = "2", Type = type, Name = "Оператор ПДн", CreatedAt = DateTime.Now },
            new() { Id = "3", Type = type, Name = "Системный администратор", CreatedAt = DateTime.Now }
        },
        "pencils" => new List<DictionaryItem>
        {
            new() { Id = "1", Type = type, Name = "Пенал железный", Code = "111", KeyNumber = "2222999110", CreatedAt = DateTime.Now },
            new() { Id = "2", Type = type, Name = "Пенал ленточный", Code = "499", KeyNumber = "018", CreatedAt = DateTime.Now },
            new() { Id = "3", Type = type, Name = "Пенал на 2 ключа", Code = "067857", KeyNumber = "ключ", CreatedAt = DateTime.Now }
        },
        "szi-certificates" => new List<DictionaryItem>
        {
            new() { Id = "1", Type = type, Name = "Контур-Эксперт", Code = "4397", IssueDate = "14.10.2022", ValidUntil = "14.10.2027", Manufacturer = "ФАП «ГНИИИ ПТЭИ ФСТЭК России»", CreatedAt = DateTime.Now },
            new() { Id = "2", Type = type, Name = "С-ТЕЛЕМ РОЛЛЕР", Code = "4695", IssueDate = "12.10.2022", ValidUntil = "12.10.2027", Manufacturer = "АО Центр «Аттестацияинформ»", CreatedAt = DateTime.Now }
        },
        "skzi-certificates" => new List<DictionaryItem>
        {
            new() { Id = "1", Type = type, Name = "КриптоПро CSP", Code = "9920", IssueDate = "27.12.2023", ValidUntil = "27.12.2028", Manufacturer = "ООО «КРИПТО-ПРО»", CreatedAt = DateTime.Now }
        },
        "organizations" => new List<DictionaryItem>
        {
            new() { Id = "1", Type = type, Name = "ООО «Ромашка»", Inn = "7701234567", Address = "г. Москва, ул. Ленина, д. 1", CreatedAt = DateTime.Now },
            new() { Id = "2", Type = type, Name = "АО «Вектор»", Inn = "7709876543", Address = "г. Санкт-Петербург, Невский пр., д. 10", CreatedAt = DateTime.Now }
        },
        "keys" => new List<DictionaryItem>
        {
            new() { Id = "1", Type = type, Name = "Ключ ЭП Иванов И.И.", Code = "A1B2C3D4", CreatedAt = DateTime.Now },
            new() { Id = "2", Type = type, Name = "Ключ ЭП Петров П.П.", Code = "X9Y8Z7W6", CreatedAt = DateTime.Now }
        },
        "stamps" => new List<DictionaryItem>
        {
            new() { Id = "1", Type = type, Name = "Печать ООО «Ромашка»", Code = "001", CreatedAt = DateTime.Now }
        },
        "legal-acts" => new List<DictionaryItem>
        {
            new() { Id = "1", Type = type, Name = "152-ФЗ «О персональных данных»", Date = "27.07.2006", CreatedAt = DateTime.Now },
            new() { Id = "2", Type = type, Name = "149-ФЗ «Об информации, информационных технологиях и о защите информации»", Date = "27.07.2006", CreatedAt = DateTime.Now },
            new() { Id = "3", Type = type, Name = "187-ФЗ «О безопасности критической информационной инфраструктуры РФ»", Date = "26.07.2017", CreatedAt = DateTime.Now }
        },
        _ => new List<DictionaryItem>()
    };

    private Dictionary<string, object> GetDirectoryGroups()
    {
        var groups = new Dictionary<string, object>();
        
        var directories = new List<Dictionary<string, string>>
        {
            new() { { "id", "document-types" }, { "label", "Виды документов" } },
            new() { { "id", "positions" }, { "label", "Должности" } },
            new() { { "id", "briefings" }, { "label", "Виды инструктажа" } },
            new() { { "id", "szi-maintenance" }, { "label", "Обслуживание СЗИ" } },
            new() { { "id", "testing" }, { "label", "Виды тестирования" } },
            new() { { "id", "tech-documentation" }, { "label", "Техническая документация" } },
            new() { { "id", "storages" }, { "label", "Хранилища" } },
            new() { { "id", "electronic-identifiers" }, { "label", "Электронные идентификаторы" } },
            new() { { "id", "media-types" }, { "label", "Машинные носители" } },
            new() { { "id", "departments" }, { "label", "Подразделения" } }
        };
        groups["directories"] = directories;

        var registries = new List<Dictionary<string, string>>
        {
            new() { { "id", "documents" }, { "label", "Документы" } },
            new() { { "id", "cover-letters" }, { "label", "Сопроводительные письма" } },
            new() { { "id", "organizations" }, { "label", "Организации" } },
            new() { { "id", "keys" }, { "label", "Ключи" } },
            new() { { "id", "stamps" }, { "label", "Печати" } },
            new() { { "id", "pencils" }, { "label", "Пеналы" } },
            new() { { "id", "szi-certificates" }, { "label", "Сертификаты СЗИ" } },
            new() { { "id", "skzi-certificates" }, { "label", "Сертификаты СКЗИ" } },
            new() { { "id", "certificates" }, { "label", "Свидетельства" } }
        };
        groups["registries"] = registries;
        
        groups["npa"] = new Dictionary<string, string> { { "id", "legal-acts" }, { "label", "Нормативно-правовые акты" } };
        
        return groups;
    }

    private Dictionary<string, object> GetDirectoryConfig(string type) => new()
    {
        { "title", GetDirectoryTitle(type) },
        { "columns", GetDirectoryColumns(type) },
        { "hasAdditionalFields", new[] { "szi-certificates", "skzi-certificates", "pencils", "keys", "stamps", "organizations", "cover-letters", "legal-acts" }.Contains(type) }
    };

    private string GetDirectoryTitle(string type) => type switch
    {
        "document-types" => "Виды документов",
        "positions" => "Должности",
        "briefings" => "Виды инструктажа",
        "szi-maintenance" => "Обслуживание СЗИ",
        "testing" => "Виды тестирования",
        "tech-documentation" => "Техническая документация",
        "storages" => "Хранилища",
        "electronic-identifiers" => "Электронные идентификаторы",
        "media-types" => "Машинные носители",
        "departments" => "Подразделения",
        "documents" => "Реестр документов",
        "cover-letters" => "Сопроводительные письма",
        "organizations" => "Реестр организаций",
        "keys" => "Реестр ключей",
        "stamps" => "Реестр печатей",
        "pencils" => "Реестр пеналов",
        "szi-certificates" => "Сертификаты СЗИ",
        "skzi-certificates" => "Сертификаты СКЗИ",
        "certificates" => "Свидетельства",
        "legal-acts" => "Нормативно-правовые акты",
        _ => "Справочник"
    };

    private List<Dictionary<string, string>> GetDirectoryColumns(string type) => type switch
    {
        "pencils" or "keys" or "stamps" => new List<Dictionary<string, string>>
        {
            new() { { "key", "name" }, { "label", "Наименование" } },
            new() { { "key", "code" }, { "label", "Номер/Идентификатор" } },
            new() { { "key", "keyNumber" }, { "label", "Номер ключа" } }
        },
        "szi-certificates" or "skzi-certificates" => new List<Dictionary<string, string>>
        {
            new() { { "key", "name" }, { "label", "Наименование" } },
            new() { { "key", "code" }, { "label", "Номер сертификата" } },
            new() { { "key", "issueDate" }, { "label", "Дата выдачи" } },
            new() { { "key", "validUntil" }, { "label", "Действителен до" } },
            new() { { "key", "manufacturer" }, { "label", "Изготовитель" } }
        },
        "organizations" => new List<Dictionary<string, string>>
        {
            new() { { "key", "name" }, { "label", "Наименование" } },
            new() { { "key", "inn" }, { "label", "ИНН" } },
            new() { { "key", "address" }, { "label", "Адрес" } }
        },
        "cover-letters" => new List<Dictionary<string, string>>
        {
            new() { { "key", "name" }, { "label", "Наименование" } },
            new() { { "key", "code" }, { "label", "Номер" } },
            new() { { "key", "date" }, { "label", "Дата" } }
        },
        "legal-acts" => new List<Dictionary<string, string>>
        {
            new() { { "key", "name" }, { "label", "Наименование акта" } },
            new() { { "key", "date" }, { "label", "Дата принятия" } },
            new() { { "key", "file" }, { "label", "Файл документа" } }
        },
        _ => new List<Dictionary<string, string>> { new() { { "key", "name" }, { "label", "Наименование" } } }
    };

    private List<DictionaryItem> FilterDirectoryItems(List<DictionaryItem> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(item => 
            (!string.IsNullOrEmpty(item.Name) && item.Name.ToLower().Contains(s)) ||
            (!string.IsNullOrEmpty(item.Code) && item.Code.ToLower().Contains(s)) ||
            (!string.IsNullOrEmpty(item.Inn) && item.Inn.ToLower().Contains(s))
        ).ToList();
    }
}