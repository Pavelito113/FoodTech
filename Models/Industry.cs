using System.Collections.Generic;

namespace FoodTech.Models
{
    public class Industry
    {
        public int IndustryId { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public int SortOrder { get; set; }

        public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    }
}
