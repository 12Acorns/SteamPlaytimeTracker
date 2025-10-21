using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamPlaytimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class Added_Price_Overview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StoreAppPriceDetailsId",
                table: "SteamAppStoreDetails",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "StoreAppPriceDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: false),
                    InitialPrice = table.Column<int>(type: "INTEGER", nullable: false),
                    FinalPrice = table.Column<int>(type: "INTEGER", nullable: false),
                    DiscountPercent = table.Column<double>(type: "REAL", nullable: false),
                    InitialPriceText = table.Column<string>(type: "TEXT", nullable: false),
                    FinalPriceText = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreAppPriceDetails", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SteamAppStoreDetails_StoreAppPriceDetailsId",
                table: "SteamAppStoreDetails",
                column: "StoreAppPriceDetailsId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SteamAppStoreDetails_StoreAppPriceDetails_StoreAppPriceDetailsId",
                table: "SteamAppStoreDetails",
                column: "StoreAppPriceDetailsId",
                principalTable: "StoreAppPriceDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SteamAppStoreDetails_StoreAppPriceDetails_StoreAppPriceDetailsId",
                table: "SteamAppStoreDetails");

            migrationBuilder.DropTable(
                name: "StoreAppPriceDetails");

            migrationBuilder.DropIndex(
                name: "IX_SteamAppStoreDetails_StoreAppPriceDetailsId",
                table: "SteamAppStoreDetails");

            migrationBuilder.DropColumn(
                name: "StoreAppPriceDetailsId",
                table: "SteamAppStoreDetails");
        }
    }
}
