using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class DataProtection3V9999 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Passport",
                table: "Hostia",
                type: "nvarchar(256)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)");

            migrationBuilder.AlterColumn<string>(
                name: "CitizenID",
                table: "Hostia",
                type: "nvarchar(256)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)");

            migrationBuilder.AlterColumn<string>(
                name: "BirthNumber",
                table: "Hostia",
                type: "nvarchar(256)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Hostia",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(192)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Passport",
                table: "Hostia",
                type: "nvarchar(32)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)");

            migrationBuilder.AlterColumn<string>(
                name: "CitizenID",
                table: "Hostia",
                type: "nvarchar(32)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)");

            migrationBuilder.AlterColumn<string>(
                name: "BirthNumber",
                table: "Hostia",
                type: "nvarchar(32)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Hostia",
                type: "nvarchar(192)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
