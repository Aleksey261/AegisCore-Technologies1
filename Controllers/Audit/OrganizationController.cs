using AegisCore.Models;
using AegisCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegisCore.Controllers.Audit;

[Authorize]
[Area("Audit")]
[Route("audit/organization")]
public class OrganizationController : Controller
{
    private readonly AuditService _audit;

    public OrganizationController(AuditService audit)
    {
        _audit = audit;
    }

    [HttpGet("")]
    public IActionResult Index(string? section, string? search, int page = 1, string? sort = null, string? order = null, int? pageSize = null)
    {
        ViewBag.ActiveModule = "audit";
        ViewBag.ActiveSection = "organization";
        var target = section ?? "general";

        var model = new OrganizationViewModel
        {
            ActiveTab = target,
            Search = search ?? "",
            Sort = sort,
            SortOrder = order,
            CurrentPage = page,
            PageSize = pageSize ?? 10,
            Organization = _audit.GetOrganization()
        };

        switch (target)
        {
            case "departments":
                var departments = FilterDepartments(_audit.GetDepartments(), search);
                model.TotalItems = departments.Count;
                if (!string.IsNullOrEmpty(sort))
                {
                    departments = sort.ToLower() switch
                    {
                        "name" => order == "desc" ? departments.OrderByDescending(d => d.Name).ToList() : departments.OrderBy(d => d.Name).ToList(),
                        _ => departments
                    };
                }
                model.Departments = departments.Skip((page - 1) * model.PageSize).Take(model.PageSize).ToList();
                break;

            case "rooms":
                var rooms = FilterRooms(_audit.GetRooms(), search);
                model.TotalItems = rooms.Count;
                if (!string.IsNullOrEmpty(sort))
                {
                    rooms = sort.ToLower() switch
                    {
                        "number" => order == "desc" ? rooms.OrderByDescending(r => r.Number).ToList() : rooms.OrderBy(r => r.Number).ToList(),
                        "description" => order == "desc" ? rooms.OrderByDescending(r => r.Description).ToList() : rooms.OrderBy(r => r.Description).ToList(),
                        "department" => order == "desc" ? rooms.OrderByDescending(r => r.Department).ToList() : rooms.OrderBy(r => r.Department).ToList(),
                        _ => rooms
                    };
                }
                model.Rooms = rooms.Skip((page - 1) * model.PageSize).Take(model.PageSize).ToList();
                break;

            case "employees":
                var employees = FilterEmployees(_audit.GetEmployees(), search);
                model.TotalItems = employees.Count;
                if (!string.IsNullOrEmpty(sort))
                {
                    employees = sort.ToLower() switch
                    {
                        "fio" => order == "desc" ? employees.OrderByDescending(e => e.Fio).ToList() : employees.OrderBy(e => e.Fio).ToList(),
                        "position" => order == "desc" ? employees.OrderByDescending(e => e.Position).ToList() : employees.OrderBy(e => e.Position).ToList(),
                        "department" => order == "desc" ? employees.OrderByDescending(e => e.Department).ToList() : employees.OrderBy(e => e.Department).ToList(),
                        "datestart" => order == "desc" ? employees.OrderByDescending(e => e.DateStart).ToList() : employees.OrderBy(e => e.DateStart).ToList(),
                        "dateend" => order == "desc" ? employees.OrderByDescending(e => e.DateEnd).ToList() : employees.OrderBy(e => e.DateEnd).ToList(),
                        _ => employees
                    };
                }
                model.Employees = employees.Skip((page - 1) * model.PageSize).Take(model.PageSize).ToList();
                break;

            case "access":
                var accesses = FilterAccesses(_audit.GetAccesses(), search);
                model.TotalItems = accesses.Count;
                if (!string.IsNullOrEmpty(sort))
                {
                    accesses = sort.ToLower() switch
                    {
                        "employeefio" => order == "desc" ? accesses.OrderByDescending(a => a.EmployeeFio).ToList() : accesses.OrderBy(a => a.EmployeeFio).ToList(),
                        _ => accesses
                    };
                }
                model.Accesses = accesses.Skip((page - 1) * model.PageSize).Take(model.PageSize).ToList();
                break;

            case "branches":
                var branches = FilterBranches(_audit.GetBranches(), search);
                model.TotalItems = branches.Count;
                if (!string.IsNullOrEmpty(sort))
                {
                    branches = sort.ToLower() switch
                    {
                        "name" => order == "desc" ? branches.OrderByDescending(b => b.Name).ToList() : branches.OrderBy(b => b.Name).ToList(),
                        "address" => order == "desc" ? branches.OrderByDescending(b => b.Address).ToList() : branches.OrderBy(b => b.Address).ToList(),
                        "director" => order == "desc" ? branches.OrderByDescending(b => b.Director).ToList() : branches.OrderBy(b => b.Director).ToList(),
                        _ => branches
                    };
                }
                model.Branches = branches.Skip((page - 1) * model.PageSize).Take(model.PageSize).ToList();
                break;
        }

        return View("~/Areas/Audit/Views/Organization/Organization.cshtml", model);
    }

    // ============================================================
    // ОБЩИЕ СВЕДЕНИЯ
    // ============================================================

    [HttpPost("save-general")]
    public IActionResult SaveGeneral(OrganizationData org)
    {
        if (string.IsNullOrWhiteSpace(org.FullName))
        {
            TempData["Error"] = "Полное наименование обязательно";
            return RedirectToAction("Index", new { section = "general" });
        }
        _audit.SaveOrganization(org);
        _audit.LogActivity(new AuditActivity
        {
            Event = "Обновление общих сведений об организации",
            User = User.Identity?.Name ?? "unknown",
            Status = "OK"
        });
        TempData["Success"] = "Общие сведения успешно сохранены";
        return RedirectToAction("Index", new { section = "general" });
    }

    // ============================================================
    // ПОДРАЗДЕЛЕНИЯ - API
    // ============================================================

    [HttpPost("departments/save")]
    public IActionResult SaveDepartment([FromBody] Department dept)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dept.Name))
            {
                return Json(new { success = false, error = "Наименование обязательно" });
            }

            if (string.IsNullOrEmpty(dept.Id))
            {
                dept.Id = Guid.NewGuid().ToString();
                _audit.AddDepartment(dept);
            }
            else
            {
                _audit.UpdateDepartment(dept);
            }

            _audit.LogActivity(new AuditActivity
            {
                Event = string.IsNullOrEmpty(dept.Id) ? "Добавление подразделения" : "Редактирование подразделения",
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

    [HttpDelete("departments/delete/{id}")]
    public IActionResult DeleteDepartment(string id)
    {
        try
        {
            // Проверка использования подразделения
            var employees = _audit.GetEmployees();
            if (employees.Any(e => e.Department == id))
            {
                return Json(new { success = false, error = "Подразделение используется сотрудниками и не может быть удалено" });
            }

            _audit.DeleteDepartment(id);
            _audit.LogActivity(new AuditActivity
            {
                Event = "Удаление подразделения",
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
    // ПОМЕЩЕНИЯ - API
    // ============================================================

    [HttpPost("rooms/save")]
    public IActionResult SaveRoom([FromBody] Room room)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(room.Number))
            {
                return Json(new { success = false, error = "Номер помещения обязателен" });
            }

            if (string.IsNullOrEmpty(room.Id))
            {
                room.Id = Guid.NewGuid().ToString();
                _audit.AddRoom(room);
            }
            else
            {
                _audit.UpdateRoom(room);
            }

            _audit.LogActivity(new AuditActivity
            {
                Event = string.IsNullOrEmpty(room.Id) ? "Добавление помещения" : "Редактирование помещения",
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

    [HttpDelete("rooms/delete/{id}")]
    public IActionResult DeleteRoom(string id)
    {
        try
        {
            _audit.DeleteRoom(id);
            _audit.LogActivity(new AuditActivity
            {
                Event = "Удаление помещения",
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

    [HttpPost("rooms/import")]
    public IActionResult ImportRooms(IFormFile file)
    {
        try
        {
            // TODO: Реализовать импорт из Excel
            return Json(new { success = true, message = "Импорт будет реализован позже", count = 0 });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    // ============================================================
    // СОТРУДНИКИ - API
    // ============================================================

    [HttpPost("employees/save")]
    public IActionResult SaveEmployee([FromBody] Employee employee)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(employee.Fio))
            {
                return Json(new { success = false, error = "ФИО обязательно" });
            }

            if (string.IsNullOrEmpty(employee.Id))
            {
                employee.Id = Guid.NewGuid().ToString();
                _audit.AddEmployee(employee);
            }
            else
            {
                _audit.UpdateEmployee(employee);
            }

            _audit.LogActivity(new AuditActivity
            {
                Event = string.IsNullOrEmpty(employee.Id) ? "Добавление сотрудника" : "Редактирование сотрудника",
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

    [HttpDelete("employees/delete/{id}")]
    public IActionResult DeleteEmployee(string id)
    {
        try
        {
            // Проверка использования сотрудника
            var accesses = _audit.GetAccesses();
            if (accesses.Any(a => a.EmployeeId == id))
            {
                return Json(new { success = false, error = "Сотрудник имеет назначенные доступы и не может быть удалён" });
            }

            _audit.DeleteEmployee(id);
            _audit.LogActivity(new AuditActivity
            {
                Event = "Удаление сотрудника",
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

    [HttpPost("employees/import")]
    public IActionResult ImportEmployees(IFormFile file)
    {
        try
        {
            // TODO: Реализовать импорт из Excel
            return Json(new { success = true, message = "Импорт будет реализован позже", count = 0 });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    // ============================================================
    // ДОСТУПЫ - API
    // ============================================================

    [HttpPost("access/save")]
    public IActionResult SaveAccess([FromBody] AccessRecord access)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(access.EmployeeFio))
            {
                return Json(new { success = false, error = "ФИО сотрудника обязательно" });
            }

            if (string.IsNullOrEmpty(access.Id))
            {
                access.Id = Guid.NewGuid().ToString();
                _audit.AddAccess(access);
            }
            else
            {
                _audit.UpdateAccess(access);
            }

            _audit.LogActivity(new AuditActivity
            {
                Event = string.IsNullOrEmpty(access.Id) ? "Добавление доступа" : "Редактирование доступа",
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

    [HttpDelete("access/delete/{id}")]
    public IActionResult DeleteAccess(string id)
    {
        try
        {
            _audit.DeleteAccess(id);
            _audit.LogActivity(new AuditActivity
            {
                Event = "Удаление доступа",
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
    // ФИЛИАЛЫ - API
    // ============================================================

    [HttpPost("branches/save")]
    public IActionResult SaveBranch([FromBody] Branch branch)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(branch.Name))
            {
                return Json(new { success = false, error = "Наименование обязательно" });
            }

            if (string.IsNullOrEmpty(branch.Id))
            {
                branch.Id = Guid.NewGuid().ToString();
                _audit.AddBranch(branch);
            }
            else
            {
                _audit.UpdateBranch(branch);
            }

            _audit.LogActivity(new AuditActivity
            {
                Event = string.IsNullOrEmpty(branch.Id) ? "Добавление филиала" : "Редактирование филиала",
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

    [HttpDelete("branches/delete/{id}")]
    public IActionResult DeleteBranch(string id)
    {
        try
        {
            _audit.DeleteBranch(id);
            _audit.LogActivity(new AuditActivity
            {
                Event = "Удаление филиала",
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
    // ФИЛЬТРАЦИЯ
    // ============================================================

    private List<Department> FilterDepartments(List<Department> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(d => d.Name.ToLower().Contains(s)).ToList();
    }

    private List<Room> FilterRooms(List<Room> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(r =>
            r.Number.ToLower().Contains(s) ||
            (r.Description?.ToLower().Contains(s) ?? false) ||
            (r.Department?.ToLower().Contains(s) ?? false)
        ).ToList();
    }

    private List<Employee> FilterEmployees(List<Employee> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(e =>
            e.Fio.ToLower().Contains(s) ||
            (e.Position?.ToLower().Contains(s) ?? false) ||
            (e.Department?.ToLower().Contains(s) ?? false)
        ).ToList();
    }

    private List<AccessRecord> FilterAccesses(List<AccessRecord> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(a => a.EmployeeFio.ToLower().Contains(s)).ToList();
    }

    private List<Branch> FilterBranches(List<Branch> list, string? search)
    {
        if (string.IsNullOrEmpty(search)) return list;
        var s = search.ToLower();
        return list.Where(b =>
            b.Name.ToLower().Contains(s) ||
            (b.Address?.ToLower().Contains(s) ?? false) ||
            (b.Director?.ToLower().Contains(s) ?? false)
        ).ToList();
    }
}