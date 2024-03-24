using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class Sklad2v51 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PohSkup_Sklady_Sklad",
                table: "PohSkup");

            migrationBuilder.DropForeignKey(
                name: "FK_PrijemkyPolozky_PohSkup_Skupina",
                table: "PrijemkyPolozky");

            migrationBuilder.DropForeignKey(
                name: "FK_PrijemkyPolozky_PolozkySkladu_PolozkaSkladu",
                table: "PrijemkyPolozky");

            migrationBuilder.DropIndex(
                name: "IX_PohSkup_Sklad",
                table: "PohSkup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PrijemkyPolozky",
                table: "PrijemkyPolozky");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "PohSkup");

            migrationBuilder.DropColumn(
                name: "DodaciID",
                table: "PohSkup");

            migrationBuilder.DropColumn(
                name: "FakturaID",
                table: "PohSkup");

            migrationBuilder.DropColumn(
                name: "Objednavka",
                table: "PohSkup");

            migrationBuilder.DropColumn(
                name: "Sklad",
                table: "PohSkup");

            migrationBuilder.RenameTable(
                name: "PrijemkyPolozky",
                newName: "PrijemkaPolozka");

            migrationBuilder.RenameIndex(
                name: "IX_PrijemkyPolozky_Skupina",
                table: "PrijemkaPolozka",
                newName: "IX_PrijemkaPolozka_Skupina");

            migrationBuilder.RenameIndex(
                name: "IX_PrijemkyPolozky_PolozkaSkladu",
                table: "PrijemkaPolozka",
                newName: "IX_PrijemkaPolozka_PolozkaSkladu");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PrijemkaPolozka",
                table: "PrijemkaPolozka",
                column: "ID");

            migrationBuilder.CreateTable(
                name: "Prijemky",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Objednavka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DodaciID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FakturaID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sklad = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prijemky", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Prijemky_Sklady_Sklad",
                        column: x => x.Sklad,
                        principalTable: "Sklady",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vydajka",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Sklad = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SkladDo = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vydajka", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Vydajka_PohSkup_ID",
                        column: x => x.ID,
                        principalTable: "PohSkup",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vydajka_Sklady_Sklad",
                        column: x => x.Sklad,
                        principalTable: "Sklady",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vydajka_Sklady_SkladDo",
                        column: x => x.SkladDo,
                        principalTable: "Sklady",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prijemky_Sklad",
                table: "Prijemky",
                column: "Sklad");

            migrationBuilder.CreateIndex(
                name: "IX_Vydajka_Sklad",
                table: "Vydajka",
                column: "Sklad");

            migrationBuilder.CreateIndex(
                name: "IX_Vydajka_SkladDo",
                table: "Vydajka",
                column: "SkladDo");

            migrationBuilder.AddForeignKey(
                name: "FK_PrijemkaPolozka_PohSkup_Skupina",
                table: "PrijemkaPolozka",
                column: "Skupina",
                principalTable: "PohSkup",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PrijemkaPolozka_PolozkySkladu_PolozkaSkladu",
                table: "PrijemkaPolozka",
                column: "PolozkaSkladu",
                principalTable: "PolozkySkladu",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrijemkaPolozka_PohSkup_Skupina",
                table: "PrijemkaPolozka");

            migrationBuilder.DropForeignKey(
                name: "FK_PrijemkaPolozka_PolozkySkladu_PolozkaSkladu",
                table: "PrijemkaPolozka");

            migrationBuilder.DropTable(
                name: "Prijemky");

            migrationBuilder.DropTable(
                name: "Vydajka");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PrijemkaPolozka",
                table: "PrijemkaPolozka");

            migrationBuilder.RenameTable(
                name: "PrijemkaPolozka",
                newName: "PrijemkyPolozky");

            migrationBuilder.RenameIndex(
                name: "IX_PrijemkaPolozka_Skupina",
                table: "PrijemkyPolozky",
                newName: "IX_PrijemkyPolozky_Skupina");

            migrationBuilder.RenameIndex(
                name: "IX_PrijemkaPolozka_PolozkaSkladu",
                table: "PrijemkyPolozky",
                newName: "IX_PrijemkyPolozky_PolozkaSkladu");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "PohSkup",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DodaciID",
                table: "PohSkup",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FakturaID",
                table: "PohSkup",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Objednavka",
                table: "PohSkup",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sklad",
                table: "PohSkup",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PrijemkyPolozky",
                table: "PrijemkyPolozky",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_PohSkup_Sklad",
                table: "PohSkup",
                column: "Sklad");

            migrationBuilder.AddForeignKey(
                name: "FK_PohSkup_Sklady_Sklad",
                table: "PohSkup",
                column: "Sklad",
                principalTable: "Sklady",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PrijemkyPolozky_PohSkup_Skupina",
                table: "PrijemkyPolozky",
                column: "Skupina",
                principalTable: "PohSkup",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PrijemkyPolozky_PolozkySkladu_PolozkaSkladu",
                table: "PrijemkyPolozky",
                column: "PolozkaSkladu",
                principalTable: "PolozkySkladu",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
