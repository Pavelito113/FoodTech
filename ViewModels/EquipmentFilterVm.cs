namespace FoodTech.ViewModels;

public class EquipmentFilterVm
{
    public int? IndustryId { get; set; }
    public int? EquipmentCategoryId { get; set; }
    public int? ManufacturerId { get; set; }
    public string? Query { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public string? SortBy { get; set; } // "newest", "name"
}
