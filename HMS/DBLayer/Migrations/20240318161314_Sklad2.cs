using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class Sklad2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mnozstvo",
                table: "PolozkySkladu");

            migrationBuilder.CreateTable(
                name: "Sklady",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nazov = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Obdobie = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sklady", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PolozkaSkladuMnozstva",
                columns: table => new
                {
                    PolozkaSkladu = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Sklad = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Mnozstvo = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolozkaSkladuMnozstva", x => new { x.PolozkaSkladu, x.Sklad });
                    table.ForeignKey(
                        name: "FK_PolozkaSkladuMnozstva_PolozkySkladu_PolozkaSkladu",
                        column: x => x.PolozkaSkladu,
                        principalTable: "PolozkySkladu",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PolozkaSkladuMnozstva_Sklady_Sklad",
                        column: x => x.Sklad,
                        principalTable: "Sklady",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SkladUzivatelia",
                columns: table => new
                {
                    Sklad = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Uzivatel = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkladUzivatelia", x => new { x.Sklad, x.Uzivatel });
                    table.ForeignKey(
                        name: "FK_SkladUzivatelia_PolozkySkladu_Uzivatel",
                        column: x => x.Uzivatel,
                        principalTable: "PolozkySkladu",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SkladUzivatelia_Sklady_Sklad",
                        column: x => x.Sklad,
                        principalTable: "Sklady",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PolozkaSkladuMnozstva_Sklad",
                table: "PolozkaSkladuMnozstva",
                column: "Sklad");

            migrationBuilder.CreateIndex(
                name: "IX_SkladUzivatelia_Uzivatel",
                table: "SkladUzivatelia",
                column: "Uzivatel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PolozkaSkladuMnozstva");

            migrationBuilder.DropTable(
                name: "SkladUzivatelia");

            migrationBuilder.DropTable(
                name: "Sklady");

            migrationBuilder.AddColumn<double>(
                name: "Mnozstvo",
                table: "PolozkySkladu",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
