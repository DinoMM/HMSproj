using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations.Data
{
    /// <inheritdoc />
    public partial class Recepcia3v86 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rezervations_AspNetUsers_GuestId",
                table: "Rezervations");

            migrationBuilder.AlterColumn<string>(
                name: "GuestId",
                table: "Rezervations",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervations_AspNetUsers_GuestId",
                table: "Rezervations",
                column: "GuestId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rezervations_AspNetUsers_GuestId",
                table: "Rezervations");

            migrationBuilder.AlterColumn<string>(
                name: "GuestId",
                table: "Rezervations",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervations_AspNetUsers_GuestId",
                table: "Rezervations",
                column: "GuestId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
