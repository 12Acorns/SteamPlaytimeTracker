using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamPlaytimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class CorrectedRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SteamApps_LocalApps_SteamAppEntryId",
                table: "SteamApps");

            migrationBuilder.DropIndex(
                name: "IX_SteamApps_SteamAppEntryId",
                table: "SteamApps");

            migrationBuilder.DropColumn(
                name: "SteamAppEntryId",
                table: "SteamApps");

            migrationBuilder.AddColumn<int>(
                name: "SteamAppId",
                table: "LocalApps",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LocalApps_SteamAppId",
                table: "LocalApps",
                column: "SteamAppId");

            migrationBuilder.AddForeignKey(
                name: "FK_LocalApps_SteamApps_SteamAppId",
                table: "LocalApps",
                column: "SteamAppId",
                principalTable: "SteamApps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocalApps_SteamApps_SteamAppId",
                table: "LocalApps");

            migrationBuilder.DropIndex(
                name: "IX_LocalApps_SteamAppId",
                table: "LocalApps");

            migrationBuilder.DropColumn(
                name: "SteamAppId",
                table: "LocalApps");

            migrationBuilder.AddColumn<int>(
                name: "SteamAppEntryId",
                table: "SteamApps",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SteamApps_SteamAppEntryId",
                table: "SteamApps",
                column: "SteamAppEntryId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SteamApps_LocalApps_SteamAppEntryId",
                table: "SteamApps",
                column: "SteamAppEntryId",
                principalTable: "LocalApps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
