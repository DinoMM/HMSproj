using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class Recepcia3v82 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HostFlags",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NumericValue = table.Column<double>(type: "float", nullable: true),
                    StringValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateValue = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostFlags", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Hostia",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(128)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(192)", nullable: false),
                    BirthNumber = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    Passport = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    CitizenID = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Guest = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hostia", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "RoomFlags",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NumericValue = table.Column<double>(type: "float", nullable: true),
                    StringValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateValue = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomFlags", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "HostConFlags",
                columns: table => new
                {
                    Host = table.Column<long>(type: "bigint", nullable: false),
                    HostFlag = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostConFlags", x => new { x.Host, x.HostFlag });
                    table.ForeignKey(
                        name: "FK_HostConFlags_HostFlags_HostFlag",
                        column: x => x.HostFlag,
                        principalTable: "HostFlags",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HostConFlags_Hostia_Host",
                        column: x => x.Host,
                        principalTable: "Hostia",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HostConReservations",
                columns: table => new
                {
                    Host = table.Column<long>(type: "bigint", nullable: false),
                    Reservation = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostConReservations", x => new { x.Host, x.Reservation });
                    table.ForeignKey(
                        name: "FK_HostConReservations_Hostia_Host",
                        column: x => x.Host,
                        principalTable: "Hostia",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoomConFlags",
                columns: table => new
                {
                    Room = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoomFlag = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomConFlags", x => new { x.Room, x.RoomFlag });
                    table.ForeignKey(
                        name: "FK_RoomConFlags_RoomFlags_RoomFlag",
                        column: x => x.RoomFlag,
                        principalTable: "RoomFlags",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HostConFlags_HostFlag",
                table: "HostConFlags",
                column: "HostFlag");

            migrationBuilder.CreateIndex(
                name: "IX_RoomConFlags_RoomFlag",
                table: "RoomConFlags",
                column: "RoomFlag");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HostConFlags");

            migrationBuilder.DropTable(
                name: "HostConReservations");

            migrationBuilder.DropTable(
                name: "RoomConFlags");

            migrationBuilder.DropTable(
                name: "HostFlags");

            migrationBuilder.DropTable(
                name: "Hostia");

            migrationBuilder.DropTable(
                name: "RoomFlags");
        }
    }
}
