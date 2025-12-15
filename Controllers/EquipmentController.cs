using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodTech.Data;
using FoodTech.Models;
using FoodTech.ViewModels;
using FoodTech.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FoodTech.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class EquipmentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly EquipmentService _service;

    public EquipmentController(ApplicationDbContext context, IWebHostEnvironment env, EquipmentService service)
    {
        _context = context;
        _env = env;
        _service = service;
    }

    // ========= МОЁ ОБОРУДОВАНИЕ =========
    public async Task<IActionResult> MyEquipment()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Unauthorized();

        var list = await _context.Equipments
         .AsSplitQuery() 
            .Include(e => e.Images)
            .Include(e => e.Industry)
            .Include(e => e.Category)
            .Where(e => e.CreatedByUserId == user.Id)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new EquipmentListItemVm
            {
                EquipmentId = e.EquipmentId,
                Name = e.Name,
                ShortDescription = e.ShortDescription ?? "",
                ImageUrl = e.Images.FirstOrDefault(i => i.IsPrimary) != null
                    ? e.Images.First(i => i.IsPrimary).Url
                    : "/img/no-image.png",
                IndustryName = e.Industry!.Name,
                CategoryName = e.Category!.Name
            })
            .ToListAsync();

        return View(list);
    }

    // ========= ДЕТАЛИ =========
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id, int page = 1)
    {
        var vm = await _service.GetEquipmentDetailsAsync(id);
        if (vm is null) return NotFound();

        vm.ReturnPage = page;
        return View(vm);
    }

    // ========= СОЗДАНИЕ =========
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new EquipmentEditVm();
        await PopulateLookups(vm);
        return View("Edit", vm);
    }

    [HttpPost]
    public async Task<IActionResult> Create(EquipmentEditVm vm, List<IFormFile>? uploadedImages)
        => await SaveEquipment(vm, uploadedImages, isNew: true);

    // ========= РЕДАКТИРОВАНИЕ =========
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Unauthorized();

        var e = await _context.Equipments
         .AsSplitQuery() 
            .Include(x => x.Images)
            .Include(x => x.Specs)
            .FirstOrDefaultAsync(x => x.EquipmentId == id && x.CreatedByUserId == user.Id);

        if (e is null) return NotFound();

        var vm = new EquipmentEditVm
        {
            EquipmentId = e.EquipmentId,
            Name = e.Name,
            Model = e.Model ?? "",
            ShortDescription = e.ShortDescription ?? "",
            Description = e.Description ?? "",
            IndustryId = e.IndustryId,
            EquipmentCategoryId = e.EquipmentCategoryId,
            ManufacturerId = e.ManufacturerId,
            IsPublished = e.IsPublished,
            Specs = e.Specs.OrderBy(s => s.SortOrder).Select(s => new EquipmentSpecVm
            {
                EquipmentSpecId = s.EquipmentSpecId,
                SpecKey = s.SpecKey,
                SpecValue = s.SpecValue ?? "",
                Unit = s.Unit ?? "",
                SortOrder = s.SortOrder
            }).ToList(),
            Images = e.Images.OrderBy(i => i.SortOrder).Select(i => new EquipmentImageVm
            {
                EquipmentImageId = i.EquipmentImageId,
                Url = i.Url,
                IsPrimary = i.IsPrimary,
                SortOrder = i.SortOrder,
                AltText = i.AltText
            }).ToList()
        };

        await PopulateLookups(vm);
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EquipmentEditVm vm, List<IFormFile>? uploadedImages)
        => await SaveEquipment(vm, uploadedImages, isNew: false);

    // ========= УДАЛЕНИЕ =========
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Json(new { success = false, message = "Требуется авторизация." });

        var eq = await _context.Equipments
            .AsSplitQuery() 
            .Include(e => e.Images)
            .FirstOrDefaultAsync(e => e.EquipmentId == id && e.CreatedByUserId == user.Id);

        if (eq is null)
            return Json(new { success = false, message = "Оборудование не найдено." });

        foreach (var img in eq.Images)
        {
            var path = Path.Combine(_env.WebRootPath, img.Url.TrimStart('/'));
            try { if (System.IO.File.Exists(path)) System.IO.File.Delete(path); } catch { }
        }

        _context.Equipments.Remove(eq);
        await _context.SaveChangesAsync();
        return Json(new { success = true, message = $"Оборудование «{eq.Name}» удалено." });
    }

    // ========= КОПИРОВАНИЕ =========
    [HttpPost]
[IgnoreAntiforgeryToken]
public async Task<IActionResult> Duplicate(int id)

{
    try
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
            return Json(new { success = false, message = "Требуется авторизация." });

        var eq = await _context.Equipments
         .AsSplitQuery() 
            .Include(e => e.Specs)
            .Include(e => e.Images)
            .FirstOrDefaultAsync(e => e.EquipmentId == id && e.CreatedByUserId == user.Id);

        if (eq is null)
            return Json(new { success = false, message = "Оборудование не найдено." });

        var copy = new Equipment
        {
            Name = string.IsNullOrWhiteSpace(eq.Name) ? "Без названия (копия)" : eq.Name + " (копия)",
            Model = eq.Model,
            ShortDescription = eq.ShortDescription,
            Description = eq.Description,
            IndustryId = eq.IndustryId,
            EquipmentCategoryId = eq.EquipmentCategoryId,
            ManufacturerId = eq.ManufacturerId,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedByUserId = user.Id
        };

        _context.Equipments.Add(copy);
        await _context.SaveChangesAsync();

        foreach (var s in eq.Specs)
        {
            _context.EquipmentSpecs.Add(new EquipmentSpec
            {
                EquipmentId = copy.EquipmentId,
                SpecKey = s.SpecKey,
                SpecValue = s.SpecValue,
                Unit = s.Unit,
                SortOrder = s.SortOrder
            });
        }

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads/equipment");
        Directory.CreateDirectory(uploadDir);

        foreach (var img in eq.Images)
        {
            var oldPath = Path.Combine(_env.WebRootPath, img.Url.TrimStart('/'));
            if (!System.IO.File.Exists(oldPath)) continue;

            var newName = $"{Guid.NewGuid()}{Path.GetExtension(img.Url)}";
            var newPath = Path.Combine(uploadDir, newName);
            System.IO.File.Copy(oldPath, newPath);

            _context.EquipmentImages.Add(new EquipmentImage
            {
                EquipmentId = copy.EquipmentId,
                Url = $"/uploads/equipment/{newName}",
                IsPrimary = img.IsPrimary,
                SortOrder = img.SortOrder,
                AltText = img.AltText
            });
        }

        await _context.SaveChangesAsync();

        return Json(new
        {
            success = true,
            message = $"Создана копия «{eq.Name}».",
            redirectUrl = Url.Action("MyEquipment")
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine("Ошибка в Duplicate(): " + ex);
        return Json(new { success = false, message = "Ошибка при копировании: " + ex.Message });
    }
}


    // ========= СОХРАНЕНИЕ (Create/Edit) =========
    private async Task<IActionResult> SaveEquipment(EquipmentEditVm vm, List<IFormFile>? uploadedImages, bool isNew)
    {
        try
        {
            Console.WriteLine("=== REQUEST FORM CONTENT ===");
            Console.WriteLine($"HasFormContentType: {Request.HasFormContentType}");
            if (Request.HasFormContentType)
            {
                foreach (var key in Request.Form.Keys)
                {
                    var val = Request.Form[key];
                    var preview = val.Count > 1 ? $"[{val.Count} items]" : val.ToString();
                    Console.WriteLine($"Form[{key}] = {preview}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка логирования Request.Form: " + ex.Message);
        }

        if (!ModelState.IsValid)
        {
            Console.WriteLine("=== MODELSTATE ERRORS ===");
            foreach (var kvp in ModelState)
            {
                if (kvp.Value?.Errors?.Count > 0)
                    Console.WriteLine($"Поле: {kvp.Key} -> {string.Join(" | ", kvp.Value.Errors.Select(e => e.ErrorMessage))}");
            }

            var errors = ModelState
                .Where(x => x.Value?.Errors?.Count > 0)
                .ToDictionary(
                    k => k.Key,
                    v => v.Value!.Errors.Select(e => e.ErrorMessage ?? string.Empty).ToArray()
                );

            var formKeys = new Dictionary<string, string[]>();
            if (Request.HasFormContentType)
            {
                foreach (var k in Request.Form.Keys)
                  formKeys[k] = Request.Form[k]
                            .Select(x => x ?? string.Empty)
                            .ToArray();
            }

            return Json(new { success = false, message = "Проверьте правильность заполнения полей.", errors, form = formKeys });
        }

        var user = await GetCurrentUserAsync();
        if (user is null) return Json(new { success = false, message = "Требуется авторизация." });

        Equipment? eq;
        if (isNew)
        {
            eq = new Equipment { CreatedByUserId = user.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _context.Equipments.Add(eq);
        }
        else
        {
             eq = await _context.Equipments
    .AsSplitQuery()
    .Include(e => e.Specs)
    .Include(e => e.Images)
    .FirstOrDefaultAsync(e => e.EquipmentId == vm.EquipmentId && e.CreatedByUserId == user.Id);

if (eq is null)
    return Json(new { success = false, message = "Оборудование не найдено." });


            eq.UpdatedAt = DateTime.UtcNow;
        }

        eq.Name = vm.Name?.Trim() ?? string.Empty;
        eq.Model = string.IsNullOrWhiteSpace(vm.Model) ? null : vm.Model.Trim();
        eq.ShortDescription = string.IsNullOrWhiteSpace(vm.ShortDescription) ? null : vm.ShortDescription.Trim();
        eq.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();
        eq.IndustryId = vm.IndustryId;
        eq.EquipmentCategoryId = vm.EquipmentCategoryId;
        eq.ManufacturerId = vm.ManufacturerId;
        eq.IsPublished = vm.IsPublished;

        try
        {
            await _context.SaveChangesAsync();

            var primaryImageId = Request.HasFormContentType
                ? Request.Form["PrimaryImageId"].FirstOrDefault()
                : null;

            await HandleImagesAsync(eq.EquipmentId, uploadedImages, primaryImageId);
            await HandleSpecsAsync(eq, vm.Specs ?? new List<EquipmentSpecVm>());

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = $"Оборудование «{vm.Name}» успешно {(isNew ? "добавлено" : "обновлено")}.",
                redirectUrl = Url.Action("MyEquipment")
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка сохранения: " + ex);
            return Json(new { success = false, message = "Ошибка при сохранении: " + ex.Message });
        }
    }
    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteImage(int id)
{
    var img = await _context.EquipmentImages.FirstOrDefaultAsync(i => i.EquipmentImageId == id);
    if (img is null) return Json(new { success = false, message = "Изображение не найдено." });

    var path = Path.Combine(_env.WebRootPath, img.Url.TrimStart('/'));
    try { if (System.IO.File.Exists(path)) System.IO.File.Delete(path); } catch { }

    _context.EquipmentImages.Remove(img);
    await _context.SaveChangesAsync();
    return Json(new { success = true });
}

    // ========= ОБРАБОТКА ИЗОБРАЖЕНИЙ =========
    private async Task HandleImagesAsync(int equipmentId, List<IFormFile>? uploadedImages, string? primaryImageId)
    {
        if (!string.IsNullOrEmpty(primaryImageId) && int.TryParse(primaryImageId, out var pid))
        {
            var imgs = await _context.EquipmentImages.Where(i => i.EquipmentId == equipmentId).ToListAsync();
            imgs.ForEach(i => i.IsPrimary = i.EquipmentImageId == pid);
        }

        if (uploadedImages != null && uploadedImages.Any(f => f.Length > 0))
        {
            var uploadDir = Path.Combine(_env.WebRootPath, "uploads/equipment");
            Directory.CreateDirectory(uploadDir);

            var existingCount = await _context.EquipmentImages.CountAsync(i => i.EquipmentId == equipmentId);
            int order = existingCount + 1;

            foreach (var file in uploadedImages.Where(f => f.Length > 0))
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadDir, fileName);

                await using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                _context.EquipmentImages.Add(new EquipmentImage
                {
                    EquipmentId = equipmentId,
                    Url = $"/uploads/equipment/{fileName}",
                    AltText = Path.GetFileNameWithoutExtension(file.FileName),
                    IsPrimary = existingCount == 0 && order == 1,
                    SortOrder = order++
                });
            }
        }
    }

    // ========= ОБРАБОТКА ХАРАКТЕРИСТИК =========
    private async Task HandleSpecsAsync(Equipment equipment, List<EquipmentSpecVm> specVms)
    {
        var validSpecs = specVms.Where(s => !string.IsNullOrWhiteSpace(s.SpecKey)).ToList();
        var existingIds = validSpecs.Where(s => s.EquipmentSpecId > 0).Select(s => s.EquipmentSpecId).ToHashSet();

        var toRemove = equipment.Specs.Where(s => !existingIds.Contains(s.EquipmentSpecId)).ToList();
        if (toRemove.Count > 0) _context.EquipmentSpecs.RemoveRange(toRemove);

        foreach (var specVm in validSpecs)
        {
            var spec = equipment.Specs.FirstOrDefault(s => s.EquipmentSpecId == specVm.EquipmentSpecId)
                      ?? new EquipmentSpec { EquipmentId = equipment.EquipmentId };

            spec.SpecKey = specVm.SpecKey.Trim();
            spec.SpecValue = string.IsNullOrWhiteSpace(specVm.SpecValue) ? string.Empty : specVm.SpecValue.Trim();
            spec.Unit = !string.IsNullOrWhiteSpace(specVm.Unit) ? specVm.Unit.Trim() : null;
            spec.SortOrder = specVm.SortOrder;

            if (spec.EquipmentSpecId == 0)
                equipment.Specs.Add(spec);
        }

        await Task.CompletedTask;
    }

    // ========= ВСПОМОГАТЕЛЬНЫЕ =========
    private async Task PopulateLookups(EquipmentEditVm vm)
{
    vm.Industries = await _context.Industries
        .OrderBy(i => i.Name)
        .Select(i => new LookupItem { Id = i.IndustryId, Name = i.Name })
        .ToListAsync();

    vm.Categories = await _context.EquipmentCategories
        .OrderBy(c => c.Name)
        .Select(c => new LookupItem { Id = c.EquipmentCategoryId, Name = c.Name })
        .ToListAsync();

    // Для User и Quest не показываем список производителей
    if (User.IsInRole("Admin") || User.IsInRole("Manager"))
    {
        vm.Manufacturers = await _context.Manufacturers
            .OrderBy(m => m.Name)
            .Select(m => new LookupItem { Id = m.ManufacturerId, Name = m.Name })
            .ToListAsync();
    }
    else
    {
        vm.Manufacturers = new List<LookupItem>();
        
        // Если у пользователя уже есть привязанный производитель, добавляем только его
        var user = await GetCurrentUserAsync();
        if (user?.ManufacturerId != null)
        {
            var manufacturer = await _context.Manufacturers
                .Where(m => m.ManufacturerId == user.ManufacturerId)
                .Select(m => new LookupItem { Id = m.ManufacturerId, Name = m.Name })
                .FirstOrDefaultAsync();
            
            if (manufacturer != null)
                vm.Manufacturers.Add(manufacturer);
        }
    }
}

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var userName = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(userName)) return null;
        return await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
    }
}
