// FoodTech/ViewModels/UserAssignmentVm.cs
namespace FoodTech.ViewModels
{
    public class UserAssignmentVm
    {
        public int ManufacturerId { get; set; }
        public string ManufacturerName { get; set; } = string.Empty;
        public List<UserVm> AssignedUsers { get; set; } = new();
        public List<UserVm> AvailableUsers { get; set; } = new();
        public string? SelectedUserId { get; set; }
    }
}