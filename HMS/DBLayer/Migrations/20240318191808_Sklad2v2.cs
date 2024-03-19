using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class Sklad2v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SkladUzivatelia_PolozkySkladu_Uzivatel",
                table: "SkladUzivatelia");

            migrationBuilder.AddForeignKey(
                name: "FK_SkladUzivatelia_AspNetUsers_Uzivatel",
                table: "SkladUzivatelia",
                column: "Uzivatel",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SkladUzivatelia_AspNetUsers_Uzivatel",
                table: "SkladUzivatelia");

            migrationBuilder.AddForeignKey(
                name: "FK_SkladUzivatelia_PolozkySkladu_Uzivatel",
                table: "SkladUzivatelia",
                column: "Uzivatel",
                principalTable: "PolozkySkladu",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
