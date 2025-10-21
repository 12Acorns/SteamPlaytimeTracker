using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamPlaytimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class Price_Details_FK_Optional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SteamAppStoreDetails_StoreAppPriceDetails_StoreAppPriceDetailsId",
                table: "SteamAppStoreDetails");

            migrationBuilder.AlterColumn<int>(
                name: "StoreAppPriceDetailsId",
                table: "SteamAppStoreDetails",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_SteamAppStoreDetails_StoreAppPriceDetails_StoreAppPriceDetailsId",
                table: "SteamAppStoreDetails",
                column: "StoreAppPriceDetailsId",
                principalTable: "StoreAppPriceDetails",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SteamAppStoreDetails_StoreAppPriceDetails_StoreAppPriceDetailsId",
                table: "SteamAppStoreDetails");

            migrationBuilder.AlterColumn<int>(
                name: "StoreAppPriceDetailsId",
                table: "SteamAppStoreDetails",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SteamAppStoreDetails_StoreAppPriceDetails_StoreAppPriceDetailsId",
                table: "SteamAppStoreDetails",
                column: "StoreAppPriceDetailsId",
                principalTable: "StoreAppPriceDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
