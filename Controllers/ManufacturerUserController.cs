using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodTech.Data;
using FoodTech.Models;
using FoodTech.ViewModels;

namespace FoodTech.Controllers;

[Authorize]
public class ManufacturerUserController : Controller
{
    private readonly ApplicationDbContext _ctx;
    private readonly UserManager<ApplicationUser> _userManager;

    public ManufacturerUserController(ApplicationDbContext ctx, UserManager<ApplicationUser> userManager)
    {
        _ctx = ctx;
        _userManager = userManager;
    }

    // ========================================================================
    //                              CREATE GET
    // ========================================================================
    [HttpGet]
    public IActionResult Create()
    {
        var model = new ManufacturerUserEditCreateVm();
        return View(model);
    }

    // ========================================================================
    //                              CREATE POST
    // ========================================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ManufacturerUserEditCreateVm model)
    {
        // Проверка на null
        if (model == null)
        {
            ModelState.AddModelError("", "Неверные данные формы");
            return View(new ManufacturerUserEditCreateVm());
        }

        if (!ModelState.IsValid)
            return View(model);

        model.Website = NormalizeWebsite(model.Website);

        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "Пользователь не найден";
                return RedirectToAction("Login", "Account");
            }

            if (user.ManufacturerId != null)
            {
                TempData["Error"] = "У вас уже есть привязанный производитель";
                return RedirectToAction("Edit");
            }

            bool exists = await _ctx.Manufacturers
                .AnyAsync(m => EF.Functions.Collate(m.Name, "SQL_Latin1_General_CP1_CI_AS") == model.Name.Trim());

            if (exists)
            {
                ModelState.AddModelError("Name", $"Производитель «{model.Name}» уже существует");
                return View(model);
            }

            var manufacturer = new Manufacturer
            {
                Name = model.Name.Trim(),
                Country = model.Country?.Trim(),
                Website = model.Website,
                Notes = model.Notes?.Trim(),
                Address = model.Address?.Trim(),
                ContactPerson = model.ContactPerson?.Trim(),
                ContactPhone = model.ContactPhone?.Trim(),
                ContactEmail = model.ContactEmail?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _ctx.Manufacturers.Add(manufacturer);
            await _ctx.SaveChangesAsync();

            user.ManufacturerId = manufacturer.ManufacturerId;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = $"Производитель «{manufacturer.Name}» успешно создан!";
            
            // Перенаправляем обратно на форму оборудования с параметрами
            // для отображения завода с лавровыми листочками
            return RedirectToAction("Create", "Equipment", new
            {
                manufacturerCreated = true,
                manufacturerName = manufacturer.Name,
                manufacturerId = manufacturer.ManufacturerId
            });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка: {ex.Message}");
            return View(model);
        }
    }

    // ========================================================================
    //                              EDIT GET
    // ========================================================================
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.ManufacturerId == null)
            return RedirectToAction("Create");

        var m = await _ctx.Manufacturers.FirstOrDefaultAsync(x => x.ManufacturerId == user.ManufacturerId);
        if (m == null)
        {
            TempData["Error"] = "Производитель не найден";
            return RedirectToAction("Create");
        }

        var vm = new ManufacturerUserEditCreateVm
        {
            ManufacturerId = m.ManufacturerId,
            Name = m.Name,
            Country = m.Country,
            Website = m.Website,
            Notes = m.Notes,
            Address = m.Address,
            ContactPerson = m.ContactPerson,
            ContactPhone = m.ContactPhone,
            ContactEmail = m.ContactEmail
        };

        return View(vm);
    }

    // ========================================================================
    //                              EDIT POST
    // ========================================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ManufacturerUserEditCreateVm model)
    {
        // Проверка на null
        if (model == null)
        {
            ModelState.AddModelError("", "Неверные данные формы");
            return View(new ManufacturerUserEditCreateVm());
        }

        if (!ModelState.IsValid)
            return View(model);

        model.Website = NormalizeWebsite(model.Website);

        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.ManufacturerId == null)
            {
                TempData["Error"] = "У вас нет привязанного производителя";
                return RedirectToAction("Create");
            }

            var m = await _ctx.Manufacturers.FirstOrDefaultAsync(x => x.ManufacturerId == user.ManufacturerId);
            if (m == null)
            {
                TempData["Error"] = "Производитель не найден";
                return RedirectToAction("Create");
            }

            if (!string.Equals(m.Name, model.Name.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                bool duplicate = await _ctx.Manufacturers
                    .AnyAsync(x => x.ManufacturerId != m.ManufacturerId &&
                                   EF.Functions.Collate(x.Name, "SQL_Latin1_General_CP1_CI_AS") == model.Name.Trim());

                if (duplicate)
                {
                    ModelState.AddModelError("Name", $"Производитель «{model.Name}» уже существует");
                    return View(model);
                }
            }

            m.Name = model.Name.Trim();
            m.Country = model.Country?.Trim();
            m.Website = model.Website;
            m.Notes = model.Notes?.Trim();
            m.Address = model.Address?.Trim();
            m.ContactPerson = model.ContactPerson?.Trim();
            m.ContactPhone = model.ContactPhone?.Trim();
            m.ContactEmail = model.ContactEmail?.Trim();
            m.UpdatedAt = DateTime.UtcNow;

            await _ctx.SaveChangesAsync();

            TempData["Success"] = "Данные обновлены";
            return RedirectToAction("Edit");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка: {ex.Message}");
            return View(model);
        }
    }

    // ========================================================================
    //                              ДОПОЛНИТЕЛЬНЫЙ МЕТОД ДЛЯ AJAX
    // ========================================================================

    /// <summary>
    /// Проверяет, есть ли у текущего пользователя привязанный производитель
    /// Используется для динамического обновления блока производителя
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCurrentManufacturer()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.ManufacturerId == null)
        {
            return Json(new { 
                hasManufacturer = false,
                message = "У вас нет привязанного производителя"
            });
        }

        var manufacturer = await _ctx.Manufacturers
            .FirstOrDefaultAsync(m => m.ManufacturerId == user.ManufacturerId);

        if (manufacturer == null)
        {
            return Json(new { 
                hasManufacturer = false,
                message = "Производитель не найден"
            });
        }

        return Json(new
        {
            hasManufacturer = true,
            manufacturerId = manufacturer.ManufacturerId,
            manufacturerName = manufacturer.Name,
            manufacturerCountry = manufacturer.Country,
            manufacturerWebsite = manufacturer.Website
        });
    }

    // ========================================================================
    //                              HELPERS
    // ========================================================================
    private string? NormalizeWebsite(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        url = url.Trim();

        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            url = "https://" + url;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return null;

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return null;

        return uri.ToString();
    }
}