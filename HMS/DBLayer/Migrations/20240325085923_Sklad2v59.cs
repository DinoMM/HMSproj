using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class Sklad2v59 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SkladObdobi",
                columns: table => new
                {
                    Obdobie = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Sklad = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkladObdobi", x => x.Obdobie);
                    table.ForeignKey(
                        name: "FK_SkladObdobi_Sklady_Sklad",
                        column: x => x.Sklad,
                        principalTable: "Sklady",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SkladObdobi_Sklad",
                table: "SkladObdobi",
                column: "Sklad");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SkladObdobi");
        }
    }
}
