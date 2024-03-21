using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class Sklad2v5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrijemkyPolozky_PolozkaSkladuMnozstva_PolozkaSkladuMnozstva",
                table: "PrijemkyPolozky");

            migrationBuilder.DropIndex(
                name: "IX_PrijemkyPolozky_PolozkaSkladuMnozstva",
                table: "PrijemkyPolozky");

            migrationBuilder.DropColumn(
                name: "PolozkaSkladuMnozstva",
                table: "PrijemkyPolozky");

            migrationBuilder.AddColumn<string>(
                name: "PolozkaSkladu",
                table: "PrijemkyPolozky",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sklad",
                table: "PohSkup",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrijemkyPolozky_PolozkaSkladu",
                table: "PrijemkyPolozky",
                column: "PolozkaSkladu");

            migrationBuilder.CreateIndex(
                name: "IX_PohSkup_Sklad",
                table: "PohSkup",
                column: "Sklad");

            migrationBuilder.AddForeignKey(
                name: "FK_PohSkup_Sklady_Sklad",
                table: "PohSkup",
                column: "Sklad",
                principalTable: "Sklady",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PrijemkyPolozky_PolozkySkladu_PolozkaSkladu",
                table: "PrijemkyPolozky",
                column: "PolozkaSkladu",
                principalTable: "PolozkySkladu",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PohSkup_Sklady_Sklad",
                table: "PohSkup");

            migrationBuilder.DropForeignKey(
                name: "FK_PrijemkyPolozky_PolozkySkladu_PolozkaSkladu",
                table: "PrijemkyPolozky");

            migrationBuilder.DropIndex(
                name: "IX_PrijemkyPolozky_PolozkaSkladu",
                table: "PrijemkyPolozky");

            migrationBuilder.DropIndex(
                name: "IX_PohSkup_Sklad",
                table: "PohSkup");

            migrationBuilder.DropColumn(
                name: "PolozkaSkladu",
                table: "PrijemkyPolozky");

            migrationBuilder.DropColumn(
                name: "Sklad",
                table: "PohSkup");

            migrationBuilder.AddColumn<long>(
                name: "PolozkaSkladuMnozstva",
                table: "PrijemkyPolozky",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_PrijemkyPolozky_PolozkaSkladuMnozstva",
                table: "PrijemkyPolozky",
                column: "PolozkaSkladuMnozstva");

            migrationBuilder.AddForeignKey(
                name: "FK_PrijemkyPolozky_PolozkaSkladuMnozstva_PolozkaSkladuMnozstva",
                table: "PrijemkyPolozky",
                column: "PolozkaSkladuMnozstva",
                principalTable: "PolozkaSkladuMnozstva",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
