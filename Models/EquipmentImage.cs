namespace FoodTech.Models
{
    public class EquipmentImage
    {
        public int EquipmentImageId { get; set; }
        public int EquipmentId { get; set; }
        public string Url { get; set; } = null!;
        public string? AltText { get; set; }
        public bool IsPrimary { get; set; }
        public int SortOrder { get; set; }

        public Equipment Equipment { get; set; } = null!;
    }
}
