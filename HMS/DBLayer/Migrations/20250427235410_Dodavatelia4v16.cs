using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class Dodavatelia4v16 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DIC",
                table: "Dodavatelia",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IC_DPH",
                table: "Dodavatelia",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Poznámka",
                table: "Dodavatelia",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DIC",
                table: "Dodavatelia");

            migrationBuilder.DropColumn(
                name: "IC_DPH",
                table: "Dodavatelia");

            migrationBuilder.DropColumn(
                name: "Poznámka",
                table: "Dodavatelia");
        }
    }
}
