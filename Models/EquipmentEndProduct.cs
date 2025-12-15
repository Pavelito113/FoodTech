namespace FoodTech.Models
{
    public class EquipmentEndProduct
    {
        public int EquipmentId { get; set; }
        public int EndProductId { get; set; }

        public Equipment Equipment { get; set; } = null!;
        public EndProduct EndProduct { get; set; } = null!;
    }
}
