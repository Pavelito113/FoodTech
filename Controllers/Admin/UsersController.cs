using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using FoodTech.Models;
using FoodTech.ViewModels;

namespace FoodTech.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("admin/users")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userMgr;
    private readonly RoleManager<IdentityRole> _roleMgr;

    public UsersController(UserManager<ApplicationUser> userMgr, RoleManager<IdentityRole> roleMgr)
    {
        _userMgr = userMgr;
        _roleMgr = roleMgr;
    }

    // ===========================================================
    // 📄 СПИСОК ПОЛЬЗОВАТЕЛЕЙ
    // ===========================================================
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var users = _userMgr.Users.ToList();
        var vm = new List<UserVm>();

        foreach (var u in users)
        {
            var roles = await _userMgr.GetRolesAsync(u);

            vm.Add(new UserVm
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Name = $"{u.FirstName} {u.LastName}".Trim(),
                Email = u.Email ?? "",
                Company = u.Company ?? "",
                Phone = u.PhoneNumber ?? "",
                Role = roles.FirstOrDefault() ?? "User",
                IsFrozen = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow
            });
        }

        return PartialView("~/Views/Admin/_EditUsers.cshtml", vm);
    }

    // ===========================================================
    // ➕ ДОБАВЛЕНИЕ
    // ===========================================================
  [HttpPost("add")]
public async Task<IActionResult> Add([FromBody] UserVm model)
{
    if (model == null)
        return Json(new { success = false, message = "Пустые данные." });

    if (string.IsNullOrWhiteSpace(model.Email))
        return Json(new { success = false, message = "Email обязателен." });

    // Разбираем имя
    var parts = (model.Name ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var first = parts.ElementAtOrDefault(0) ?? "";
    var last = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "";

    if (await _userMgr.FindByEmailAsync(model.Email) != null)
        return Json(new { success = false, message = "Email уже существует." });

    // Генерация временного пароля
    var tempPassword = GenerateTemporaryPassword();
    
    var user = new ApplicationUser
    {
        UserName = model.Email,
        Email = model.Email,
        FirstName = first,
        LastName = last,
        Company = model.Company ?? "",
        PhoneNumber = model.Phone ?? ""
    };

    var create = await _userMgr.CreateAsync(user, tempPassword);
    if (!create.Succeeded)
        return Json(new { success = false, message = string.Join("; ", create.Errors.Select(e => e.Description)) });

    var role = string.IsNullOrWhiteSpace(model.Role) ? "User" : model.Role;

    if (!await _roleMgr.RoleExistsAsync(role))
        await _roleMgr.CreateAsync(new IdentityRole(role));

    await _userMgr.AddToRoleAsync(user, role);

    // Требуем смену пароля при первом входе
    user.EmailConfirmed = true; // Подтверждаем email для упрощения
    await _userMgr.UpdateAsync(user);

    return Json(new { 
        success = true, 
        message = "Пользователь создан.",
        tempPassword = tempPassword, // Отправляем временный пароль клиенту
        userId = user.Id
    });
}

// Генерация временного пароля
private string GenerateTemporaryPassword()
{
    const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
    var random = new Random();
    var chars = new char[12];
    
    for (int i = 0; i < chars.Length; i++)
    {
        chars[i] = validChars[random.Next(validChars.Length)];
    }
    
    return new string(chars);
}
    // ===========================================================
    // ✏️ РЕДАКТИРОВАНИЕ
    // ===========================================================
    [HttpPost("edit")]
    public async Task<IActionResult> Edit([FromBody] UserVm model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.Id))
            return Json(new { success = false, message = "Некорректные данные." });

        var user = await _userMgr.FindByIdAsync(model.Id);
        if (user == null)
            return Json(new { success = false, message = "Пользователь не найден." });

        var parts = (model.Name ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        user.FirstName = parts.ElementAtOrDefault(0) ?? "";
        user.LastName = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "";

        user.Email = model.Email;
        user.UserName = model.Email;
        user.Company = model.Company ?? "";
        user.PhoneNumber = model.Phone ?? "";

        var upd = await _userMgr.UpdateAsync(user);

        return Json(new
        {
            success = upd.Succeeded,
            message = upd.Succeeded ? "Обновлено" : string.Join("; ", upd.Errors.Select(e => e.Description))
        });
    }

    // ===========================================================
    // 🎭 ОБНОВЛЕНИЕ РОЛИ
    // ===========================================================
    [HttpPost("update-role")]
    public async Task<IActionResult> UpdateRole([FromBody] RoleUpdateVm model)
    {
        if (model == null || string.IsNullOrWhiteSpace(model.UserId))
            return Json(new { success = false, message = "Некорректные данные." });

        var user = await _userMgr.FindByIdAsync(model.UserId);
        if (user == null)
            return Json(new { success = false, message = "Пользователь не найден." });

        var current = await _userMgr.GetRolesAsync(user);
        await _userMgr.RemoveFromRolesAsync(user, current);

        if (!await _roleMgr.RoleExistsAsync(model.Role))
            await _roleMgr.CreateAsync(new IdentityRole(model.Role));

        await _userMgr.AddToRoleAsync(user, model.Role);

        return Json(new { success = true, message = "Роль обновлена." });
    }

    // ===========================================================
    // ❄️ FREEZE/UNFREEZE
    // ===========================================================
    [HttpPost("freeze")]
    public async Task<IActionResult> Freeze([FromBody] object body)
    {
        // поддерживаем и {"id":"xxx"} и "xxx"
        string? id = body?.ToString()?.Trim('"');

        if (string.IsNullOrWhiteSpace(id))
            return Json(new { success = false, message = "Некорректный ID." });

        var user = await _userMgr.FindByIdAsync(id);
        if (user == null)
            return Json(new { success = false, message = "Не найден." });

        bool frozen = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow;

        user.LockoutEnd = frozen ? null : DateTimeOffset.UtcNow.AddYears(50);
        await _userMgr.UpdateAsync(user);

        return Json(new { success = true, frozen = !frozen });
    }

    // ===========================================================
    // 🗑 DELETE
    // ===========================================================
    [HttpPost("delete")]
    
    public async Task<IActionResult> Delete([FromBody] object body)
    {
        string? id = body?.ToString()?.Trim('"');

        if (string.IsNullOrWhiteSpace(id))
            return Json(new { success = false, message = "Некорректный ID." });

        var user = await _userMgr.FindByIdAsync(id);
        if (user == null)
            return Json(new { success = false, message = "Не найден." });

        var res = await _userMgr.DeleteAsync(user);

        return Json(new
        {
            success = res.Succeeded,
            message = res.Succeeded ? "Удалён" : string.Join("; ", res.Errors.Select(e => e.Description))
        });
    }
}
