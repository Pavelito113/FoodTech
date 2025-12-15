namespace FoodTech.ViewModels;

public class EquipmentIndexVm
{
    public required EquipmentFilterVm Filter { get; set; }
    public required List<EquipmentListItemVm> Items { get; set; }
    public required PaginationVm Pagination { get; set; }

    public List<EquipmentListItemVm>? HotOffers { get; set; }

    public List<LookupItem> Industries { get; set; } = new();
    public List<LookupItem> Categories { get; set; } = new();
    public List<LookupItem> Manufacturers { get; set; } = new();
}