using System.Collections.Generic;
using FoodTech.Models;

namespace FoodTech.ViewModels;

public class FilterOptionsVm
{
    public List<Industry> Industries { get; set; } = new();
    public List<EquipmentCategory> Categories { get; set; } = new();

    // 👇 Добавлено свойство для вывода списка заводов
    public List<ManufacturerVm> ManufacturersVm { get; set; } = new();
}
