using System;

namespace FoodTech.Models
{
    public class ContactRequest
    {
        public long ContactRequestId { get; set; }
        public int? EquipmentId { get; set; }
        public string? EquipmentName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Company { get; set; }
        public string? Message { get; set; }
        
        public string? SourceUrl { get; set; }
        public string Status { get; set; } = null!;

        public Equipment? Equipment { get; set; }
    }
}
