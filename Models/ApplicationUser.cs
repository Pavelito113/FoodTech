using Microsoft.AspNetCore.Identity;

namespace FoodTech.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
       

    public int? ManufacturerId { get; set; } // 🔹 добавляем FK
    public Manufacturer? Manufacturer { get; set; } // 🔹 навигационное свойство
    }
}