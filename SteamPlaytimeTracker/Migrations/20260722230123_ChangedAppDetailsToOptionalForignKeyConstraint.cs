using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamPlaytimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class ChangedAppDetailsToOptionalForignKeyConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SteamStoreAppsData_SteamStoreAppsDetails_SteamAppStoreDetailsId",
                table: "SteamStoreAppsData");

            migrationBuilder.AlterColumn<int>(
                name: "SteamAppStoreDetailsId",
                table: "SteamStoreAppsData",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_SteamStoreAppsData_SteamStoreAppsDetails_SteamAppStoreDetailsId",
                table: "SteamStoreAppsData",
                column: "SteamAppStoreDetailsId",
                principalTable: "SteamStoreAppsDetails",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SteamStoreAppsData_SteamStoreAppsDetails_SteamAppStoreDetailsId",
                table: "SteamStoreAppsData");

            migrationBuilder.AlterColumn<int>(
                name: "SteamAppStoreDetailsId",
                table: "SteamStoreAppsData",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SteamStoreAppsData_SteamStoreAppsDetails_SteamAppStoreDetailsId",
                table: "SteamStoreAppsData",
                column: "SteamAppStoreDetailsId",
                principalTable: "SteamStoreAppsDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
