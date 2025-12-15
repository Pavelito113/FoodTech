using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodTech.Migrations
{
    /// <inheritdoc />
    public partial class AddManufacturerContactFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Manufacturer",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPerson",
                table: "Manufacturer",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Manufacturer",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Manufacturer");

            migrationBuilder.DropColumn(
                name: "ContactPerson",
                table: "Manufacturer");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Manufacturer");
        }
    }
}
