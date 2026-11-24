using AegisCore.Models;
using AegisCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegisCore.Controllers.Audit;

[Authorize]
[Area("Audit")]
[Route("audit/objects")]
public class ObjectsController : Controller
{
    private readonly AuditService _audit;

    public ObjectsController(AuditService audit)
    {
        _audit = audit;
    }

    [HttpGet("")]
    public IActionResult Index(string? search, int page = 1, string? sort = null, string? order = null, int? pageSize = null)
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "objects";

        var objects = FilterObjects(_audit.GetInformatizationObjects(), search);

        if (!string.IsNullOrEmpty(sort))
        {
            objects = sort.ToLower() switch
            {
                "name" => order == "desc" ? objects.OrderByDescending(o => o.Name).ToList() : objects.OrderBy(o => o.Name).ToList(),
                "inventorynumber" => order == "desc" ? objects.OrderByDescending(o => o.InventoryNumber).ToList() : objects.OrderBy(o => o.InventoryNumber).ToList(),
                "classification" => order == "desc" ? objects.OrderByDescending(o => o.Classification).ToList() : objects.OrderBy(o => o.Classification).ToList(),
                "location" => order == "desc" ? objects.OrderByDescending(o => o.Location).ToList() : objects.OrderBy(o => o.Location).ToList(),
                _ => objects.OrderByDescending(o => o.CreatedAt).ToList()
            };
        }

        var ps = pageSize ?? 10;
        var model = new ObjectsViewModel
        {
            Search = search ?? "",
            Sort = sort,
            SortOrder = order,
            CurrentPage = page,
            PageSize = ps,
            TotalItems = objects.Count,
            Objects = objects.Skip((page - 1) * ps).Take(ps).ToList()
        };

        return View("~/Areas/Audit/Views/Objects/Objects.cshtml", model);
    }

    [HttpGet("details/{id}")]
    public IActionResult Details(string id, string tab = "main")
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "objects";

        InformatizationObject? obj = null;
        if (id != "new")
        {
            obj = _audit.GetInformatizationObject(id);
            if (obj == null) return NotFound();
        }

        var model = new ObjectsViewModel
        {
            ActiveTab = tab,
            CurrentObject = obj,
            AvailableTechnicalAssets = _audit.GetTechnicalAssets(),
            AvailableSoftware = _audit.GetSoftware(),
            AvailableSzi = _audit.GetValidSzi(),
            AvailableSkzi = _audit.GetValidSkzi(),
            AvailableInfoSystems = _audit.GetInfoSystems(),
            AvailableEmployees = _audit.GetEmployees(),
            // ✅ НОВОЕ: Загрузка помещений
            AvailableRooms = _audit.GetRooms()
        };

        return View("~/Areas/Audit/Views/Objects/Details.cshtml", model);
    }

    [HttpPost("save")]
    public IActionResult SaveObject([FromBody] InformatizationObject obj)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(obj.Name))
                return Json(new { success = false, error = "Наименование обязательно" });

            if (string.IsNullOrEmpty(obj.Id))
            {
                _audit.AddInformatizationObject(obj);
                _audit.LogActivity(new AuditActivity
                {
                    Event = $"Добавление объекта информатизации: {obj.Name}",
                    User = User.Identity?.Name ?? "unknown",
                    Status = "OK"
                });
            }
            else
            {
                _audit.UpdateInformatizationObject(obj);
                _audit.LogActivity(new AuditActivity
                {
                    Event = $"Редактирование объекта: {obj.Name}",
                    User = User.Identity?.Name ?? "unknown",
                    Status = "OK"
                });
            }

            return Json(new { success = true, id = obj.Id });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpDelete("delete/{id}")]
    public IActionResult DeleteObject(string id)
    {
        try
        {
            var obj = _audit.GetInformatizationObject(id);
            if (obj == null)
                return Json(new { success = false, error = "Объект не найден" });

            _audit.DeleteInformatizationObject(id);
            _audit.LogActivity(new AuditActivity
            {
                Event = $"Удаление объекта: {obj?.Name}",
                User = User.Identity?.Name ?? "unknown",
                Status = "OK"
            });

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    // ============================================================
    // ✅ НОВОЕ: МАССОВОЕ СОХРАНЕНИЕ СВЯЗЕЙ (для модалок с "Выбрать все")
    // ============================================================

    [HttpPost("{id}/set-technical-assets")]
    public IActionResult SetTechnicalAssets(string id, [FromBody] List<string> assetIds)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });

        var added = assetIds.Where(a => !obj.TechnicalAssetIds.Contains(a)).ToList();
        var removed = obj.TechnicalAssetIds.Where(a => !assetIds.Contains(a)).ToList();

        obj.TechnicalAssetIds = assetIds ?? new List<string>();

        if (obj.TrackChanges)
        {
            foreach (var a in added)
                AddAutoHistory(obj, "Добавление ТС", $"Добавлено техническое средство ID: {a}");
            foreach (var r in removed)
                AddAutoHistory(obj, "Удаление ТС", $"Удалено техническое средство ID: {r}");
        }

        _audit.UpdateInformatizationObject(obj);
        return Json(new { success = true });
    }

    [HttpPost("{id}/set-software")]
    public IActionResult SetSoftware(string id, [FromBody] List<string> softwareIds)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });

        var added = softwareIds.Where(s => !obj.SoftwareIds.Contains(s)).ToList();
        var removed = obj.SoftwareIds.Where(s => !softwareIds.Contains(s)).ToList();

        obj.SoftwareIds = softwareIds ?? new List<string>();

        if (obj.TrackChanges)
        {
            foreach (var a in added)
                AddAutoHistory(obj, "Добавление ПО", $"Добавлено ПО ID: {a}");
            foreach (var r in removed)
                AddAutoHistory(obj, "Удаление ПО", $"Удалено ПО ID: {r}");
        }

        _audit.UpdateInformatizationObject(obj);
        return Json(new { success = true });
    }

    [HttpPost("{id}/set-szi")]
    public IActionResult SetSzi(string id, [FromBody] List<string> sziIds)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });

        var added = sziIds.Where(s => !obj.SziIds.Contains(s)).ToList();
        var removed = obj.SziIds.Where(s => !sziIds.Contains(s)).ToList();

        obj.SziIds = sziIds ?? new List<string>();

        if (obj.TrackChanges)
        {
            foreach (var a in added)
                AddAutoHistory(obj, "Добавление СЗИ", $"Добавлено СЗИ ID: {a}");
            foreach (var r in removed)
                AddAutoHistory(obj, "Удаление СЗИ", $"Удалено СЗИ ID: {r}");
        }

        _audit.UpdateInformatizationObject(obj);
        return Json(new { success = true });
    }

    [HttpPost("{id}/set-skzi")]
    public IActionResult SetSkzi(string id, [FromBody] List<string> skziIds)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });

        var added = skziIds.Where(s => !obj.SkziIds.Contains(s)).ToList();
        var removed = obj.SkziIds.Where(s => !skziIds.Contains(s)).ToList();

        obj.SkziIds = skziIds ?? new List<string>();

        if (obj.TrackChanges)
        {
            foreach (var a in added)
                AddAutoHistory(obj, "Добавление СКЗИ", $"Добавлено СКЗИ ID: {a}");
            foreach (var r in removed)
                AddAutoHistory(obj, "Удаление СКЗИ", $"Удалено СКЗИ ID: {r}");
        }

        _audit.UpdateInformatizationObject(obj);
        return Json(new { success = true });
    }

    [HttpPost("{id}/set-telecom")]
    public IActionResult SetTelecom(string id, [FromBody] List<string> telecomIds)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });

        var added = telecomIds.Where(t => !obj.TelecomIds.Contains(t)).ToList();
        var removed = obj.TelecomIds.Where(t => !telecomIds.Contains(t)).ToList();

        obj.TelecomIds = telecomIds ?? new List<string>();

        if (obj.TrackChanges)
        {
            foreach (var a in added)
                AddAutoHistory(obj, "Добавление телеком", $"Добавлено телеком оборудование ID: {a}");
            foreach (var r in removed)
                AddAutoHistory(obj, "Удаление телеком", $"Удалено телеком оборудование ID: {r}");
        }

        _audit.UpdateInformatizationObject(obj);
        return Json(new { success = true });
    }

    [HttpPost("{id}/set-info-system")]
    public IActionResult SetInfoSystem(string id, [FromBody] List<string> infoSystemIds)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });

        var added = infoSystemIds.Where(i => !obj.InfoSystemIds.Contains(i)).ToList();
        var removed = obj.InfoSystemIds.Where(i => !infoSystemIds.Contains(i)).ToList();

        obj.InfoSystemIds = infoSystemIds ?? new List<string>();

        if (obj.TrackChanges)
        {
            foreach (var a in added)
                AddAutoHistory(obj, "Добавление ИС", $"Добавлена ИС ID: {a}");
            foreach (var r in removed)
                AddAutoHistory(obj, "Удаление ИС", $"Удалена ИС ID: {r}");
        }

        _audit.UpdateInformatizationObject(obj);
        return Json(new { success = true });
    }

    // ============================================================
    // Связи с ТС (одиночные операции — для совместимости)
    // ============================================================
    [HttpPost("{id}/add-technical-asset")]
    public IActionResult AddTechnicalAsset(string id, [FromBody] string assetId)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });
        if (!obj.TechnicalAssetIds.Contains(assetId))
        {
            obj.TechnicalAssetIds.Add(assetId);
            if (obj.TrackChanges) AddAutoHistory(obj, "Добавление ТС", "Добавлено техническое средство");
            _audit.UpdateInformatizationObject(obj);
        }
        return Json(new { success = true });
    }

    [HttpPost("{id}/remove-technical-asset")]
    public IActionResult RemoveTechnicalAsset(string id, [FromBody] string assetId)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });
        obj.TechnicalAssetIds.Remove(assetId);
        if (obj.TrackChanges) AddAutoHistory(obj, "Удаление ТС", "Удалено техническое средство");
        _audit.UpdateInformatizationObject(obj);
        return Json(new { success = true });
    }

    // ============================================================
    // Связи с ПО (одиночные операции)
    // ============================================================
    [HttpPost("{id}/add-software")]
    public IActionResult AddSoftware(string id, [FromBody] string softwareId)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });
        if (!obj.SoftwareIds.Contains(softwareId))
        {
            obj.SoftwareIds.Add(softwareId);
            if (obj.TrackChanges) AddAutoHistory(obj, "Добавление ПО", "Добавлено программное обеспечение");
            _audit.UpdateInformatizationObject(obj);
        }
        return Json(new { success = true });
    }

    [HttpPost("{id}/remove-software")]
    public IActionResult RemoveSoftware(string id, [FromBody] string softwareId)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });
        obj.SoftwareIds.Remove(softwareId);
        if (obj.TrackChanges) AddAutoHistory(obj, "Удаление ПО", "Удалено программное обеспечение");
        _audit.UpdateInformatizationObject(obj);
        return Json(new { success = true });
    }

    // ============================================================
    // Связи с СЗИ (одиночные операции)
    // ============================================================
    [HttpPost("{id}/add-szi")]
    public IActionResult AddSzi(string id, [FromBody] string sziId)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });
        if (!obj.SziIds.Contains(sziId))
        {
            obj.SziIds.Add(sziId);
            if (obj.TrackChanges) AddAutoHistory(obj, "Добавление СЗИ", "Добавлено средство защиты информации");
            _audit.UpdateInformatizationObject(obj);
        }
        return Json(new { success = true });
    }

    [HttpPost("{id}/remove-szi")]
    public IActionResult RemoveSzi(string id, [FromBody] string sziId)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });
        obj.SziIds.Remove(sziId);
        if (obj.TrackChanges) AddAutoHistory(obj, "Удаление СЗИ", "Удалено средство защиты информации");
        _audit.UpdateInformatizationObject(obj);
        return Json(new { success = true });
    }

    // ============================================================
    // Связи с СКЗИ (одиночные операции)
    // ============================================================
    [HttpPost("{id}/add-skzi")]
    public IActionResult AddSkzi(string id, [FromBody] string skziId)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });
        if (!obj.SkziIds.Contains(skziId))
        {
            obj.SkziIds.Add(skziId);
            if (obj.TrackChanges) AddAutoHistory(obj, "Добавление СКЗИ", "Добавлено СКЗИ");
            _audit.UpdateInformatizationObject(obj);
        }
        return Json(new { success = true });
    }

    [HttpPost("{id}/remove-skzi")]
    public IActionResult RemoveSkzi(string id, [FromBody] string skziId)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });
        obj.SkziIds.Remove(skziId);
        if (obj.TrackChanges) AddAutoHistory(obj, "Удаление СКЗИ", "Удалено СКЗИ");
        _audit.UpdateInformatizationObject(obj);
        return Json(new { success = true });
    }

    // ============================================================
    // Связи с телеком (одиночные операции)
    // ============================================================
    [HttpPost("{id}/add-telecom")]
    public IActionResult AddTelecom(string id, [FromBody] string telecomId)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });
        if (!obj.TelecomIds.Contains(telecomId))
        {
            obj.TelecomIds.Add(telecomId);
            if (obj.TrackChanges) AddAutoHistory(obj, "Добавление телеком", "Добавлено телекоммуникационное оборудование");
            _audit.UpdateInformatizationObject(obj);
        }
        return Json(new { success = true });
    }

    [HttpPost("{id}/remove-telecom")]
    public IActionResult RemoveTelecom(string id, [FromBody] string telecomId)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });
        obj.TelecomIds.Remove(telecomId);
        if (obj.TrackChanges) AddAutoHistory(obj, "Удаление телеком", "Удалено телекоммуникационное оборудование");
        _audit.UpdateInformatizationObject(obj);
        return Json(new { success = true });
    }

    // ============================================================
    // Связи с ИС (одиночные операции)
    // ============================================================
    [HttpPost("{id}/add-info-system")]
    public IActionResult AddInfoSystem(string id, [FromBody] string isId)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });
        if (!obj.InfoSystemIds.Contains(isId))
        {
            obj.InfoSystemIds.Add(isId);
            if (obj.TrackChanges) AddAutoHistory(obj, "Добавление ИС", "Добавлена информационная система");
            _audit.UpdateInformatizationObject(obj);
        }
        return Json(new { success = true });
    }

    [HttpPost("{id}/remove-info-system")]
    public IActionResult RemoveInfoSystem(string id, [FromBody] string isId)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });
        obj.InfoSystemIds.Remove(isId);
        if (obj.TrackChanges) AddAutoHistory(obj, "Удаление ИС", "Удалена информационная система");
        _audit.UpdateInformatizationObject(obj);
        return Json(new { success = true });
    }

    // ============================================================
    // История изменений
    // ============================================================
    [HttpPost("{id}/add-history")]
    public IActionResult AddHistory(string id, [FromBody] ChangeHistoryRecord record)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });
        if (string.IsNullOrEmpty(record.Id)) record.Id = Guid.NewGuid().ToString();
        record.Date = DateTime.Now;
        record.ChangedBy = User.Identity?.Name ?? "unknown";
        obj.ChangeHistory.Add(record);
        _audit.UpdateInformatizationObject(obj);
        return Json(new { success = true });
    }

    [HttpPost("{id}/update-history")]
    public IActionResult UpdateHistory(string id, [FromBody] ChangeHistoryRecord record)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });
        var existing = obj.ChangeHistory.FirstOrDefault(h => h.Id == record.Id);
        if (existing != null)
        {
            existing.Reason = record.Reason;
            existing.Description = record.Description;
            _audit.UpdateInformatizationObject(obj);
        }
        return Json(new { success = true });
    }

    [HttpPost("{id}/toggle-tracking")]
    public IActionResult ToggleTracking(string id, [FromBody] bool track)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });
        obj.TrackChanges = track;
        _audit.UpdateInformatizationObject(obj);
        return Json(new { success = true });
    }

    // ============================================================
    // Документы
    // ============================================================
    [HttpPost("{id}/generate-document")]
    public IActionResult GenerateDocument(string id, [FromBody] string documentType)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });
        var doc = new ObjectDocument
        {
            DocumentType = documentType,
            Name = GetDocumentTypeName(documentType) + " — " + obj.Name,
            Status = "draft",
            CreatedAt = DateTime.Now
        };
        obj.Documents.Add(doc);
        _audit.UpdateInformatizationObject(obj);
        return Json(new { success = true, documentId = doc.Id });
    }

    [HttpPost("{id}/generate-document/{documentId}")]
    public IActionResult GenerateSpecificDocument(string id, string documentId)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });

        var doc = obj.Documents.FirstOrDefault(d => d.Id == documentId);
        if (doc == null) return Json(new { success = false, error = "Документ не найден" });

        doc.GeneratedFileName = $"{doc.DocumentType}_{obj.Name.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf";
        doc.Status = "generated";

        if (obj.TrackChanges)
            AddAutoHistory(obj, "Формирование документа", $"Сформирован документ: {doc.Name}");

        _audit.UpdateInformatizationObject(obj);
        return Json(new { success = true });
    }

    [HttpPost("{id}/regenerate-document/{documentId}")]
    public IActionResult RegenerateDocument(string id, string documentId)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });

        var doc = obj.Documents.FirstOrDefault(d => d.Id == documentId);
        if (doc == null) return Json(new { success = false, error = "Документ не найден" });

        doc.ApprovedFileName = null;
        doc.ApprovedAt = null;
        doc.Status = "generated";
        doc.GeneratedFileName = $"{doc.DocumentType}_{obj.Name.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.pdf";

        if (obj.TrackChanges)
            AddAutoHistory(obj, "Переформирование документа", $"Переформирован документ: {doc.Name}");

        _audit.UpdateInformatizationObject(obj);
        return Json(new { success = true });
    }

    [HttpPost("{id}/upload-approved/{documentId}")]
    public async Task<IActionResult> UploadApprovedDocument(string id, string documentId)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });

        var doc = obj.Documents.FirstOrDefault(d => d.Id == documentId);
        if (doc == null) return Json(new { success = false, error = "Документ не найден" });

        if (Request.Form.Files.Count == 0)
            return Json(new { success = false, error = "Файл не загружен" });

        var file = Request.Form.Files[0];
        var uploadsDir = Path.Combine(_audit.GetDataDirectory(), "documents", id);
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{documentId}_{DateTime.Now:yyyyMMddHHmmss}_{file.FileName}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        doc.ApprovedFileName = file.FileName;
        doc.ApprovedAt = DateTime.Now;
        doc.Status = "approved";

        if (obj.TrackChanges)
            AddAutoHistory(obj, "Утверждение документа", $"Утверждён документ: {doc.Name}");

        _audit.UpdateInformatizationObject(obj);
        return Json(new { success = true });
    }

    [HttpGet("{id}/download-generated/{documentId}")]
    public IActionResult DownloadGeneratedDocument(string id, string documentId)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return NotFound();

        var doc = obj.Documents.FirstOrDefault(d => d.Id == documentId);
        if (doc == null || string.IsNullOrEmpty(doc.GeneratedFileName))
            return NotFound();

        var content = $"Документ: {doc.Name}\nТип: {doc.DocumentType}\nОбъект: {obj.Name}\nДата: {doc.CreatedAt:dd.MM.yyyy}\n\nЭтот документ сформирован автоматически.\nРаспечатайте, подпишите и загрузите утверждённую версию.";
        return File(System.Text.Encoding.UTF8.GetBytes(content), "application/pdf", doc.GeneratedFileName);
    }

    [HttpGet("{id}/download-approved/{documentId}")]
    public IActionResult DownloadApprovedDocument(string id, string documentId)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return NotFound();

        var doc = obj.Documents.FirstOrDefault(d => d.Id == documentId);
        if (doc == null || string.IsNullOrEmpty(doc.ApprovedFileName))
            return NotFound();

        var filePath = _audit.GetApprovedDocumentPath(id, documentId);
        if (filePath == null) return NotFound();

        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        return File(fileBytes, "application/octet-stream", doc.ApprovedFileName);
    }

    [HttpPost("{id}/approve-document")]
    public IActionResult ApproveDocument(string id, [FromBody] string documentId)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });
        var doc = obj.Documents.FirstOrDefault(d => d.Id == documentId);
        if (doc != null)
        {
            doc.Status = "approved";
            doc.ApprovedAt = DateTime.Now;
            _audit.UpdateInformatizationObject(obj);
        }
        return Json(new { success = true });
    }

    [HttpDelete("{id}/delete-document/{documentId}")]
    public IActionResult DeleteDocument(string id, string documentId)
    {
        var obj = _audit.GetInformatizationObject(id);
        if (obj == null) return Json(new { success = false, error = "Объект не найден" });
        obj.Documents.RemoveAll(d => d.Id == documentId);
        _audit.UpdateInformatizationObject(obj);
        return Json(new { success = true });
    }

    [HttpGet("export/{format}")]
    public IActionResult Export(string format, string? search = null)
    {
        return Json(new { success = true, message = $"Экспорт в {format.ToUpper()} будет реализован", format });
    }

    private List<InformatizationObject> FilterObjects(List<InformatizationObject> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(o =>
            o.Name.ToLower().Contains(s) ||
            o.InventoryNumber.ToLower().Contains(s) ||
            o.Location.ToLower().Contains(s) ||
            o.Classification.ToLower().Contains(s)
        ).ToList();
    }

    private void AddAutoHistory(InformatizationObject obj, string type, string description)
    {
        obj.ChangeHistory.Add(new ChangeHistoryRecord
        {
            ChangeType = type,
            Description = description,
            ChangedBy = User.Identity?.Name ?? "unknown",
            Date = DateTime.Now
        });
    }

    private string GetDocumentTypeName(string type) => type switch
    {
        "techpasport" => "Технический паспорт",
        "order" => "Приказ на ввод в эксплуатацию",
        "classification_act" => "Акт классификации",
        "security_level_act" => "Акт определения уровня защищённости",
        "tz" => "Техническое задание на СЗИ",
        "tp" => "Технический проект СЗИ",
        "tech_process" => "Описание технологического процесса",
        "access_system" => "Разрешительная система доступа",
        "threat_model" => "Модель угроз",
        _ => "Документ"
    };
}