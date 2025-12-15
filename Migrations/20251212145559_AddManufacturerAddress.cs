using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodTech.Migrations
{
    /// <inheritdoc />
    public partial class AddManufacturerAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Manufacturer",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Manufacturer");
        }
    }
}
