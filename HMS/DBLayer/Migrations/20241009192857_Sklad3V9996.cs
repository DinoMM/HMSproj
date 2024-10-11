using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class Sklad3V9996 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cena",
                table: "PolozkySkladu");

            migrationBuilder.AddColumn<decimal>(
                name: "Cena",
                table: "PolozkaSkladuMnozstva",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cena",
                table: "PolozkaSkladuMnozstva");

            migrationBuilder.AddColumn<decimal>(
                name: "Cena",
                table: "PolozkySkladu",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
