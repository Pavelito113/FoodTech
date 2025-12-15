using System.Collections.Generic;

namespace FoodTech.Models
{
    public class EquipmentCategory
    {
        public int EquipmentCategoryId { get; set; }
        public int? ParentCategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public int SortOrder { get; set; }

        public EquipmentCategory? ParentCategory { get; set; }
        public ICollection<EquipmentCategory> Children { get; set; } = new List<EquipmentCategory>();
        public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    }
}

