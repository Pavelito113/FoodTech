// FoodTech/ViewModels/ManufacturerVm.cs
using System.ComponentModel.DataAnnotations;

namespace FoodTech.ViewModels
{
    public class ManufacturerVm
    {
        public int ManufacturerId { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(100, ErrorMessage = "Максимальная длина 100 символов")]
        public string Name { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Максимальная длина 50 символов")]
        public string? Country { get; set; }

        [StringLength(200, ErrorMessage = "Максимальная длина 200 символов")]
        [RegularExpression(@"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$", 
            ErrorMessage = "Некорректный формат URL")]
        public string? Website { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(500, ErrorMessage = "Максимальная длина 500 символов")]
        public string? Notes { get; set; }

        [StringLength(100, ErrorMessage = "Максимальная длина 100 символов")]
        public string? ContactPerson { get; set; }

        [Phone(ErrorMessage = "Некорректный номер телефона")]
        [StringLength(20, ErrorMessage = "Максимальная длина 20 символов")]
        public string? ContactPhone { get; set; }

        [EmailAddress(ErrorMessage = "Некорректный email адрес")]
        [StringLength(100, ErrorMessage = "Максимальная длина 100 символов")]
        public string? ContactEmail { get; set; }
        
        // Для отображения
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsEditing { get; set; }
        
        // НОВЫЕ СВОЙСТВА ДЛЯ ПРИВЯЗКИ ПОЛЬЗОВАТЕЛЕЙ
        public List<UserVm>? AssignedUsers { get; set; }
        
        // Для формы привязки/отвязки
        public string? UserToAssignId { get; set; }
        public List<UserVm>? AvailableUsers { get; set; }
    }
}