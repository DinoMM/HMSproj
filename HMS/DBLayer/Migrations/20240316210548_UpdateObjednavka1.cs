using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class UpdateObjednavka1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Stav",
                table: "Objednavky",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Tvorca",
                table: "Objednavky",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Objednavky_Tvorca",
                table: "Objednavky",
                column: "Tvorca");

            migrationBuilder.AddForeignKey(
                name: "FK_Objednavky_AspNetUsers_Tvorca",
                table: "Objednavky",
                column: "Tvorca",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Objednavky_AspNetUsers_Tvorca",
                table: "Objednavky");

            migrationBuilder.DropIndex(
                name: "IX_Objednavky_Tvorca",
                table: "Objednavky");

            migrationBuilder.DropColumn(
                name: "Stav",
                table: "Objednavky");

            migrationBuilder.DropColumn(
                name: "Tvorca",
                table: "Objednavky");
        }
    }
}
