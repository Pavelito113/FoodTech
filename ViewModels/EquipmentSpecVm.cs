namespace FoodTech.ViewModels
{
    public class EquipmentSpecVm
    {
        public int EquipmentSpecId { get; set; }
        public int? EquipmentId { get; set; }
        public string SpecKey { get; set; } = null!;
        public string SpecValue { get; set; } = null!;
        public string? Unit { get; set; }
        public int SortOrder { get; set; }
    }
}
