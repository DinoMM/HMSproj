using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class Uctovn3v61 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Faktury",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Skupina = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Vystavenie = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Splatnost = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OdKoho = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Spracovana = table.Column<bool>(type: "bit", nullable: false),
                    CenaBezDPH = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faktury", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Faktury_Dodavatelia_OdKoho",
                        column: x => x.OdKoho,
                        principalTable: "Dodavatelia",
                        principalColumn: "ICO",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Faktury_PohSkup_Skupina",
                        column: x => x.Skupina,
                        principalTable: "PohSkup",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Faktury_OdKoho",
                table: "Faktury",
                column: "OdKoho");

            migrationBuilder.CreateIndex(
                name: "IX_Faktury_Skupina",
                table: "Faktury",
                column: "Skupina");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Faktury");
        }
    }
}
