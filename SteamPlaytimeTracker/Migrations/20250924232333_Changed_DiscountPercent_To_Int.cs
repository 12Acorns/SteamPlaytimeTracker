using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamPlaytimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class Changed_DiscountPercent_To_Int : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DiscountPercent",
                table: "StoreAppPriceDetails",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "DiscountPercent",
                table: "StoreAppPriceDetails",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }
    }
}
