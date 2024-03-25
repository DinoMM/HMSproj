using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class Sklad2v6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SkladObdobi",
                table: "SkladObdobi");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SkladObdobi",
                table: "SkladObdobi",
                columns: new[] { "Obdobie", "Sklad" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SkladObdobi",
                table: "SkladObdobi");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SkladObdobi",
                table: "SkladObdobi",
                column: "Obdobie");
        }
    }
}
