using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FoodTech.Data;
using FoodTech.Models;
using FoodTech.ViewModels;
using System.Security.Claims;

namespace FoodTech.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("admin/manufacturers")]
    public class ManufacturersController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManufacturersController(
            ApplicationDbContext ctx,
            UserManager<ApplicationUser> userManager)
        {
            _ctx = ctx;
            _userManager = userManager;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        }

        // ========== СУЩЕСТВУЮЩИЙ МЕТОД ==========
        // GET: /admin/manufacturers
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var manufacturers = await _ctx.Manufacturers
                .AsSplitQuery()
                .Include(m => m.Users)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            var manufacturerVms = manufacturers.Select(m => new ManufacturerVm
            {
                ManufacturerId = m.ManufacturerId,
                Name = m.Name,
                Country = m.Country,
                Website = m.Website,
                Notes = m.Notes,
                ContactPerson = m.ContactPerson,
                ContactPhone = m.ContactPhone,
                ContactEmail = m.ContactEmail,
                CreatedBy = m.Users.FirstOrDefault()?.LastName ?? "Система",
                CreatedAt = m.CreatedAt,
                IsEditing = false,
                AssignedUsers = m.Users.Select(u => new UserVm
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Phone = u.PhoneNumber,
                    Company = u.Company
                }).ToList()
            }).ToList();

            return PartialView("~/Views/Admin/_EditManufacturer.cshtml", manufacturerVms);
        }

        // ========== НОВЫЕ МЕТОДЫ (ДОБАВИТЬ) ==========
        
        // GET: /admin/manufacturers/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            return PartialView("~/Views/Admin/_ManufacturerForm.cshtml", new ManufacturerVm
            {
                IsEditing = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // GET: /admin/manufacturers/edit/{id}
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var manufacturer = await _ctx.Manufacturers
                .AsSplitQuery()
                .Include(m => m.Users)
                .FirstOrDefaultAsync(m => m.ManufacturerId == id);

            if (manufacturer == null)
                return NotFound();

            var model = new ManufacturerVm
            {
                ManufacturerId = manufacturer.ManufacturerId,
                Name = manufacturer.Name,
                Country = manufacturer.Country,
                Website = manufacturer.Website,
                Notes = manufacturer.Notes,
                ContactPerson = manufacturer.ContactPerson,
                ContactPhone = manufacturer.ContactPhone,
                ContactEmail = manufacturer.ContactEmail,
                CreatedBy = manufacturer.Users.FirstOrDefault()?.LastName ?? "Система",
                CreatedAt = manufacturer.CreatedAt,
                IsEditing = true
            };

            return PartialView("~/Views/Admin/_ManufacturerForm.cshtml", model);
        }

        // POST: /admin/manufacturers/save
        [HttpPost("save")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(ManufacturerVm model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("~/Views/Admin/_ManufacturerForm.cshtml", model);
            }

            var currentUserId = GetCurrentUserId();
            var currentUser = !string.IsNullOrEmpty(currentUserId)
                ? await _userManager.FindByIdAsync(currentUserId)
                : null;

            try
            {
                if (model.ManufacturerId == 0)
                {
                    // Создание
                    bool exists = await _ctx.Manufacturers
                        .AnyAsync(m => m.Name.ToLower() == model.Name.Trim().ToLower());

                    if (exists)
                    {
                        ModelState.AddModelError("Name", $"Производитель «{model.Name}» уже существует");
                        return PartialView("~/Views/Admin/_ManufacturerForm.cshtml", model);
                    }

                    var manufacturer = new Manufacturer
                    {
                        Name = model.Name.Trim(),
                        Country = model.Country?.Trim(),
                        Website = model.Website?.Trim(),
                        Notes = model.Notes?.Trim(),
                        ContactPerson = model.ContactPerson?.Trim(),
                        ContactPhone = model.ContactPhone?.Trim(),
                        ContactEmail = model.ContactEmail?.Trim(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _ctx.Manufacturers.Add(manufacturer);
                    await _ctx.SaveChangesAsync();

                    // Связываем текущего пользователя с производителем
                    if (currentUser != null && currentUser.ManufacturerId == null)
                    {
                        currentUser.ManufacturerId = manufacturer.ManufacturerId;
                        await _userManager.UpdateAsync(currentUser);
                    }

                    model.ManufacturerId = manufacturer.ManufacturerId;
                    model.CreatedAt = manufacturer.CreatedAt;
                    model.CreatedBy = currentUser != null ? $"{currentUser.FirstName} {currentUser.LastName}" : "Система";
                }
                else
                {
                    // Обновление
                    var manufacturer = await _ctx.Manufacturers
                        .AsSplitQuery()
                        .Include(m => m.Users)
                        .FirstOrDefaultAsync(m => m.ManufacturerId == model.ManufacturerId);

                    if (manufacturer == null)
                    {
                        return Json(new { success = false, message = "Производитель не найден" });
                    }

                    // Проверка дубликатов при обновлении
                    bool duplicate = await _ctx.Manufacturers
                        .AnyAsync(m => m.ManufacturerId != model.ManufacturerId &&
                                       m.Name.ToLower() == model.Name.Trim().ToLower());

                    if (duplicate)
                    {
                        ModelState.AddModelError("Name", $"Производитель «{model.Name}» уже существует");
                        return PartialView("~/Views/Admin/_ManufacturerForm.cshtml", model);
                    }

                    manufacturer.Name = model.Name.Trim();
                    manufacturer.Country = model.Country?.Trim();
                    manufacturer.Website = model.Website?.Trim();
                    manufacturer.Notes = model.Notes?.Trim();
                    manufacturer.ContactPerson = model.ContactPerson?.Trim();
                    manufacturer.ContactPhone = model.ContactPhone?.Trim();
                    manufacturer.ContactEmail = model.ContactEmail?.Trim();
                    manufacturer.UpdatedAt = DateTime.UtcNow;

                    _ctx.Manufacturers.Update(manufacturer);
                    await _ctx.SaveChangesAsync();

                    // Обновляем CreatedBy для возврата
                    var createdByUser = manufacturer.Users.FirstOrDefault();
                    model.CreatedBy = createdByUser != null ? $"{createdByUser.FirstName} {createdByUser.LastName}" : "Система";
                    model.CreatedAt = manufacturer.CreatedAt;
                }

                model.IsEditing = false;
                return PartialView("~/Views/Shared/_ManufacturerRow.cshtml", model);
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", $"Ошибка сохранения: {ex.InnerException?.Message ?? ex.Message}");
                return PartialView("~/Views/Admin/_ManufacturerForm.cshtml", model);
            }
        }

        // GET: /admin/manufacturers/delete/{id}
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var manufacturer = await _ctx.Manufacturers
                .AsSplitQuery()
                .Include(m => m.Users)
                .Include(m => m.Equipments)
                .FirstOrDefaultAsync(m => m.ManufacturerId == id);

            if (manufacturer == null)
            {
                return NotFound();
            }

            var hasUsers = manufacturer.Users.Any();
            var hasEquipments = manufacturer.Equipments.Any();

            var deleteInfo = new
            {
                ManufacturerId = manufacturer.ManufacturerId,
                Name = manufacturer.Name,
                HasUsers = hasUsers,
                UserCount = manufacturer.Users.Count,
                HasEquipments = hasEquipments,
                EquipmentCount = manufacturer.Equipments.Count
            };

            return PartialView("~/Views/Shared/_DeleteConfirmation.cshtml", deleteInfo);
        }

        // POST: /admin/manufacturers/delete
        [HttpPost("delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed([FromForm] int id)
        {
            try
            {
                var manufacturer = await _ctx.Manufacturers.FindAsync(id);

                if (manufacturer == null)
                {
                    return Json(new { success = false, message = "Производитель не найден" });
                }

                var currentUserId = GetCurrentUserId();

                // Проверяем связанные записи
                var hasOtherUsers = await _ctx.Users.AnyAsync(u => u.ManufacturerId == id && u.Id != currentUserId);
                var hasEquipment = await _ctx.Equipments.AnyAsync(e => e.ManufacturerId == id);

                if (hasOtherUsers || hasEquipment)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Нельзя удалить производителя, есть связанные записи"
                    });
                }

                _ctx.Manufacturers.Remove(manufacturer);

                // Если текущий пользователь был привязан к этому производителю — отвязываем
                var currentUser = !string.IsNullOrEmpty(currentUserId)
                    ? await _userManager.FindByIdAsync(currentUserId)
                    : null;

                if (currentUser != null && currentUser.ManufacturerId == id)
                {
                    currentUser.ManufacturerId = null;
                    await _userManager.UpdateAsync(currentUser);
                }

                await _ctx.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ========== СУЩЕСТВУЮЩИЕ МЕТОДЫ ==========
        
        // GET: /admin/manufacturers/users/{manufacturerId}
        [HttpGet("users/{manufacturerId}")]
        public async Task<IActionResult> GetUsersForAssignment(int manufacturerId)
        {
            var manufacturer = await _ctx.Manufacturers
                .Include(m => m.Users)
                .FirstOrDefaultAsync(m => m.ManufacturerId == manufacturerId);

            if (manufacturer == null)
                return NotFound();

            // Привязанные пользователи
            var assignedUsers = manufacturer.Users.Select(u => new UserVm
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Phone = u.PhoneNumber,
                Company = u.Company
            }).ToList();

            // Доступные пользователи (без производителя или другой роли)
            var availableUsers = await _ctx.Users
                .Where(u => u.ManufacturerId == null || u.ManufacturerId == manufacturerId)
                .Select(u => new UserVm
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Phone = u.PhoneNumber,
                    Company = u.Company,
                    ManufacturerId = u.ManufacturerId
                })
                .ToListAsync();

            var model = new UserAssignmentVm
            {
                ManufacturerId = manufacturer.ManufacturerId,
                ManufacturerName = manufacturer.Name,
                AssignedUsers = assignedUsers,
                AvailableUsers = availableUsers
            };

            return PartialView("~/Views/Admin/_UserAssignment.cshtml", model);
        }

        // POST: /admin/manufacturers/assign-user
        [HttpPost("assign-user")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignUser([FromBody] AssignUserRequest request)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Неверные данные" });

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return Json(new { success = false, message = "Пользователь не найден" });

            var manufacturer = await _ctx.Manufacturers.FindAsync(request.ManufacturerId);
            if (manufacturer == null)
                return Json(new { success = false, message = "Производитель не найден" });

            // Проверяем, не привязан ли уже пользователь к другому производителю
            if (user.ManufacturerId != null && user.ManufacturerId != request.ManufacturerId)
            {
                return Json(new 
                { 
                    success = false, 
                    message = $"Пользователь уже привязан к другому производителю" 
                });
            }

            user.ManufacturerId = request.ManufacturerId;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Обновляем навигационное свойство
                manufacturer.Users.Add(user);
                await _ctx.SaveChangesAsync();

                return Json(new 
                { 
                    success = true,
                    user = new 
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FullName = user.LastName,
                        Phone = user.PhoneNumber
                    }
                });
            }

            return Json(new 
            { 
                success = false, 
                message = string.Join(", ", result.Errors.Select(e => e.Description)) 
            });
        }

        // POST: /admin/manufacturers/unassign-user
        [HttpPost("unassign-user")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignUser([FromBody] UnassignUserRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return Json(new { success = false, message = "Пользователь не найден" });

            var manufacturer = await _ctx.Manufacturers
                .Include(m => m.Users)
                .FirstOrDefaultAsync(m => m.ManufacturerId == request.ManufacturerId);

            if (manufacturer == null)
                return Json(new { success = false, message = "Производитель не найден" });

            // Проверяем, что пользователь действительно привязан к этому производителю
            if (user.ManufacturerId != request.ManufacturerId)
            {
                return Json(new { success = false, message = "Пользователь не привязан к этому производителю" });
            }

            user.ManufacturerId = null;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Удаляем из навигационного свойства
                manufacturer.Users.Remove(user);
                await _ctx.SaveChangesAsync();

                return Json(new { success = true });
            }

            return Json(new 
            { 
                success = false, 
                message = string.Join(", ", result.Errors.Select(e => e.Description)) 
            });
        }
    }

    // DTO для запросов
    public class AssignUserRequest
    {
        public int ManufacturerId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class UnassignUserRequest
    {
        public int ManufacturerId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}