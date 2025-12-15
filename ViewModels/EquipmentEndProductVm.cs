namespace FoodTech.ViewModels;

public class EquipmentEndProductVm
{
    public int EquipmentId { get; set; }
    public int EndProductId { get; set; }
    public string EndProductName { get; set; } = string.Empty;
    public string EndProductSlug { get; set; } = string.Empty;
}