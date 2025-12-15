using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodTech.Migrations
{
    /// <inheritdoc />
    public partial class AddEquipmentNameToContactRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Equipment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EquipmentName",
                table: "ContactRequest",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Equipment");

            migrationBuilder.DropColumn(
                name: "EquipmentName",
                table: "ContactRequest");
        }
    }
}
