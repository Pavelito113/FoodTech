using System.Collections.Generic;

namespace FoodTech.Models
{
    public class EndProduct
    {
        public int EndProductId { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public int SortOrder { get; set; }

        public ICollection<EquipmentEndProduct> EquipmentEndProducts { get; set; } = new List<EquipmentEndProduct>();
    }
}
