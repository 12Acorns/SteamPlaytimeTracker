using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamPlaytimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class Fixed_FK_Name : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SteamStoreApps_SteamStoreAppData_SteamAppEntryId",
                table: "SteamStoreApps");

            migrationBuilder.RenameColumn(
                name: "SteamAppEntryId",
                table: "SteamStoreApps",
                newName: "SteamStoreAppDataId");

            migrationBuilder.RenameIndex(
                name: "IX_SteamStoreApps_SteamAppEntryId",
                table: "SteamStoreApps",
                newName: "IX_SteamStoreApps_SteamStoreAppDataId");

            migrationBuilder.AddForeignKey(
                name: "FK_SteamStoreApps_SteamStoreAppData_SteamStoreAppDataId",
                table: "SteamStoreApps",
                column: "SteamStoreAppDataId",
                principalTable: "SteamStoreAppData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SteamStoreApps_SteamStoreAppData_SteamStoreAppDataId",
                table: "SteamStoreApps");

            migrationBuilder.RenameColumn(
                name: "SteamStoreAppDataId",
                table: "SteamStoreApps",
                newName: "SteamAppEntryId");

            migrationBuilder.RenameIndex(
                name: "IX_SteamStoreApps_SteamStoreAppDataId",
                table: "SteamStoreApps",
                newName: "IX_SteamStoreApps_SteamAppEntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_SteamStoreApps_SteamStoreAppData_SteamAppEntryId",
                table: "SteamStoreApps",
                column: "SteamAppEntryId",
                principalTable: "SteamStoreAppData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
