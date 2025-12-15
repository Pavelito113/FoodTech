namespace FoodTech.ViewModels;

public class EquipmentListItemVm
{
    public int EquipmentId { get; set; }
    public required string Name { get; set; }
    public string? Model { get; set; }
    public string? ShortDescription { get; set; }
    public string? ImageUrl { get; set; }
    public string? IndustryName { get; set; }
    public string? CategoryName { get; set; }
    public string? ManufacturerName { get; set; }
}