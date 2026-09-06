using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamPlaytimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddedMissingStoreData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SteamStoreAppData_SteamAppStoreDetails_SteamAppStoreDetailsId",
                table: "SteamStoreAppData");

            migrationBuilder.DropForeignKey(
                name: "FK_SteamStoreApps_SteamStoreAppData_SteamStoreAppDataId",
                table: "SteamStoreApps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SteamStoreAppData",
                table: "SteamStoreAppData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SteamAppStoreDetails",
                table: "SteamAppStoreDetails");

            migrationBuilder.RenameTable(
                name: "SteamStoreAppData",
                newName: "SteamStoreAppsData");

            migrationBuilder.RenameTable(
                name: "SteamAppStoreDetails",
                newName: "SteamStoreAppsDetails");

            migrationBuilder.RenameIndex(
                name: "IX_SteamStoreAppData_SteamAppStoreDetailsId",
                table: "SteamStoreAppsData",
                newName: "IX_SteamStoreAppsData_SteamAppStoreDetailsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SteamStoreAppsData",
                table: "SteamStoreAppsData",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SteamStoreAppsDetails",
                table: "SteamStoreAppsDetails",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SteamStoreApps_SteamStoreAppsData_SteamStoreAppDataId",
                table: "SteamStoreApps",
                column: "SteamStoreAppDataId",
                principalTable: "SteamStoreAppsData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SteamStoreAppsData_SteamStoreAppsDetails_SteamAppStoreDetailsId",
                table: "SteamStoreAppsData",
                column: "SteamAppStoreDetailsId",
                principalTable: "SteamStoreAppsDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SteamStoreApps_SteamStoreAppsData_SteamStoreAppDataId",
                table: "SteamStoreApps");

            migrationBuilder.DropForeignKey(
                name: "FK_SteamStoreAppsData_SteamStoreAppsDetails_SteamAppStoreDetailsId",
                table: "SteamStoreAppsData");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SteamStoreAppsDetails",
                table: "SteamStoreAppsDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SteamStoreAppsData",
                table: "SteamStoreAppsData");

            migrationBuilder.RenameTable(
                name: "SteamStoreAppsDetails",
                newName: "SteamAppStoreDetails");

            migrationBuilder.RenameTable(
                name: "SteamStoreAppsData",
                newName: "SteamStoreAppData");

            migrationBuilder.RenameIndex(
                name: "IX_SteamStoreAppsData_SteamAppStoreDetailsId",
                table: "SteamStoreAppData",
                newName: "IX_SteamStoreAppData_SteamAppStoreDetailsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SteamAppStoreDetails",
                table: "SteamAppStoreDetails",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SteamStoreAppData",
                table: "SteamStoreAppData",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SteamStoreAppData_SteamAppStoreDetails_SteamAppStoreDetailsId",
                table: "SteamStoreAppData",
                column: "SteamAppStoreDetailsId",
                principalTable: "SteamAppStoreDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SteamStoreApps_SteamStoreAppData_SteamStoreAppDataId",
                table: "SteamStoreApps",
                column: "SteamStoreAppDataId",
                principalTable: "SteamStoreAppData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
