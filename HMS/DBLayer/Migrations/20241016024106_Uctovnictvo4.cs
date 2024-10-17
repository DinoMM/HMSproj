using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class Uctovnictvo4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ZmenyMien",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Vznik = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MenaZ = table.Column<string>(type: "nvarchar(16)", nullable: false),
                    MenaDO = table.Column<string>(type: "nvarchar(16)", nullable: false),
                    Kurz = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SumaZ = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZmenyMien", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ZmenyMien");
        }
    }
}
