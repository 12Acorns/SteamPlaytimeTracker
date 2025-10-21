using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamPlaytimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class Please : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgeLiteral",
                table: "SteamAppStoreDetails");

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "SteamAppStoreDetails",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "SteamAppStoreDetails");

            migrationBuilder.AddColumn<string>(
                name: "AgeLiteral",
                table: "SteamAppStoreDetails",
                type: "TEXT",
                nullable: true);
        }
    }
}
