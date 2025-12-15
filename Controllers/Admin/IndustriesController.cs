using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodTech.Data;
using FoodTech.Models;

namespace FoodTech.Controllers.Admin;

[Authorize(Roles = "Admin,Manager,User")]
[Route("admin/industries")]
public class IndustriesController : Controller
{
    private readonly ApplicationDbContext _ctx;

    public IndustriesController(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }

   [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        // ИЗМЕНИТЕ: возвращаем правильный partial view для отраслей
        var list = await _ctx.Industries.OrderBy(i => i.SortOrder).ToListAsync();
        return PartialView("~/Views/Admin/_EditIndustries.cshtml", list);
    }
[HttpPost("add")]
public async Task<IActionResult> Add(string name)
{
    var item = new Industry { 
        Name = name, 
        Slug = name.ToLower().Replace(" ", "-") 
    };

    _ctx.Industries.Add(item);
    await _ctx.SaveChangesAsync();

    return Json(new { success = true, id = item.IndustryId });
}


    [HttpPost("delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _ctx.Industries.FindAsync(id);
        if (item != null)
        {
            _ctx.Industries.Remove(item);
            await _ctx.SaveChangesAsync();
            return Json(new { success = true });
        }
        return Json(new { success = false, message = "Отрасль не найдена" });
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update(int id, string name)
    {
        var item = await _ctx.Industries.FindAsync(id);
        if (item == null) return Json(new { success = false, message = "Отрасль не найдена" });

        item.Name = name;
        item.Slug = name.ToLower().Replace(" ", "-");
        
        await _ctx.SaveChangesAsync();
        return Json(new { success = true });
    }
}