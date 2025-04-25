using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class Sklad4v15 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "PolozkaSkladuMnozstva",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "INDX_PolozkaSkladuMnozstva_Active",
                table: "PolozkaSkladuMnozstva",
                column: "Active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "INDX_PolozkaSkladuMnozstva_Active",
                table: "PolozkaSkladuMnozstva");

            migrationBuilder.DropColumn(
                name: "Active",
                table: "PolozkaSkladuMnozstva");
        }
    }
}
