using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodTech.Migrations
{
    /// <inheritdoc />
    public partial class AddManufacturerToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ManufacturerId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ManufacturerId",
                table: "AspNetUsers",
                column: "ManufacturerId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Manufacturer_ManufacturerId",
                table: "AspNetUsers",
                column: "ManufacturerId",
                principalTable: "Manufacturer",
                principalColumn: "ManufacturerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Manufacturer_ManufacturerId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ManufacturerId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ManufacturerId",
                table: "AspNetUsers");
        }
    }
}
