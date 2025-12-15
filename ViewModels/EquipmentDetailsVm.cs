namespace FoodTech.ViewModels
{
    public class EquipmentDetailsVm
    {
        public int EquipmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Model { get; set; }
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public string? CreatedByUserId { get; set; } // 👈 ДОБАВЛЕНО

        // Навигационные свойства
        public string? IndustryName { get; set; }
        public string? CategoryName { get; set; }
        public string? ManufacturerName { get; set; }
        public string? ManufacturerWebsite { get; set; }

        // Коллекции
        public List<EquipmentImageVm> Images { get; set; } = new();
        public List<EquipmentSpecVm> Specs { get; set; } = new();

        // Обратная связь
        public ContactRequestVm ContactRequest { get; set; } = new ContactRequestVm();

        // Для возврата к списку
        public int ReturnPage { get; set; }
        public EquipmentFilterVm Filter { get; set; } = new();
    }

    public class EquipmentImageVm
    {
        public int EquipmentImageId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? AltText { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }

        
    }

    
}