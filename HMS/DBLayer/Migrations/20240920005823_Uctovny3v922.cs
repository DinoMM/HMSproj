using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DBLayer.Migrations
{
    /// <inheritdoc />
    public partial class Uctovny3v922 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UniConItemyPoklDokladu_PolozkySkladu_PolozkaSkladu",
                table: "UniConItemyPoklDokladu");

            migrationBuilder.DropIndex(
                name: "IX_UniConItemyPoklDokladu_PolozkaSkladu",
                table: "UniConItemyPoklDokladu");

            migrationBuilder.DropColumn(
                name: "PolozkaSkladu",
                table: "UniConItemyPoklDokladu");

            migrationBuilder.AddColumn<long>(
                name: "PolozkaSkladuMnozstva",
                table: "UniConItemyPoklDokladu",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nazov",
                table: "ItemyPokladDokladu",
                type: "nvarchar(196)",
                maxLength: 196,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UniConItemyPoklDokladu_PolozkaSkladuMnozstva",
                table: "UniConItemyPoklDokladu",
                column: "PolozkaSkladuMnozstva",
                unique: true,
                filter: "[PolozkaSkladuMnozstva] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_UniConItemyPoklDokladu_PolozkaSkladuMnozstva_PolozkaSkladuMnozstva",
                table: "UniConItemyPoklDokladu",
                column: "PolozkaSkladuMnozstva",
                principalTable: "PolozkaSkladuMnozstva",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UniConItemyPoklDokladu_PolozkaSkladuMnozstva_PolozkaSkladuMnozstva",
                table: "UniConItemyPoklDokladu");

            migrationBuilder.DropIndex(
                name: "IX_UniConItemyPoklDokladu_PolozkaSkladuMnozstva",
                table: "UniConItemyPoklDokladu");

            migrationBuilder.DropColumn(
                name: "PolozkaSkladuMnozstva",
                table: "UniConItemyPoklDokladu");

            migrationBuilder.AddColumn<string>(
                name: "PolozkaSkladu",
                table: "UniConItemyPoklDokladu",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nazov",
                table: "ItemyPokladDokladu",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(196)",
                oldMaxLength: 196,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UniConItemyPoklDokladu_PolozkaSkladu",
                table: "UniConItemyPoklDokladu",
                column: "PolozkaSkladu",
                unique: true,
                filter: "[PolozkaSkladu] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_UniConItemyPoklDokladu_PolozkySkladu_PolozkaSkladu",
                table: "UniConItemyPoklDokladu",
                column: "PolozkaSkladu",
                principalTable: "PolozkySkladu",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
