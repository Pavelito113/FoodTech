using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoodTech.ViewModels
{
    public class EquipmentEditVm
    {
        public int? EquipmentId { get; set; }

        [Required, StringLength(400)]
        public string Name { get; set; } = null!;

        [StringLength(200)]
        public string? Model { get; set; }

        [StringLength(1000)]
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }

        [Required]
        public int IndustryId { get; set; }

        [Required]
        public int EquipmentCategoryId { get; set; }

        public int? ManufacturerId { get; set; }

        public bool IsPublished { get; set; } = true;

        public string? CreatedByUserId { get; set; }

        public List<EquipmentSpecVm> Specs { get; set; } = new();
        public List<EquipmentImageVm> Images { get; set; } = new();

        public List<LookupItem> Industries { get; set; } = new();
        public List<LookupItem> Categories { get; set; } = new();
        public List<LookupItem> Manufacturers { get; set; } = new();
    }

    public class LookupItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
