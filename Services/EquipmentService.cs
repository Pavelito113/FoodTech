// =============================
// Services/EquipmentService.cs
// =============================
using Microsoft.EntityFrameworkCore;
using FoodTech.Data;
using FoodTech.Models;
using FoodTech.ViewModels;

namespace FoodTech.Services
{
    public class EquipmentService
    {
        private readonly ApplicationDbContext _context;

        public EquipmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===== Index =====
        public async Task<EquipmentIndexVm> GetEquipmentIndexVmAsync(EquipmentFilterVm filter)
        {
            filter ??= new EquipmentFilterVm();
            if (filter.Page < 1) filter.Page = 1;

            var query = _context.Equipments
                .Include(e => e.Images)
                .Include(e => e.Industry)
                .Include(e => e.Category)
                .Include(e => e.Manufacturer)
                .Where(e => e.IsPublished);

            if (filter.IndustryId.HasValue)
                query = query.Where(e => e.IndustryId == filter.IndustryId.Value);
            if (filter.EquipmentCategoryId.HasValue)
                query = query.Where(e => e.EquipmentCategoryId == filter.EquipmentCategoryId.Value);
            if (filter.ManufacturerId.HasValue)
                query = query.Where(e => e.ManufacturerId == filter.ManufacturerId.Value);
            if (!string.IsNullOrWhiteSpace(filter.Query))
                query = query.Where(e => e.Name.Contains(filter.Query));

            var totalItems = await query.CountAsync();

            var itemsList = await query
                .OrderBy(e => e.Name)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(e => new EquipmentListItemVm
                {
                    EquipmentId = e.EquipmentId,
                    Name = e.Name,
                    Model = e.Model ?? string.Empty,
                    ShortDescription = e.ShortDescription ?? string.Empty,
                    ImageUrl = e.Images.Where(i => i.IsPrimary).Select(i => i.Url).FirstOrDefault() ?? string.Empty,
                    IndustryName = e.Industry != null ? e.Industry.Name : string.Empty,
                    CategoryName = e.Category != null ? e.Category.Name : string.Empty,
                    ManufacturerName = e.Manufacturer != null ? e.Manufacturer.Name : string.Empty
                })
                .ToListAsync();

            var hotOffers = await _context.Equipments
                .Include(e => e.Images)
                .Where(e => e.IsPublished)
                .OrderByDescending(e => e.CreatedAt)
                .Take(3)
                .Select(e => new EquipmentListItemVm
                {
                    EquipmentId = e.EquipmentId,
                    Name = e.Name,
                    Model = e.Model ?? string.Empty,
                    ShortDescription = e.ShortDescription ?? string.Empty,
                    ImageUrl = e.Images.Where(i => i.IsPrimary).Select(i => i.Url).FirstOrDefault() ?? string.Empty
                })
                .ToListAsync();

            return new EquipmentIndexVm
            {
                Filter = filter,
                Items = itemsList,
                Pagination = new PaginationVm
                {
                    CurrentPage = filter.Page,
                    PageSize = filter.PageSize,
                    TotalItems = totalItems
                },
                HotOffers = hotOffers,
                Industries = await _context.Industries
                    .OrderBy(i => i.Name)
                    .Select(i => new LookupItem { Id = i.IndustryId, Name = i.Name })
                    .ToListAsync(),
                Categories = await _context.EquipmentCategories
                    .OrderBy(c => c.Name)
                    .Select(c => new LookupItem { Id = c.EquipmentCategoryId, Name = c.Name })
                    .ToListAsync(),
                Manufacturers = await _context.Manufacturers
                    .OrderBy(m => m.Name)
                    .Select(m => new LookupItem { Id = m.ManufacturerId, Name = m.Name })
                    .ToListAsync()
            };
        }

        // ===== Details =====
        public async Task<EquipmentDetailsVm?> GetEquipmentDetailsAsync(int id)
        {
            var equipment = await _context.Equipments
                .Include(x => x.Images)
                .Include(x => x.Specs)
                .Include(x => x.Industry)
                .Include(x => x.Category)
                .Include(x => x.Manufacturer)
                .AsSplitQuery()
                .FirstOrDefaultAsync(x => x.EquipmentId == id && x.IsPublished);

            if (equipment == null)
                return null;

            return new EquipmentDetailsVm
            {
                EquipmentId = equipment.EquipmentId,
                Name = equipment.Name,
                Model = equipment.Model ?? string.Empty,
                ShortDescription = equipment.ShortDescription ?? string.Empty,
                Description = equipment.Description ?? string.Empty,
                IndustryName = equipment.Industry?.Name ?? string.Empty,
                CategoryName = equipment.Category?.Name ?? string.Empty,
                CreatedByUserId = equipment.CreatedByUserId,
                ManufacturerName = equipment.Manufacturer?.Name ?? string.Empty,
                ManufacturerWebsite = equipment.Manufacturer?.Website ?? string.Empty,
                Images = equipment.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new EquipmentImageVm
                    {
                        EquipmentImageId = i.EquipmentImageId,
                        Url = i.Url,
                        AltText = i.AltText ?? string.Empty,
                        IsPrimary = i.IsPrimary,
                        SortOrder = i.SortOrder
                    }).ToList(),
                Specs = equipment.Specs
                    .OrderBy(s => s.SortOrder)
                    .Select(s => new EquipmentSpecVm
                    {
                        EquipmentSpecId = s.EquipmentSpecId,
                        SpecKey = s.SpecKey,
                        SpecValue = s.SpecValue,
                        Unit = s.Unit ?? string.Empty,
                        SortOrder = s.SortOrder,
                        EquipmentId = s.EquipmentId
                    }).ToList(),
                ContactRequest = new ContactRequestVm { EquipmentId = equipment.EquipmentId },
                ReturnPage = 1,
                Filter = new EquipmentFilterVm()
            };
        }

        // ===== Общая контактная форма =====
        public async Task CreateGeneralContactRequestAsync(ContactRequestVm vm, string? sourceUrl = null)
        {
            var contact = new ContactRequest
            {
                Name = vm.Name,
                Email = vm.Email ?? string.Empty,
                Phone = vm.Phone ?? string.Empty,
                Company = vm.Company ?? string.Empty,
                Message = vm.Message ?? string.Empty,
                EquipmentId = null, // 👈 null для общей формы
                EquipmentName = "Общий вопрос",
                SourceUrl = sourceUrl ?? string.Empty,
                Status = "New",
                CreatedAt = DateTime.UtcNow
            };

            _context.ContactRequests.Add(contact);
            await _context.SaveChangesAsync();
        }

        // ===== Контактная форма для оборудования =====
        public async Task<ContactRequest> CreateContactRequestAsync(ContactRequestVm vm, string? sourceUrl = null)
        {
            if (!vm.EquipmentId.HasValue)
                throw new ArgumentException("EquipmentId не может быть null");

            // Получаем имя оборудования по ID
            var equipmentName = await _context.Equipments
                .Where(e => e.EquipmentId == vm.EquipmentId.Value)
                .Select(e => e.Name)
                .FirstOrDefaultAsync() ?? string.Empty;

            var contact = new ContactRequest
            {
                Name = vm.Name,
                Email = vm.Email ?? string.Empty,
                Phone = vm.Phone ?? string.Empty,
                Company = vm.Company ?? string.Empty,
                Message = vm.Message ?? string.Empty,
                EquipmentId = vm.EquipmentId.Value,
                EquipmentName = equipmentName,
                SourceUrl = sourceUrl ?? string.Empty,
                Status = "New",
                CreatedAt = DateTime.UtcNow
            };

            _context.ContactRequests.Add(contact);
            await _context.SaveChangesAsync();

            // синхронизируем имя обратно в vm
            vm.EquipmentName = equipmentName;

            return contact;
        }
    }
}