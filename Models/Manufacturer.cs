using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoodTech.Models
{
    public class Manufacturer
    {
        public int ManufacturerId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Country { get; set; }
        
        [StringLength(500)]
        public string? Website { get; set; }
        
        public string? Notes { get; set; }
        [StringLength(200)]
public string? Address { get; set; }

        
        // КОНТАКТНЫЕ ДАННЫЕ ПРОИЗВОДИТЕЛЯ
        [StringLength(100)]
        public string? ContactPerson { get; set; }
        
        [StringLength(20)]
        public string? ContactPhone { get; set; }
        
        [StringLength(100)]
        public string? ContactEmail { get; set; }
        
        // Временные метки
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Навигационные свойства (как уже настроено в DbContext)
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    }
}