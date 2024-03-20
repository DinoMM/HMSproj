using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class Sklad2v3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PolozkaSkladuMnozstva",
                table: "PolozkaSkladuMnozstva");

            migrationBuilder.AddColumn<long>(
                name: "ID",
                table: "PolozkaSkladuMnozstva",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Iban",
                table: "Dodavatelia",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PolozkaSkladuMnozstva",
                table: "PolozkaSkladuMnozstva",
                column: "ID");

            migrationBuilder.CreateTable(
                name: "PohSkup",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Vznik = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Poznamka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Spracovana = table.Column<bool>(type: "bit", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Objednavka = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DodaciID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FakturaID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PohSkup", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PrijemkyPolozky",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PolozkaSkladuMnozstva = table.Column<long>(type: "bigint", nullable: false),
                    Skupina = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nazov = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mnozstvo = table.Column<double>(type: "float", nullable: false),
                    Cena = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrijemkyPolozky", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PrijemkyPolozky_PohSkup_Skupina",
                        column: x => x.Skupina,
                        principalTable: "PohSkup",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrijemkyPolozky_PolozkaSkladuMnozstva_PolozkaSkladuMnozstva",
                        column: x => x.PolozkaSkladuMnozstva,
                        principalTable: "PolozkaSkladuMnozstva",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PolozkaSkladuMnozstva_PolozkaSkladu",
                table: "PolozkaSkladuMnozstva",
                column: "PolozkaSkladu");

            migrationBuilder.CreateIndex(
                name: "IX_PrijemkyPolozky_PolozkaSkladuMnozstva",
                table: "PrijemkyPolozky",
                column: "PolozkaSkladuMnozstva");

            migrationBuilder.CreateIndex(
                name: "IX_PrijemkyPolozky_Skupina",
                table: "PrijemkyPolozky",
                column: "Skupina");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrijemkyPolozky");

            migrationBuilder.DropTable(
                name: "PohSkup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PolozkaSkladuMnozstva",
                table: "PolozkaSkladuMnozstva");

            migrationBuilder.DropIndex(
                name: "IX_PolozkaSkladuMnozstva_PolozkaSkladu",
                table: "PolozkaSkladuMnozstva");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "PolozkaSkladuMnozstva");

            migrationBuilder.DropColumn(
                name: "Iban",
                table: "Dodavatelia");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PolozkaSkladuMnozstva",
                table: "PolozkaSkladuMnozstva",
                columns: new[] { "PolozkaSkladu", "Sklad" });
        }
    }
}
