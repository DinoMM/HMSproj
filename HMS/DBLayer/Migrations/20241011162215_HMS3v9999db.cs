using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class HMS3v9999db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DruhPohybu",
                table: "Vydajka",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DruhPohybu",
                table: "Prijemky",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Host",
                table: "PokladnicneDoklady",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HotovostStav",
                table: "Kasy",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "AspNetUsers",
                type: "nvarchar(128)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ObcianskyID",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RodneCislo",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Sex",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DruhyPohybov",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nazov = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MD = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DAL = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DruhyPohybov", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vydajka_DruhPohybu",
                table: "Vydajka",
                column: "DruhPohybu");

            migrationBuilder.CreateIndex(
                name: "IX_Prijemky_DruhPohybu",
                table: "Prijemky",
                column: "DruhPohybu");

            migrationBuilder.CreateIndex(
                name: "IX_PokladnicneDoklady_Host",
                table: "PokladnicneDoklady",
                column: "Host");

            migrationBuilder.AddForeignKey(
                name: "FK_PokladnicneDoklady_Hostia_Host",
                table: "PokladnicneDoklady",
                column: "Host",
                principalTable: "Hostia",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Prijemky_DruhyPohybov_DruhPohybu",
                table: "Prijemky",
                column: "DruhPohybu",
                principalTable: "DruhyPohybov",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Vydajka_DruhyPohybov_DruhPohybu",
                table: "Vydajka",
                column: "DruhPohybu",
                principalTable: "DruhyPohybov",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PokladnicneDoklady_Hostia_Host",
                table: "PokladnicneDoklady");

            migrationBuilder.DropForeignKey(
                name: "FK_Prijemky_DruhyPohybov_DruhPohybu",
                table: "Prijemky");

            migrationBuilder.DropForeignKey(
                name: "FK_Vydajka_DruhyPohybov_DruhPohybu",
                table: "Vydajka");

            migrationBuilder.DropTable(
                name: "DruhyPohybov");

            migrationBuilder.DropIndex(
                name: "IX_Vydajka_DruhPohybu",
                table: "Vydajka");

            migrationBuilder.DropIndex(
                name: "IX_Prijemky_DruhPohybu",
                table: "Prijemky");

            migrationBuilder.DropIndex(
                name: "IX_PokladnicneDoklady_Host",
                table: "PokladnicneDoklady");

            migrationBuilder.DropColumn(
                name: "DruhPohybu",
                table: "Vydajka");

            migrationBuilder.DropColumn(
                name: "DruhPohybu",
                table: "Prijemky");

            migrationBuilder.DropColumn(
                name: "Host",
                table: "PokladnicneDoklady");

            migrationBuilder.DropColumn(
                name: "HotovostStav",
                table: "Kasy");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ObcianskyID",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RodneCislo",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "AspNetUsers");
        }
    }
}
