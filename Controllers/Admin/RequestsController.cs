using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodTech.Data;
using FoodTech.ViewModels;
using FoodTech.Models;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace FoodTech.Controllers.Admin;

[Authorize(Roles = "Admin,Manager,User")]
[Route("admin/requests")]
public class RequestsController : Controller
{
    private readonly ApplicationDbContext _ctx;
    private readonly UserManager<ApplicationUser> _userManager;

    public RequestsController(ApplicationDbContext ctx, UserManager<ApplicationUser> userManager)
    {
        _ctx = ctx;
        _userManager = userManager;
    }

    // ===============================
    // 📋 Список запросов (только для Admin/Manager)
    // ===============================
    [HttpGet("")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Index()
    {
        var requests = await _ctx.ContactRequests
            .AsNoTracking()
            .Include(r => r.Equipment)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ContactRequestVm
            {
                ContactRequestId = r.ContactRequestId,
                EquipmentId = r.EquipmentId,
                EquipmentName = r.Equipment != null
                    ? r.Equipment.Name
                    : r.EquipmentName ?? string.Empty,
                Company = r.Company ?? string.Empty,
                Name = r.Name ?? string.Empty,
                Email = r.Email ?? string.Empty,
                Phone = r.Phone ?? string.Empty,
                Message = r.Message ?? string.Empty,
                Status = r.Status ?? string.Empty,
                CreatedAt = r.CreatedAt,
                SourceUrl = r.SourceUrl ?? string.Empty
            })
            .ToListAsync();

        return PartialView("~/Views/Admin/_RequestManagement.cshtml", requests);
    }

    // ===============================
    // 📋 Мои запросы (для User)
    // ===============================
    [HttpGet("my-requests")]
    public async Task<IActionResult> MyRequests()
    {
        var user = await GetCurrentUserAsync();
        if (user == null) return Unauthorized();

        // Получаем все запросы, связанные с оборудованием пользователя или его заводом
        var requests = await _ctx.ContactRequests
            .AsNoTracking()
            .Include(r => r.Equipment)
            .Where(r => r.Equipment != null)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                ContactRequest = r,
                Equipment = r.Equipment
            })
            .ToListAsync();

        // Фильтруем запросы в памяти
        var filteredRequests = requests
            .Where(x => x.Equipment != null && 
                       (x.Equipment.CreatedByUserId == user.Id || 
                        (user.ManufacturerId != null && x.Equipment.ManufacturerId == user.ManufacturerId)))
            .Select(x => new ContactRequestUserVm
            {
                ContactRequest = new ContactRequestVm
                {
                    ContactRequestId = x.ContactRequest.ContactRequestId,
                    EquipmentId = x.ContactRequest.EquipmentId,
                    EquipmentName = x.Equipment != null 
                        ? x.Equipment.Name 
                        : x.ContactRequest.EquipmentName ?? string.Empty,
                    Company = x.ContactRequest.Company ?? string.Empty,
                    Name = x.ContactRequest.Name ?? string.Empty,
                    Email = x.ContactRequest.Email ?? string.Empty,
                    Phone = x.ContactRequest.Phone ?? string.Empty,
                    Message = x.ContactRequest.Message ?? string.Empty,
                    Status = x.ContactRequest.Status ?? "Новый",
                    CreatedAt = x.ContactRequest.CreatedAt,
                    SourceUrl = x.ContactRequest.SourceUrl ?? string.Empty
                },
                IsMyEquipment = x.Equipment.CreatedByUserId == user.Id,
                IsMyManufacturer = user.ManufacturerId != null && 
                                   x.Equipment.ManufacturerId == user.ManufacturerId
            })
            .ToList();

        return PartialView("~/Views/User/_MyRequests.cshtml", filteredRequests);
    }

    // ===============================
    // 🔄 Обновление статуса
    // ===============================
    [HttpPost("update-status")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateStatus([FromBody] ContactRequestVm model)
    {
        if (model.ContactRequestId <= 0)
            return Json(new { success = false, message = "Некорректный ID запроса." });

        var entity = await _ctx.ContactRequests.FindAsync(model.ContactRequestId);
        if (entity == null)
            return Json(new { success = false, message = "Запрос не найден." });

        if (!string.IsNullOrWhiteSpace(model.Status))
            entity.Status = model.Status;

        await _ctx.SaveChangesAsync();

        return Json(new { success = true, message = "Статус обновлён." });
    }

    // ===============================
    // ❌ Удаление запроса
    // ===============================
    [HttpPost("delete")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete([FromBody] IdDto dto)
    {
        if (dto.Id <= 0)
            return Json(new { success = false, message = "Некорректный ID." });

        var entity = await _ctx.ContactRequests.FindAsync(dto.Id);
        if (entity == null)
            return Json(new { success = false, message = "Запрос не найден." });

        _ctx.ContactRequests.Remove(entity);
        await _ctx.SaveChangesAsync();

        return Json(new { success = true, message = "Запрос удалён." });
    }

    // ===============================
    // 🔍 Детали запроса (модалка)
    // ===============================
    [HttpGet("details/{id:long}")]
    public async Task<IActionResult> Details(long id)
    {
        var r = await _ctx.ContactRequests
            .AsNoTracking()
            .Include(x => x.Equipment)
            .FirstOrDefaultAsync(x => x.ContactRequestId == id);

        if (r == null)
            return NotFound();

        // Проверяем права доступа для User
        var user = await GetCurrentUserAsync();
        if (user != null && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            // Для User проверяем, что запрос относится к его оборудованию или заводу
            if (r.Equipment == null || 
                (r.Equipment.CreatedByUserId != user.Id && 
                 (user.ManufacturerId == null || r.Equipment.ManufacturerId != user.ManufacturerId)))
            {
                return Forbid();
            }
        }

        var vm = new ContactRequestVm
        {
            ContactRequestId = r.ContactRequestId,
            EquipmentId = r.EquipmentId,
            EquipmentName = r.Equipment?.Name ?? r.EquipmentName ?? string.Empty,
            Company = r.Company ?? string.Empty,
            Name = r.Name ?? string.Empty,
            Email = r.Email ?? string.Empty,
            Phone = r.Phone ?? string.Empty,
            Message = r.Message ?? string.Empty,
            Status = r.Status ?? string.Empty,
            CreatedAt = r.CreatedAt,
            SourceUrl = r.SourceUrl ?? string.Empty
        };

        return PartialView("~/Views/Admin/_RequestDetailsPartial.cshtml", vm);
    }

    // ===============================
    // 🏭 Список производителей
    // ===============================
    [HttpGet("manufacturers/list")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetManufacturersList()
    {
        try
        {
            var usersWithCompany = await _ctx.Users
                .AsNoTracking()
                .Where(u => !string.IsNullOrWhiteSpace(u.Company))
                .ToListAsync();

            var manufacturers = usersWithCompany
                .Select(u => new UserVm
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    Name = GetUserName(u),
                    Phone = u.PhoneNumber ?? string.Empty,
                    Company = u.Company ?? string.Empty,
                    ManufacturerId = u.ManufacturerId
                })
                .OrderBy(u => u.Company)
                .ThenBy(u => u.Name)
                .ToList();

            var uniqueCompanies = manufacturers
                .GroupBy(u => u.Company ?? "Без компании")
                .Select(g => new
                {
                    CompanyId = g.First().ManufacturerId ?? 0,
                    CompanyName = g.Key,
                    ContactPerson = g.First().Name,
                    ContactEmail = g.First().Email,
                    ContactPhone = g.First().Phone,
                    Users = g.Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        u.Email,
                        u.Name,
                        u.Phone
                    }).ToList()
                })
                .Where(c => !string.IsNullOrWhiteSpace(c.CompanyName) && c.CompanyName != "Без компании")
                .OrderBy(c => c.CompanyName)
                .ToList();

            return Json(uniqueCompanies);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка загрузки производителей: {ex.Message}");
            return Json(new List<object>());
        }
    }

    // ===============================
    // helper — безопасное имя пользователя
    // ===============================
    private string GetUserName(ApplicationUser user)
    {
        var firstName = user.GetType().GetProperty("FirstName")?.GetValue(user) as string;
        var lastName = user.GetType().GetProperty("LastName")?.GetValue(user) as string;

        if (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName))
            return $"{firstName} {lastName}".Trim();

        var nameProp = user.GetType().GetProperty("Name")?.GetValue(user) as string;
        if (!string.IsNullOrWhiteSpace(nameProp))
            return nameProp;

        return user.UserName ?? "Без имени";
    }

    // ===============================
    // 📤 Переслать производителю
    // ===============================
    [HttpPost("forward")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Forward([FromBody] ForwardDto dto)
    {
        if (dto.ContactRequestId <= 0)
            return Json(new { success = false, message = "Некорректный ID запроса." });

        var request = await _ctx.ContactRequests.FindAsync(dto.ContactRequestId);
        if (request == null)
            return Json(new { success = false, message = "Запрос не найден." });

        var manufacturer = await _ctx.Users
            .Where(u => u.ManufacturerId == dto.ManufacturerId)
            .FirstOrDefaultAsync();

        string manufacturerName = manufacturer?.Company ?? dto.ManufacturerName ?? "Производитель";
        string manufacturerEmail = manufacturer?.Email ?? dto.ManufacturerEmail ?? string.Empty;
        string contactPerson = manufacturer != null ? GetUserName(manufacturer) : "Не указано";

        // Логируем отправку
        var forwardLog = new
        {
            RequestId = dto.ContactRequestId,
            ManufacturerId = dto.ManufacturerId,
            ManufacturerName = manufacturerName,
            ManufacturerEmail = manufacturerEmail,
            ContactPerson = contactPerson,
            Note = dto.Note,
            ForwardedAt = DateTime.Now,
            ForwardedBy = User.Identity?.Name ?? "system"
        };

        Console.WriteLine($"📤 Пересылка запроса: {JsonSerializer.Serialize(forwardLog)}");

        return Json(new
        {
            success = true,
            message = $"Запрос переслан производителю: {manufacturerName}"
        });
    }

    // ===============================
    // ✉ Ответ клиенту
    // ===============================
    [HttpPost("reply")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Reply([FromBody] ReplyDto dto)
    {
        if (dto.ContactRequestId <= 0)
            return Json(new { success = false, message = "Некорректный ID." });

        var request = await _ctx.ContactRequests.FindAsync(dto.ContactRequestId);
        if (request == null)
            return Json(new { success = false, message = "Запрос не найден." });

        if (string.IsNullOrWhiteSpace(dto.Subject) || string.IsNullOrWhiteSpace(dto.Body))
            return Json(new { success = false, message = "Тема и текст ответа обязательны." });

        return Json(new { success = true, message = "Ответ отправлен клиенту." });
    }

    // ===============================
    // Получение текущего пользователя
    // ===============================
    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return null;

        return await _userManager.FindByIdAsync(userId);
    }
}

// ===============================
// DTO для AJAX
// ===============================
public class IdDto
{
    public long Id { get; set; }
}

public class ForwardDto
{
    public long ContactRequestId { get; set; }
    public long ManufacturerId { get; set; }
    public string? ManufacturerName { get; set; }
    public string? ManufacturerEmail { get; set; }
    public string? Note { get; set; }
}

public class ReplyDto
{
    public long ContactRequestId { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
}

// ===============================
// ViewModel для запросов пользователя
// ===============================
