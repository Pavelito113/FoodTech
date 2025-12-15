using System.ComponentModel.DataAnnotations;

namespace FoodTech.ViewModels
{
    public class ManufacturerUserEditCreateVm
    {
        public int? ManufacturerId { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(100, ErrorMessage = "Максимальная длина 100 символов")]
        public string Name { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Максимальная длина 50 символов")]
        public string? Country { get; set; }

        [StringLength(200, ErrorMessage = "Максимальная длина 200 символов")]
        public string? Website { get; set; }

        [StringLength(500, ErrorMessage = "Максимальная длина 500 символов")]
        public string? Notes { get; set; }

        [StringLength(200, ErrorMessage = "Максимальная длина 200 символов")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "Максимальная длина 100 символов")]
        public string? ContactPerson { get; set; }

        [Phone(ErrorMessage = "Некорректный номер телефона")]
        [StringLength(20, ErrorMessage = "Максимальная длина 20 символов")]
        public string? ContactPhone { get; set; }

        [EmailAddress(ErrorMessage = "Некорректный email адрес")]
        [StringLength(100, ErrorMessage = "Максимальная длина 100 символов")]
        public string? ContactEmail { get; set; }
    }
}
