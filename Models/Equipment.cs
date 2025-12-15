using System;
using System.Collections.Generic;

namespace FoodTech.Models
{
    public class Equipment
    {
        public int EquipmentId { get; set; }

        public string Name { get; set; } = null!;
            
    public string Slug { get; set; } = string.Empty;
        public string? CreatedByUserId { get; set; }
        public string? Model { get; set; }
        
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }

        // ===== Внешние ключи =====
        public int IndustryId { get; set; }
        public int EquipmentCategoryId { get; set; }
        public int? ManufacturerId { get; set; }

        // ===== Системные поля =====
        public bool IsPublished { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // ===== Навигационные свойства =====
        public Industry Industry { get; set; } = null!;
        public EquipmentCategory Category { get; set; } = null!;
        public Manufacturer? Manufacturer { get; set; }

        public ICollection<EquipmentImage> Images { get; set; } = new List<EquipmentImage>();
        public ICollection<EquipmentSpec> Specs { get; set; } = new List<EquipmentSpec>();
        public ICollection<EquipmentEndProduct> EquipmentEndProducts { get; set; } = new List<EquipmentEndProduct>();
        public ICollection<ContactRequest> ContactRequests { get; set; } = new List<ContactRequest>();
    }
}
