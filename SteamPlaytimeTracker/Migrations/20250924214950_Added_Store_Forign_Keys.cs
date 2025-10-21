using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamPlaytimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class Added_Store_Forign_Keys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SteamStoreAppData_SteamAppStoreDetails_StoreDataId",
                table: "SteamStoreAppData");

            migrationBuilder.DropForeignKey(
                name: "FK_SteamStoreApps_SteamStoreAppData_AppDataId",
                table: "SteamStoreApps");

            migrationBuilder.DropIndex(
                name: "IX_SteamStoreApps_AppDataId",
                table: "SteamStoreApps");

            migrationBuilder.DropIndex(
                name: "IX_SteamStoreAppData_StoreDataId",
                table: "SteamStoreAppData");

            migrationBuilder.RenameColumn(
                name: "AppDataId",
                table: "SteamStoreApps",
                newName: "SteamAppEntryId");

            migrationBuilder.RenameColumn(
                name: "StoreDataId",
                table: "SteamStoreAppData",
                newName: "SteamAppStoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_SteamStoreApps_SteamAppEntryId",
                table: "SteamStoreApps",
                column: "SteamAppEntryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SteamStoreAppData_SteamAppStoreDetailsId",
                table: "SteamStoreAppData",
                column: "SteamAppStoreDetailsId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SteamStoreAppData_SteamAppStoreDetails_SteamAppStoreDetailsId",
                table: "SteamStoreAppData",
                column: "SteamAppStoreDetailsId",
                principalTable: "SteamAppStoreDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SteamStoreApps_SteamStoreAppData_SteamAppEntryId",
                table: "SteamStoreApps",
                column: "SteamAppEntryId",
                principalTable: "SteamStoreAppData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SteamStoreAppData_SteamAppStoreDetails_SteamAppStoreDetailsId",
                table: "SteamStoreAppData");

            migrationBuilder.DropForeignKey(
                name: "FK_SteamStoreApps_SteamStoreAppData_SteamAppEntryId",
                table: "SteamStoreApps");

            migrationBuilder.DropIndex(
                name: "IX_SteamStoreApps_SteamAppEntryId",
                table: "SteamStoreApps");

            migrationBuilder.DropIndex(
                name: "IX_SteamStoreAppData_SteamAppStoreDetailsId",
                table: "SteamStoreAppData");

            migrationBuilder.RenameColumn(
                name: "SteamAppEntryId",
                table: "SteamStoreApps",
                newName: "AppDataId");

            migrationBuilder.RenameColumn(
                name: "SteamAppStoreDetailsId",
                table: "SteamStoreAppData",
                newName: "StoreDataId");

            migrationBuilder.CreateIndex(
                name: "IX_SteamStoreApps_AppDataId",
                table: "SteamStoreApps",
                column: "AppDataId");

            migrationBuilder.CreateIndex(
                name: "IX_SteamStoreAppData_StoreDataId",
                table: "SteamStoreAppData",
                column: "StoreDataId");

            migrationBuilder.AddForeignKey(
                name: "FK_SteamStoreAppData_SteamAppStoreDetails_StoreDataId",
                table: "SteamStoreAppData",
                column: "StoreDataId",
                principalTable: "SteamAppStoreDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SteamStoreApps_SteamStoreAppData_AppDataId",
                table: "SteamStoreApps",
                column: "AppDataId",
                principalTable: "SteamStoreAppData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
