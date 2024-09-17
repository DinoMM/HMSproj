using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class Reception3v899 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kasy",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Dodavatel = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActualUser = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kasy", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Kasy_AspNetUsers_ActualUser",
                        column: x => x.ActualUser,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Kasy_Dodavatelia_Dodavatel",
                        column: x => x.Dodavatel,
                        principalTable: "Dodavatelia",
                        principalColumn: "ICO",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UniConItemyPoklDokladu",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Discriminator = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: false),
                    Reservation = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniConItemyPoklDokladu", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PokladnicneDoklady",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Kasa = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TypPlatby = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PokladnicneDoklady", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PokladnicneDoklady_Kasy_Kasa",
                        column: x => x.Kasa,
                        principalTable: "Kasy",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "ItemyPokladDokladu",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Skupina = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nazov = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mnozstvo = table.Column<double>(type: "float", nullable: false),
                    Cena = table.Column<double>(type: "float", nullable: false),
                    UniConItemPoklDokladu = table.Column<long>(type: "bigint", nullable: false),
                    DPH = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemyPokladDokladu", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ItemyPokladDokladu_PohSkup_Skupina",
                        column: x => x.Skupina,
                        principalTable: "PohSkup",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemyPokladDokladu_UniConItemyPoklDokladu_UniConItemPoklDokladu",
                        column: x => x.UniConItemPoklDokladu,
                        principalTable: "UniConItemyPoklDokladu",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemyPokladDokladu_Skupina",
                table: "ItemyPokladDokladu",
                column: "Skupina");

            migrationBuilder.CreateIndex(
                name: "IX_ItemyPokladDokladu_UniConItemPoklDokladu",
                table: "ItemyPokladDokladu",
                column: "UniConItemPoklDokladu");

            migrationBuilder.CreateIndex(
                name: "IX_Kasy_ActualUser",
                table: "Kasy",
                column: "ActualUser");

            migrationBuilder.CreateIndex(
                name: "IX_Kasy_Dodavatel",
                table: "Kasy",
                column: "Dodavatel");

            migrationBuilder.CreateIndex(
                name: "IX_PokladnicneDoklady_Kasa",
                table: "PokladnicneDoklady",
                column: "Kasa");

            migrationBuilder.CreateIndex(
                name: "IX_UniConItemyPoklDokladu_Reservation",
                table: "UniConItemyPoklDokladu",
                column: "Reservation",
                unique: true,
                filter: "[Reservation] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemyPokladDokladu");

            migrationBuilder.DropTable(
                name: "PokladnicneDoklady");

            migrationBuilder.DropTable(
                name: "UniConItemyPoklDokladu");

            migrationBuilder.DropTable(
                name: "Kasy");
        }
    }
}
