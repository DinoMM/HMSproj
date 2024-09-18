using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class Uctovny3v92 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PolozkaSkladu",
                table: "UniConItemyPoklDokladu",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PredajnaCena",
                table: "UniConItemyPoklDokladu",
                type: "decimal(18,3)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PredajneDPH",
                table: "UniConItemyPoklDokladu",
                type: "decimal(9,3)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DPH",
                table: "ItemyPokladDokladu",
                type: "decimal(9,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateTable(
                name: "UniConItemPoklDokladuFlagy",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NumericValue = table.Column<double>(type: "float", nullable: true),
                    StringValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateValue = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniConItemPoklDokladuFlagy", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "UniConItemPoklDokladuConFlagy",
                columns: table => new
                {
                    UniConItemPoklDokladu = table.Column<long>(type: "bigint", nullable: false),
                    UniConItemPoklDokladuFlag = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniConItemPoklDokladuConFlagy", x => new { x.UniConItemPoklDokladu, x.UniConItemPoklDokladuFlag });
                    table.ForeignKey(
                        name: "FK_UniConItemPoklDokladuConFlagy_UniConItemPoklDokladuFlagy_UniConItemPoklDokladuFlag",
                        column: x => x.UniConItemPoklDokladuFlag,
                        principalTable: "UniConItemPoklDokladuFlagy",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UniConItemPoklDokladuConFlagy_UniConItemyPoklDokladu_UniConItemPoklDokladu",
                        column: x => x.UniConItemPoklDokladu,
                        principalTable: "UniConItemyPoklDokladu",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UniConItemyPoklDokladu_PolozkaSkladu",
                table: "UniConItemyPoklDokladu",
                column: "PolozkaSkladu",
                unique: true,
                filter: "[PolozkaSkladu] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UniConItemPoklDokladuConFlagy_UniConItemPoklDokladuFlag",
                table: "UniConItemPoklDokladuConFlagy",
                column: "UniConItemPoklDokladuFlag");

            migrationBuilder.AddForeignKey(
                name: "FK_UniConItemyPoklDokladu_PolozkySkladu_PolozkaSkladu",
                table: "UniConItemyPoklDokladu",
                column: "PolozkaSkladu",
                principalTable: "PolozkySkladu",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UniConItemyPoklDokladu_PolozkySkladu_PolozkaSkladu",
                table: "UniConItemyPoklDokladu");

            migrationBuilder.DropTable(
                name: "UniConItemPoklDokladuConFlagy");

            migrationBuilder.DropTable(
                name: "UniConItemPoklDokladuFlagy");

            migrationBuilder.DropIndex(
                name: "IX_UniConItemyPoklDokladu_PolozkaSkladu",
                table: "UniConItemyPoklDokladu");

            migrationBuilder.DropColumn(
                name: "PolozkaSkladu",
                table: "UniConItemyPoklDokladu");

            migrationBuilder.DropColumn(
                name: "PredajnaCena",
                table: "UniConItemyPoklDokladu");

            migrationBuilder.DropColumn(
                name: "PredajneDPH",
                table: "UniConItemyPoklDokladu");

            migrationBuilder.AlterColumn<decimal>(
                name: "DPH",
                table: "ItemyPokladDokladu",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(9,3)");
        }
    }
}
