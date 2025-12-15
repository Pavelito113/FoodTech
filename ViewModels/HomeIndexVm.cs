using System.Collections.Generic;

namespace FoodTech.ViewModels
{
    public class HomeIndexVm
    {
        public List<EquipmentListItemVm> PopularEquipment { get; set; } = new();
        public List<EquipmentListItemVm> NewEquipment { get; set; } = new();
        public EquipmentFilterVm Filter { get; set; } = new();
    }
}