using System.ComponentModel.DataAnnotations;

namespace FoodTech.ViewModels;
public class ContactRequestUserVm
{
    public ContactRequestVm ContactRequest { get; set; } = new ContactRequestVm();
    public bool IsMyEquipment { get; set; }
    public bool IsMyManufacturer { get; set; }
}