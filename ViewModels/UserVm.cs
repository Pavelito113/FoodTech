namespace FoodTech.ViewModels
{
    public class UserVm
    {
        public string Id { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }

        // 👇 новые свойства, которых не хватало
        public string? Name { get; set; }
        public string? Phone { get; set; }

        public string? Company { get; set; }
        public string? Role { get; set; }
        public int? ManufacturerId { get; set; }

        // при необходимости — если где-то используются:
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool? IsFrozen  {get; set; }
    }
}
