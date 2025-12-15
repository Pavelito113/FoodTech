namespace FoodTech.Models
{
    public class EquipmentSpec
    {
        public int EquipmentSpecId { get; set; }
        public int EquipmentId { get; set; }
        public string SpecKey { get; set; } = null!;
        public string SpecValue { get; set; } = null!;
        public string? Unit { get; set; }
        public int SortOrder { get; set; }

        public Equipment Equipment { get; set; } = null!;
    }
}
