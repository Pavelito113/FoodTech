using System.ComponentModel.DataAnnotations;

namespace FoodTech.ViewModels;

public class ContactRequestVm
{
    public long? ContactRequestId { get; set; }
    public int? EquipmentId { get; set; }

    [StringLength(400)]
    public string? EquipmentName { get; set; }

    [Required, StringLength(240)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress, StringLength(400)]
    public string? Email { get; set; }

    [StringLength(100)]
    public string? Phone { get; set; }

    [StringLength(400)]
    public string? Company { get; set; }

    [StringLength(800)]
    public string? Message { get; set; }

    [StringLength(600)]
    public string? SourceUrl { get; set; }

    [Required, StringLength(60)]
    public string Status { get; set; } = "New";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
