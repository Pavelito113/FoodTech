using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodTech.Data;
using FoodTech.Models;

namespace FoodTech.Controllers.Admin;

[Authorize(Roles = "Admin,Manager,User")]
[Route("admin/categories")]
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _ctx;

    public CategoriesController(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var list = await _ctx.EquipmentCategories
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        return PartialView("~/Views/Admin/_EditCategories.cshtml", list);
    }

    // ---------------------------------------------------------------
    // ✅ Улучшенный метод Add — как просил
    // ---------------------------------------------------------------
    [HttpPost("add")]
    public async Task<IActionResult> Add(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Json(new
            {
                success = false,
                message = "Название не может быть пустым"
            });
        }

        name = name.Trim();

        // Проверка дубликатов
        bool exists = await _ctx.EquipmentCategories
            .AnyAsync(c => c.Name.ToLower() == name.ToLower());

        if (exists)
        {
            return Json(new
            {
                success = false,
                message = $"Категория «{name}» уже существует"
            });
        }

        var item = new EquipmentCategory
        {
            Name = name,
            Slug = name.ToLower().Replace(" ", "-"),
            SortOrder = await _ctx.EquipmentCategories.CountAsync() + 1
        };

        _ctx.EquipmentCategories.Add(item);
        await _ctx.SaveChangesAsync();

        return Json(new { success = true });
    }

    // ---------------------------------------------------------------

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _ctx.EquipmentCategories.FindAsync(id);
        if (item != null)
        {
            _ctx.EquipmentCategories.Remove(item);
            await _ctx.SaveChangesAsync();
            return Json(new { success = true });
        }

        return Json(new { success = false, message = "Категория не найдена" });
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(int id, string name)
    {
        var item = await _ctx.EquipmentCategories.FindAsync(id);
        if (item == null)
        {
            return Json(new { success = false, message = "Категория не найдена" });
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Json(new { success = false, message = "Название не может быть пустым" });
        }

        name = name.Trim();

        // Проверка дубликатов, исключая текущую запись
        bool duplicate = await _ctx.EquipmentCategories
            .AnyAsync(c => c.EquipmentCategoryId != id && c.Name.ToLower() == name.ToLower());

        if (duplicate)
        {
            return Json(new
            {
                success = false,
                message = $"Категория «{name}» уже существует"
            });
        }

        item.Name = name;
        item.Slug = name.ToLower().Replace(" ", "-");

        await _ctx.SaveChangesAsync();

        return Json(new { success = true });
    }
}
