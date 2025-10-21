using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamPlaytimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class Renamed_Local_Apps_To_User_Apps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocalApps_SteamStoreApps_SteamStoreAppId",
                table: "LocalApps");

            migrationBuilder.DropForeignKey(
                name: "FK_PlaytimeSlices_LocalApps_SteamAppEntryId",
                table: "PlaytimeSlices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LocalApps",
                table: "LocalApps");

            migrationBuilder.RenameTable(
                name: "LocalApps",
                newName: "UserApps");

            migrationBuilder.RenameIndex(
                name: "IX_LocalApps_SteamStoreAppId",
                table: "UserApps",
                newName: "IX_UserApps_SteamStoreAppId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserApps",
                table: "UserApps",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlaytimeSlices_UserApps_SteamAppEntryId",
                table: "PlaytimeSlices",
                column: "SteamAppEntryId",
                principalTable: "UserApps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserApps_SteamStoreApps_SteamStoreAppId",
                table: "UserApps",
                column: "SteamStoreAppId",
                principalTable: "SteamStoreApps",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlaytimeSlices_UserApps_SteamAppEntryId",
                table: "PlaytimeSlices");

            migrationBuilder.DropForeignKey(
                name: "FK_UserApps_SteamStoreApps_SteamStoreAppId",
                table: "UserApps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserApps",
                table: "UserApps");

            migrationBuilder.RenameTable(
                name: "UserApps",
                newName: "LocalApps");

            migrationBuilder.RenameIndex(
                name: "IX_UserApps_SteamStoreAppId",
                table: "LocalApps",
                newName: "IX_LocalApps_SteamStoreAppId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LocalApps",
                table: "LocalApps",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LocalApps_SteamStoreApps_SteamStoreAppId",
                table: "LocalApps",
                column: "SteamStoreAppId",
                principalTable: "SteamStoreApps",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlaytimeSlices_LocalApps_SteamAppEntryId",
                table: "PlaytimeSlices",
                column: "SteamAppEntryId",
                principalTable: "LocalApps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
