using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class UctovnictvoDPH4v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Faktury_PohSkup_Skupina",
                table: "Faktury");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Faktury",
                table: "Faktury");

            migrationBuilder.AddColumn<decimal>(
                name: "DPH",
                table: "PrijemkaPolozka",
                type: "decimal(9,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DPH_mask",
                table: "PolozkySkladu",
                type: "decimal(9,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<bool>(
                name: "Spracovana",
                table: "Faktury",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<string>(
                name: "Adresa",
                table: "Faktury",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "CenaSDPH",
                table: "Faktury",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CisloObjednavky",
                table: "Faktury",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DIC",
                table: "Faktury",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "Dodanie",
                table: "Faktury",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Faktury",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FormaUhrady",
                table: "Faktury",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "Faktury",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ICO",
                table: "Faktury",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IC_DPH",
                table: "Faktury",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KonstantnySymbol",
                table: "Faktury",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nazov",
                table: "Faktury",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SpecifickySymbol",
                table: "Faktury",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefon",
                table: "Faktury",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VariabilnySymbol",
                table: "Faktury",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Faktury",
                table: "Faktury",
                columns: new[] { "ID", "Skupina" });

            migrationBuilder.AddForeignKey(
                name: "FK_Faktury_PohSkup_Skupina",
                table: "Faktury",
                column: "Skupina",
                principalTable: "PohSkup",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Faktury_PohSkup_Skupina",
                table: "Faktury");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Faktury",
                table: "Faktury");

            migrationBuilder.DropColumn(
                name: "DPH",
                table: "PrijemkaPolozka");

            migrationBuilder.DropColumn(
                name: "DPH_mask",
                table: "PolozkySkladu");

            migrationBuilder.DropColumn(
                name: "Adresa",
                table: "Faktury");

            migrationBuilder.DropColumn(
                name: "CenaSDPH",
                table: "Faktury");

            migrationBuilder.DropColumn(
                name: "CisloObjednavky",
                table: "Faktury");

            migrationBuilder.DropColumn(
                name: "DIC",
                table: "Faktury");

            migrationBuilder.DropColumn(
                name: "Dodanie",
                table: "Faktury");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Faktury");

            migrationBuilder.DropColumn(
                name: "FormaUhrady",
                table: "Faktury");

            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "Faktury");

            migrationBuilder.DropColumn(
                name: "ICO",
                table: "Faktury");

            migrationBuilder.DropColumn(
                name: "IC_DPH",
                table: "Faktury");

            migrationBuilder.DropColumn(
                name: "KonstantnySymbol",
                table: "Faktury");

            migrationBuilder.DropColumn(
                name: "Nazov",
                table: "Faktury");

            migrationBuilder.DropColumn(
                name: "SpecifickySymbol",
                table: "Faktury");

            migrationBuilder.DropColumn(
                name: "Telefon",
                table: "Faktury");

            migrationBuilder.DropColumn(
                name: "VariabilnySymbol",
                table: "Faktury");

            migrationBuilder.AlterColumn<bool>(
                name: "Spracovana",
                table: "Faktury",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Faktury",
                table: "Faktury",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Faktury_PohSkup_Skupina",
                table: "Faktury",
                column: "Skupina",
                principalTable: "PohSkup",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
