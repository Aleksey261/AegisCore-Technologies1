using AegisCore.Models;
using AegisCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegisCore.Controllers.Audit;

[Authorize]
[Area("Audit")]
[Route("audit/infrastructure")]
public class InfrastructureController : Controller
{
    private readonly AuditService _audit;

    public InfrastructureController(AuditService audit)
    {
        _audit = audit;
    }

    [HttpGet("")]
    public IActionResult Index(string? section, string? search, int page = 1, string? sort = null, string? order = null, int? pageSize = null)
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "infrastructure";

        var model = new InfrastructureViewModel
        {
            ActiveTab = section ?? "szi",
            Search = search ?? "",
            Sort = sort,
            SortOrder = order,
            CurrentPage = page,
            PageSize = pageSize ?? 10,
            
            // Загружаем справочники
            Organizations = _audit.GetOrganizations(),
            Configurations = _audit.GetConfigurations(),
            CoverLetters = _audit.GetCoverLetters(),
            SkziNames = _audit.GetSkziNames(),
            TechnicalAssetTypes = _audit.GetTechnicalAssetTypes(),
            StorageTypes = _audit.GetStorageTypes(),
            IdentifierTypes = _audit.GetIdentifierTypes(),
            MediaTypes = _audit.GetMediaTypes(),
            SoftwareTypes = _audit.GetSoftwareTypes(),
            KeyInfoNumbers = _audit.GetKeyInfoNumbers(),
            OSIList = _audit.GetOSIList()
        };

        switch (model.ActiveTab)
        {
            case "info-systems":
                var infoSystems = FilterInfoSystems(_audit.GetInfoSystems(), search);
                model.TotalItems = infoSystems.Count;
                if (!string.IsNullOrEmpty(sort))
                {
                    infoSystems = sort.ToLower() switch
                    {
                        "name" => order == "desc" ? infoSystems.OrderByDescending(i => i.Name).ToList() : infoSystems.OrderBy(i => i.Name).ToList(),
                        "type" => order == "desc" ? infoSystems.OrderByDescending(i => i.Type).ToList() : infoSystems.OrderBy(i => i.Type).ToList(),
                        _ => infoSystems
                    };
                }
                model.InfoSystems = infoSystems.Skip((page - 1) * model.PageSize).Take(model.PageSize).ToList();
                break;

            case "storage":
                var storages = FilterStorages(_audit.GetStorages(), search);
                model.TotalItems = storages.Count;
                if (!string.IsNullOrEmpty(sort))
                {
                    storages = sort.ToLower() switch
                    {
                        "name" => order == "desc" ? storages.OrderByDescending(s => s.Name).ToList() : storages.OrderBy(s => s.Name).ToList(),
                        "type" => order == "desc" ? storages.OrderByDescending(s => s.Type).ToList() : storages.OrderBy(s => s.Type).ToList(),
                        _ => storages
                    };
                }
                model.Storages = storages.Skip((page - 1) * model.PageSize).Take(model.PageSize).ToList();
                break;

            case "technical-assets":
                var technicalAssets = FilterTechnicalAssets(_audit.GetTechnicalAssets(), search);
                model.TotalItems = technicalAssets.Count;
                if (!string.IsNullOrEmpty(sort))
                {
                    technicalAssets = sort.ToLower() switch
                    {
                        "name" => order == "desc" ? technicalAssets.OrderByDescending(t => t.Name).ToList() : technicalAssets.OrderBy(t => t.Name).ToList(),
                        "type" => order == "desc" ? technicalAssets.OrderByDescending(t => t.Type).ToList() : technicalAssets.OrderBy(t => t.Type).ToList(),
                        _ => technicalAssets
                    };
                }
                model.TechnicalAssets = technicalAssets.Skip((page - 1) * model.PageSize).Take(model.PageSize).ToList();
                break;

            case "szi":
                var szis = FilterSzi(_audit.GetSzi(), search);
                model.TotalItems = szis.Count;
                if (!string.IsNullOrEmpty(sort))
                {
                    szis = sort.ToLower() switch
                    {
                        "name" => order == "desc" ? szis.OrderByDescending(s => s.Name).ToList() : szis.OrderBy(s => s.Name).ToList(),
                        "serialnumber" => order == "desc" ? szis.OrderByDescending(s => s.SerialNumber).ToList() : szis.OrderBy(s => s.SerialNumber).ToList(),
                        "receiptdate" => order == "desc" ? szis.OrderByDescending(s => s.ReceiptDate).ToList() : szis.OrderBy(s => s.ReceiptDate).ToList(),
                        "installbydate" => order == "desc" ? szis.OrderByDescending(s => s.InstallByDate).ToList() : szis.OrderBy(s => s.InstallByDate).ToList(),
                        "receivedfrom" => order == "desc" ? szis.OrderByDescending(s => s.ReceivedFrom).ToList() : szis.OrderBy(s => s.ReceivedFrom).ToList(),
                        _ => szis
                    };
                }
                model.Szis = szis.Skip((page - 1) * model.PageSize).Take(model.PageSize).ToList();
                break;

            case "skzi":
                var skzis = FilterSkzi(_audit.GetSkzi(), search);
                model.TotalItems = skzis.Count;
                if (!string.IsNullOrEmpty(sort))
                {
                    skzis = sort.ToLower() switch
                    {
                        "name" => order == "desc" ? skzis.OrderByDescending(s => s.Name).ToList() : skzis.OrderBy(s => s.Name).ToList(),
                        _ => skzis
                    };
                }
                model.Skzis = skzis.Skip((page - 1) * model.PageSize).Take(model.PageSize).ToList();
                break;

            case "identifiers":
                var identifiers = FilterIdentifiers(_audit.GetIdentifiers(), search);
                model.TotalItems = identifiers.Count;
                if (!string.IsNullOrEmpty(sort))
                {
                    identifiers = sort.ToLower() switch
                    {
                        "name" => order == "desc" ? identifiers.OrderByDescending(i => i.Name).ToList() : identifiers.OrderBy(i => i.Name).ToList(),
                        _ => identifiers
                    };
                }
                model.Identifiers = identifiers.Skip((page - 1) * model.PageSize).Take(model.PageSize).ToList();
                break;

            case "signatures":
                var signatures = FilterSignatures(_audit.GetSignatures(), search);
                model.TotalItems = signatures.Count;
                if (!string.IsNullOrEmpty(sort))
                {
                    signatures = sort.ToLower() switch
                    {
                        "name" => order == "desc" ? signatures.OrderByDescending(s => s.KeyCarrier).ToList() : signatures.OrderBy(s => s.KeyCarrier).ToList(),
                        _ => signatures
                    };
                }
                model.Signatures = signatures.Skip((page - 1) * model.PageSize).Take(model.PageSize).ToList();
                break;

            case "media":
                var media = FilterMedia(_audit.GetMedia(), search);
                model.TotalItems = media.Count;
                if (!string.IsNullOrEmpty(sort))
                {
                    media = sort.ToLower() switch
                    {
                        "name" => order == "desc" ? media.OrderByDescending(m => m.Name).ToList() : media.OrderBy(m => m.Name).ToList(),
                        _ => media
                    };
                }
                model.Media = media.Skip((page - 1) * model.PageSize).Take(model.PageSize).ToList();
                break;

            case "software":
                var software = FilterSoftware(_audit.GetSoftware(), search);
                model.TotalItems = software.Count;
                if (!string.IsNullOrEmpty(sort))
                {
                    software = sort.ToLower() switch
                    {
                        "name" => order == "desc" ? software.OrderByDescending(s => s.Name).ToList() : software.OrderBy(s => s.Name).ToList(),
                        _ => software
                    };
                }
                model.Software = software.Skip((page - 1) * model.PageSize).Take(model.PageSize).ToList();
                break;

            case "asutp":
                var asutps = FilterAsutp(_audit.GetAsutp(), search);
                model.TotalItems = asutps.Count;
                if (!string.IsNullOrEmpty(sort))
                {
                    asutps = sort.ToLower() switch
                    {
                        "name" => order == "desc" ? asutps.OrderByDescending(a => a.Name).ToList() : asutps.OrderBy(a => a.Name).ToList(),
                        _ => asutps
                    };
                }
                model.Asutps = asutps.Skip((page - 1) * model.PageSize).Take(model.PageSize).ToList();
                break;

            case "its":
                var its = FilterIts(_audit.GetIts(), search);
                model.TotalItems = its.Count;
                if (!string.IsNullOrEmpty(sort))
                {
                    its = sort.ToLower() switch
                    {
                        "name" => order == "desc" ? its.OrderByDescending(i => i.Name).ToList() : its.OrderBy(i => i.Name).ToList(),
                        _ => its
                    };
                }
                model.Its = its.Skip((page - 1) * model.PageSize).Take(model.PageSize).ToList();
                break;
        }

        return View("~/Areas/Audit/Views/Infrastructure/Infrastructure.cshtml", model);
    }

    // ============================================================
    // СТРАНИЦА СОЗДАНИЯ ИНФОРМАЦИОННОЙ СИСТЕМЫ
    // ============================================================

    [HttpGet("create-info-system")]
    public IActionResult CreateInfoSystem()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "infrastructure";
        ViewBag.ShowCreateModal = true;

        var model = new InfrastructureViewModel
        {
            ActiveTab = "info-systems",
            InfoSystems = _audit.GetInfoSystems(),
            Organizations = _audit.GetOrganizations(),
            Configurations = _audit.GetConfigurations(),
            CoverLetters = _audit.GetCoverLetters(),
            SkziNames = _audit.GetSkziNames(),
            TechnicalAssetTypes = _audit.GetTechnicalAssetTypes(),
            StorageTypes = _audit.GetStorageTypes(),
            IdentifierTypes = _audit.GetIdentifierTypes(),
            MediaTypes = _audit.GetMediaTypes(),
            SoftwareTypes = _audit.GetSoftwareTypes(),
            KeyInfoNumbers = _audit.GetKeyInfoNumbers(),
            OSIList = _audit.GetOSIList()
        };

        return View("~/Areas/Audit/Views/Infrastructure/InfoSystems.cshtml", model);
    }

    // ============================================================
    // СТРАНИЦА СОЗДАНИЯ АСУ ТП
    // ============================================================

    [HttpGet("create-asutp")]
    public IActionResult CreateAsutp()
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "infrastructure";
        ViewBag.ShowCreateModal = true;

        var model = new InfrastructureViewModel
        {
            ActiveTab = "asutp",
            Asutps = _audit.GetAsutp()
        };

        return View("~/Areas/Audit/Views/Infrastructure/Asutp.cshtml", model);
    }

    // ============================================================
    // API методы для ИС
    // ============================================================

    [HttpPost("api/info-systems/save")]
    [IgnoreAntiforgeryToken]
    public IActionResult SaveInfoSystem([FromBody] InfoSystemDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return Json(new { success = false, error = "Наименование обязательно" });

            var infoSystems = _audit.GetInfoSystems();

            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                var newItem = new InfoSystem
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = dto.Name,
                    Type = dto.Type,
                    Role = dto.Role,
                    Purpose = dto.Purpose,
                    Goals = dto.Goals,
                    Tasks = dto.Tasks,
                    SecurityClass = dto.SecurityClass,
                    SecurityLevel = dto.SecurityLevel,
                    SignificanceLevel = dto.SignificanceLevel,
                    Scale = dto.Scale,
                    DataCategory = dto.DataCategory,
                    ProcessingType = dto.ProcessingType,
                    SubjectsCount = dto.SubjectsCount,
                    ThreatType = dto.ThreatType,
                    AttestationNumber = dto.AttestationNumber,
                    AttestationBody = dto.AttestationBody,
                    ComplianceStatus = dto.ComplianceStatus
                };

                if (!string.IsNullOrEmpty(dto.AttestationDate))
                    newItem.AttestationDate = DateTime.Parse(dto.AttestationDate);
                if (!string.IsNullOrEmpty(dto.AttestationValidUntil))
                    newItem.AttestationValidUntil = DateTime.Parse(dto.AttestationValidUntil);

                infoSystems.Add(newItem);
            }
            else
            {
                var item = infoSystems.FirstOrDefault(i => i.Id == dto.Id);
                if (item == null)
                    return Json(new { success = false, error = "Запись не найдена" });

                item.Name = dto.Name;
                item.Type = dto.Type;
                item.Role = dto.Role;
                item.Purpose = dto.Purpose;
                item.Goals = dto.Goals;
                item.Tasks = dto.Tasks;
                item.SecurityClass = dto.SecurityClass;
                item.SecurityLevel = dto.SecurityLevel;
                item.SignificanceLevel = dto.SignificanceLevel;
                item.Scale = dto.Scale;
                item.DataCategory = dto.DataCategory;
                item.ProcessingType = dto.ProcessingType;
                item.SubjectsCount = dto.SubjectsCount;
                item.ThreatType = dto.ThreatType;
                item.AttestationNumber = dto.AttestationNumber;
                item.AttestationBody = dto.AttestationBody;
                item.ComplianceStatus = dto.ComplianceStatus;

                if (!string.IsNullOrEmpty(dto.AttestationDate))
                    item.AttestationDate = DateTime.Parse(dto.AttestationDate);
                if (!string.IsNullOrEmpty(dto.AttestationValidUntil))
                    item.AttestationValidUntil = DateTime.Parse(dto.AttestationValidUntil);
            }

            _audit.SaveInfoSystems(infoSystems);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpDelete("api/info-systems/delete/{id}")]
    [IgnoreAntiforgeryToken]
    public IActionResult DeleteInfoSystem(string id)
    {
        try
        {
            var infoSystems = _audit.GetInfoSystems();
            var item = infoSystems.FirstOrDefault(i => i.Id == id);

            if (item == null)
                return Json(new { success = false, error = "Запись не найдена" });

            infoSystems.Remove(item);
            _audit.SaveInfoSystems(infoSystems);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    // ============================================================
    // API методы для АСУ ТП
    // ============================================================

    [HttpPost("api/asutp/save")]
    [IgnoreAntiforgeryToken]
    public IActionResult SaveAsutp([FromBody] AsutpDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return Json(new { success = false, error = "Наименование обязательно" });

            var asutps = _audit.GetAsutp();

            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                var newItem = new Asutp
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = dto.Name,
                    Type = dto.Type,
                    Location = dto.Location,
                    Description = dto.Description,
                    CommissioningDate = !string.IsNullOrEmpty(dto.CommissioningDate) ? DateTime.Parse(dto.CommissioningDate) : null
                };
                asutps.Add(newItem);
            }
            else
            {
                var item = asutps.FirstOrDefault(a => a.Id == dto.Id);
                if (item == null)
                    return Json(new { success = false, error = "Запись не найдена" });

                item.Name = dto.Name;
                item.Type = dto.Type;
                item.Location = dto.Location;
                item.Description = dto.Description;
                item.CommissioningDate = !string.IsNullOrEmpty(dto.CommissioningDate) ? DateTime.Parse(dto.CommissioningDate) : null;
            }

            _audit.SaveAsutp(asutps);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpDelete("api/asutp/delete/{id}")]
    [IgnoreAntiforgeryToken]
    public IActionResult DeleteAsutp(string id)
    {
        try
        {
            var asutps = _audit.GetAsutp();
            var item = asutps.FirstOrDefault(a => a.Id == id);
            if (item == null)
                return Json(new { success = false, error = "Запись не найдена" });

            asutps.Remove(item);
            _audit.SaveAsutp(asutps);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    // ============================================================
    // API методы для СЗИ
    // ============================================================

    [HttpPost("api/szi/save")]
    [IgnoreAntiforgeryToken]
    public IActionResult SaveSzi([FromBody] SziDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return Json(new { success = false, error = "Наименование обязательно" });

            var szis = _audit.GetSzi();

            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                var newItem = new Szi
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = dto.Name,
                    SerialNumber = dto.SerialNumber,
                    ConformityMark = dto.ConformityMark,
                    LicenseNumber = dto.LicenseNumber,
                    MaxInstallations = dto.MaxInstallations,
                    ReceivedFrom = dto.ReceivedFrom,
                    Configuration = dto.Configuration,
                    ReceiptDate = dto.ReceiptDate,
                    InstallByDate = dto.InstallByDate,
                    DestructionDate = dto.DestructionDate
                };
                szis.Add(newItem);
            }
            else
            {
                var item = szis.FirstOrDefault(i => i.Id == dto.Id);
                if (item == null)
                    return Json(new { success = false, error = "Запись не найдена" });

                item.Name = dto.Name;
                item.SerialNumber = dto.SerialNumber;
                item.ConformityMark = dto.ConformityMark;
                item.LicenseNumber = dto.LicenseNumber;
                item.MaxInstallations = dto.MaxInstallations;
                item.ReceivedFrom = dto.ReceivedFrom;
                item.Configuration = dto.Configuration;
                item.ReceiptDate = dto.ReceiptDate;
                item.InstallByDate = dto.InstallByDate;
                item.DestructionDate = dto.DestructionDate;
            }

            _audit.SaveSzi(szis);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpDelete("api/szi/delete/{id}")]
    [IgnoreAntiforgeryToken]
    public IActionResult DeleteSzi(string id)
    {
        try
        {
            var szis = _audit.GetSzi();
            var item = szis.FirstOrDefault(i => i.Id == id);
            if (item == null)
                return Json(new { success = false, error = "Запись не найдена" });

            szis.Remove(item);
            _audit.SaveSzi(szis);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    // ============================================================
    // API методы для СКЗИ
    // ============================================================

    [HttpPost("api/skzi/save")]
    [IgnoreAntiforgeryToken]
    public IActionResult SaveSkzi([FromBody] SkziDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return Json(new { success = false, error = "Наименование обязательно" });

            var skzis = _audit.GetSkzi();

            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                var newItem = new Skzi
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = dto.Name,
                    SerialNumber = dto.SerialNumber,
                    CertificateNumber = dto.CertificateNumber,
                    DistributionNumber = dto.DistributionNumber,
                    CopyNumber = dto.CopyNumber,
                    CoverLetter = dto.CoverLetter,
                    Owner = dto.Owner,
                    Configuration = dto.Configuration,
                    LicenseCount = dto.LicenseCount,
                    CertificateValidity = dto.CertificateValidity,
                    IssueDate = dto.IssueDate,
                    ReceiptDate = dto.ReceiptDate,
                };
                skzis.Add(newItem);
            }
            else
            {
                var item = skzis.FirstOrDefault(i => i.Id == dto.Id);
                if (item == null)
                    return Json(new { success = false, error = "Запись не найдена" });

                item.Name = dto.Name;
                item.SerialNumber = dto.SerialNumber;
                item.CertificateNumber = dto.CertificateNumber;
                item.DistributionNumber = dto.DistributionNumber;
                item.CopyNumber = dto.CopyNumber;
                item.CoverLetter = dto.CoverLetter;
                item.Owner = dto.Owner;
                item.Configuration = dto.Configuration;
                item.LicenseCount = dto.LicenseCount;
                item.CertificateValidity = dto.CertificateValidity;
                item.IssueDate = dto.IssueDate;
                item.ReceiptDate = dto.ReceiptDate;
            }

            _audit.SaveSkzi(skzis);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpDelete("api/skzi/delete/{id}")]
    [IgnoreAntiforgeryToken]
    public IActionResult DeleteSkzi(string id)
    {
        try
        {
            var skzis = _audit.GetSkzi();
            var item = skzis.FirstOrDefault(i => i.Id == id);
            if (item == null)
                return Json(new { success = false, error = "Запись не найдена" });

            skzis.Remove(item);
            _audit.SaveSkzi(skzis);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    // ============================================================
    // API методы для технических средств
    // ============================================================

    [HttpPost("api/technical-assets/save")]
    [IgnoreAntiforgeryToken]
    public IActionResult SaveTechnicalAsset([FromBody] TechnicalAssetDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return Json(new { success = false, error = "Наименование обязательно" });

            var assets = _audit.GetTechnicalAssets();

            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                var newItem = new TechnicalAsset
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = dto.Name,
                    Type = dto.Type,
                    InventoryNumber = dto.InventoryNumber,
                    SerialNumber = dto.SerialNumber,
                    PurchaseDate = !string.IsNullOrEmpty(dto.PurchaseDate) ? DateTime.Parse(dto.PurchaseDate) : null,
                    WriteOffDate = !string.IsNullOrEmpty(dto.WriteOffDate) ? DateTime.Parse(dto.WriteOffDate) : null
                };
                assets.Add(newItem);
            }
            else
            {
                var item = assets.FirstOrDefault(i => i.Id == dto.Id);
                if (item == null)
                    return Json(new { success = false, error = "Запись не найдена" });

                item.Name = dto.Name;
                item.Type = dto.Type;
                item.InventoryNumber = dto.InventoryNumber;
                item.SerialNumber = dto.SerialNumber;
                item.PurchaseDate = !string.IsNullOrEmpty(dto.PurchaseDate) ? DateTime.Parse(dto.PurchaseDate) : null;
                item.WriteOffDate = !string.IsNullOrEmpty(dto.WriteOffDate) ? DateTime.Parse(dto.WriteOffDate) : null;
            }

            _audit.SaveTechnicalAssets(assets);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpDelete("api/technical-assets/delete/{id}")]
    [IgnoreAntiforgeryToken]
    public IActionResult DeleteTechnicalAsset(string id)
    {
        try
        {
            var assets = _audit.GetTechnicalAssets();
            var item = assets.FirstOrDefault(i => i.Id == id);
            if (item == null)
                return Json(new { success = false, error = "Запись не найдена" });

            assets.Remove(item);
            _audit.SaveTechnicalAssets(assets);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    // ============================================================
    // API методы для хранилищ
    // ============================================================

    [HttpPost("api/storage/save")]
    [IgnoreAntiforgeryToken]
    public IActionResult SaveStorage([FromBody] StorageDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return Json(new { success = false, error = "Наименование обязательно" });

            var storages = _audit.GetStorages();

            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                var newItem = new Storage
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = dto.Name,
                    Type = dto.Type,
                    InventoryNumber = dto.InventoryNumber,
                    WarrantyYears = dto.WarrantyYears,
                    PurchaseDate = !string.IsNullOrEmpty(dto.PurchaseDate) ? DateTime.Parse(dto.PurchaseDate) : null,
                    WriteOffDate = !string.IsNullOrEmpty(dto.WriteOffDate) ? DateTime.Parse(dto.WriteOffDate) : null
                };
                storages.Add(newItem);
            }
            else
            {
                var item = storages.FirstOrDefault(i => i.Id == dto.Id);
                if (item == null)
                    return Json(new { success = false, error = "Запись не найдена" });

                item.Name = dto.Name;
                item.Type = dto.Type;
                item.InventoryNumber = dto.InventoryNumber;
                item.WarrantyYears = dto.WarrantyYears;
                item.PurchaseDate = !string.IsNullOrEmpty(dto.PurchaseDate) ? DateTime.Parse(dto.PurchaseDate) : null;
                item.WriteOffDate = !string.IsNullOrEmpty(dto.WriteOffDate) ? DateTime.Parse(dto.WriteOffDate) : null;
            }

            _audit.SaveStorages(storages);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpDelete("api/storage/delete/{id}")]
    [IgnoreAntiforgeryToken]
    public IActionResult DeleteStorage(string id)
    {
        try
        {
            var storages = _audit.GetStorages();
            var item = storages.FirstOrDefault(i => i.Id == id);
            if (item == null)
                return Json(new { success = false, error = "Запись не найдена" });

            storages.Remove(item);
            _audit.SaveStorages(storages);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    // ============================================================
    // API методы для идентификаторов
    // ============================================================

    [HttpPost("api/identifiers/save")]
    [IgnoreAntiforgeryToken]
    public IActionResult SaveIdentifier([FromBody] IdentifierDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return Json(new { success = false, error = "Наименование обязательно" });

            var identifiers = _audit.GetIdentifiers();

            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                var newItem = new Identifier
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = dto.Name,
                    Type = dto.Type,
                    SerialNumber = dto.SerialNumber,
                    PurchaseDate = !string.IsNullOrEmpty(dto.PurchaseDate) ? DateTime.Parse(dto.PurchaseDate) : null,
                    WriteOffDate = !string.IsNullOrEmpty(dto.WriteOffDate) ? DateTime.Parse(dto.WriteOffDate) : null
                };
                identifiers.Add(newItem);
            }
            else
            {
                var item = identifiers.FirstOrDefault(i => i.Id == dto.Id);
                if (item == null)
                    return Json(new { success = false, error = "Запись не найдена" });

                item.Name = dto.Name;
                item.Type = dto.Type;
                item.SerialNumber = dto.SerialNumber;
                item.PurchaseDate = !string.IsNullOrEmpty(dto.PurchaseDate) ? DateTime.Parse(dto.PurchaseDate) : null;
                item.WriteOffDate = !string.IsNullOrEmpty(dto.WriteOffDate) ? DateTime.Parse(dto.WriteOffDate) : null;
            }

            _audit.SaveIdentifiers(identifiers);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpDelete("api/identifiers/delete/{id}")]
    [IgnoreAntiforgeryToken]
    public IActionResult DeleteIdentifier(string id)
    {
        try
        {
            var identifiers = _audit.GetIdentifiers();
            var item = identifiers.FirstOrDefault(i => i.Id == id);
            if (item == null)
                return Json(new { success = false, error = "Запись не найдена" });

            identifiers.Remove(item);
            _audit.SaveIdentifiers(identifiers);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    // ============================================================
    // API методы для электронных подписей
    // ============================================================

    [HttpPost("api/signatures/save")]
    [IgnoreAntiforgeryToken]
    public IActionResult SaveSignature([FromBody] SignatureDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.KeyCarrier))
                return Json(new { success = false, error = "Ключевой носитель обязателен" });

            var signatures = _audit.GetSignatures();

            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                var newItem = new Signature
                {
                    Id = Guid.NewGuid().ToString(),
                    KeyCarrier = dto.KeyCarrier,
                    KeyInfoNumber = dto.KeyInfoNumber,
                    OwnerFIO = dto.OwnerFIO,
                    ReceiptDate = !string.IsNullOrEmpty(dto.ReceiptDate) ? DateTime.Parse(dto.ReceiptDate) : null,
                    DestructionDate = !string.IsNullOrEmpty(dto.DestructionDate) ? DateTime.Parse(dto.DestructionDate) : null
                };
                signatures.Add(newItem);
            }
            else
            {
                var item = signatures.FirstOrDefault(i => i.Id == dto.Id);
                if (item == null)
                    return Json(new { success = false, error = "Запись не найдена" });

                item.KeyCarrier = dto.KeyCarrier;
                item.KeyInfoNumber = dto.KeyInfoNumber;
                item.OwnerFIO = dto.OwnerFIO;
                item.ReceiptDate = !string.IsNullOrEmpty(dto.ReceiptDate) ? DateTime.Parse(dto.ReceiptDate) : null;
                item.DestructionDate = !string.IsNullOrEmpty(dto.DestructionDate) ? DateTime.Parse(dto.DestructionDate) : null;
            }

            _audit.SaveSignatures(signatures);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpDelete("api/signatures/delete/{id}")]
    [IgnoreAntiforgeryToken]
    public IActionResult DeleteSignature(string id)
    {
        try
        {
            var signatures = _audit.GetSignatures();
            var item = signatures.FirstOrDefault(i => i.Id == id);
            if (item == null)
                return Json(new { success = false, error = "Запись не найдена" });

            signatures.Remove(item);
            _audit.SaveSignatures(signatures);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    // ============================================================
    // API методы для машинных носителей
    // ============================================================

    [HttpPost("api/media/save")]
    [IgnoreAntiforgeryToken]
    public IActionResult SaveMedia([FromBody] MediaDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return Json(new { success = false, error = "Наименование обязательно" });

            var media = _audit.GetMedia();

            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                var newItem = new Media
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = dto.Name,
                    Type = dto.Type,
                    InventoryNumber = dto.InventoryNumber,
                    Capacity = dto.Capacity.HasValue ? dto.Capacity.Value.ToString() : null,
                    PurchaseDate = !string.IsNullOrEmpty(dto.PurchaseDate) ? DateTime.Parse(dto.PurchaseDate) : null,
                    WriteOffDate = !string.IsNullOrEmpty(dto.WriteOffDate) ? DateTime.Parse(dto.WriteOffDate) : null
                };
                media.Add(newItem);
            }
            else
            {
                var item = media.FirstOrDefault(i => i.Id == dto.Id);
                if (item == null)
                    return Json(new { success = false, error = "Запись не найдена" });

                item.Name = dto.Name;
                item.Type = dto.Type;
                item.InventoryNumber = dto.InventoryNumber;
                item.Capacity = dto.Capacity.HasValue ? dto.Capacity.Value.ToString() : null;
                item.PurchaseDate = !string.IsNullOrEmpty(dto.PurchaseDate) ? DateTime.Parse(dto.PurchaseDate) : null;
                item.WriteOffDate = !string.IsNullOrEmpty(dto.WriteOffDate) ? DateTime.Parse(dto.WriteOffDate) : null;
            }

            _audit.SaveMedia(media);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpDelete("api/media/delete/{id}")]
    [IgnoreAntiforgeryToken]
    public IActionResult DeleteMedia(string id)
    {
        try
        {
            var media = _audit.GetMedia();
            var item = media.FirstOrDefault(i => i.Id == id);
            if (item == null)
                return Json(new { success = false, error = "Запись не найдена" });

            media.Remove(item);
            _audit.SaveMedia(media);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    // ============================================================
    // API методы для ПО
    // ============================================================

    [HttpPost("api/software/save")]
    [IgnoreAntiforgeryToken]
    public IActionResult SaveSoftware([FromBody] SoftwareDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return Json(new { success = false, error = "Наименование обязательно" });

            var software = _audit.GetSoftware();

            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                var newItem = new Software
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = dto.Name,
                    Type = dto.Type,
                    Version = dto.Version,
                    LicenseType = dto.LicenseType,
                    LicenseNumber = dto.LicenseNumber,
                    Manufacturer = dto.Manufacturer,
                    InstallDate = dto.InstallDate
                };
                software.Add(newItem);
            }
            else
            {
                var item = software.FirstOrDefault(i => i.Id == dto.Id);
                if (item == null)
                    return Json(new { success = false, error = "Запись не найдена" });

                item.Name = dto.Name;
                item.Type = dto.Type;
                item.Version = dto.Version;
                item.LicenseType = dto.LicenseType;
                item.LicenseNumber = dto.LicenseNumber;
                item.Manufacturer = dto.Manufacturer;
                item.InstallDate = dto.InstallDate;
            }

            _audit.SaveSoftware(software);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpDelete("api/software/delete/{id}")]
    [IgnoreAntiforgeryToken]
    public IActionResult DeleteSoftware(string id)
    {
        try
        {
            var software = _audit.GetSoftware();
            var item = software.FirstOrDefault(i => i.Id == id);
            if (item == null)
                return Json(new { success = false, error = "Запись не найдена" });

            software.Remove(item);
            _audit.SaveSoftware(software);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    // ============================================================
    // API методы для ИТС
    // ============================================================

    [HttpPost("api/its/save")]
    [IgnoreAntiforgeryToken]
    public IActionResult SaveIts([FromBody] ItsDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return Json(new { success = false, error = "Наименование обязательно" });

            var its = _audit.GetIts();

            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                var newItem = new Its
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = dto.Name,
                    Type = dto.Type,
                    IpAddress = dto.IpAddress,
                    Description = dto.Description
                };
                its.Add(newItem);
            }
            else
            {
                var item = its.FirstOrDefault(i => i.Id == dto.Id);
                if (item == null)
                    return Json(new { success = false, error = "Запись не найдена" });

                item.Name = dto.Name;
                item.Type = dto.Type;
                item.IpAddress = dto.IpAddress;
                item.Description = dto.Description;
            }

            _audit.SaveIts(its);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpDelete("api/its/delete/{id}")]
    [IgnoreAntiforgeryToken]
    public IActionResult DeleteIts(string id)
    {
        try
        {
            var its = _audit.GetIts();
            var item = its.FirstOrDefault(i => i.Id == id);
            if (item == null)
                return Json(new { success = false, error = "Запись не найдена" });

            its.Remove(item);
            _audit.SaveIts(its);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    // ============================================================
    // API методы для справочников (организации, комплектации и т.д.)
    // ============================================================

    [HttpPost("api/organizations/save")]
    [IgnoreAntiforgeryToken]
    public IActionResult SaveOrganization([FromBody] OrganizationReferenceDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return Json(new { success = false, error = "Название обязательно" });

            var organizations = _audit.GetOrganizations();

            var newItem = new OrganizationReference
            {
                Id = Guid.NewGuid().ToString(),
                Name = dto.Name,
                FullName = dto.FullName,
                Address = dto.Address,
                Inn = dto.Inn,
                Phone = dto.Phone,
                Email = dto.Email
            };

            organizations.Add(newItem);
            _audit.SaveOrganizations(organizations);
            return Json(new { success = true, id = newItem.Id });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    // ============================================================
    // Вспомогательные методы фильтрации
    // ============================================================

    private List<InfoSystem> FilterInfoSystems(List<InfoSystem> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(i => i.Name.ToLower().Contains(s) || (i.Type?.ToLower().Contains(s) ?? false)).ToList();
    }

    private List<Storage> FilterStorages(List<Storage> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(i => i.Name.ToLower().Contains(s) || (i.Type?.ToLower().Contains(s) ?? false)).ToList();
    }

    private List<TechnicalAsset> FilterTechnicalAssets(List<TechnicalAsset> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(i => i.Name.ToLower().Contains(s) || (i.Type?.ToLower().Contains(s) ?? false)).ToList();
    }

    private List<Szi> FilterSzi(List<Szi> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(i =>
            i.Name.ToLower().Contains(s) ||
            (i.SerialNumber?.ToLower().Contains(s) ?? false) ||
            (i.ReceivedFrom?.ToLower().Contains(s) ?? false) ||
            (i.LicenseNumber?.ToLower().Contains(s) ?? false) ||
            (i.ConformityMark?.ToLower().Contains(s) ?? false)
        ).ToList();
    }

    private List<Skzi> FilterSkzi(List<Skzi> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(i => i.Name.ToLower().Contains(s)).ToList();
    }

    private List<Identifier> FilterIdentifiers(List<Identifier> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(i => i.Name.ToLower().Contains(s)).ToList();
    }

    private List<Signature> FilterSignatures(List<Signature> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(i => i.KeyCarrier.ToLower().Contains(s)).ToList();
    }

    private List<Media> FilterMedia(List<Media> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(i => i.Name.ToLower().Contains(s)).ToList();
    }

    private List<Software> FilterSoftware(List<Software> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(i => i.Name.ToLower().Contains(s)).ToList();
    }

    private List<Asutp> FilterAsutp(List<Asutp> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(i => i.Name.ToLower().Contains(s)).ToList();
    }

    private List<Its> FilterIts(List<Its> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(i => i.Name.ToLower().Contains(s)).ToList();
    }
}