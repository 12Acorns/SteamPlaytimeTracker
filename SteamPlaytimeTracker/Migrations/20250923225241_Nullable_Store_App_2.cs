using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamPlaytimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class Nullable_Store_App_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.DropForeignKey(
				name: "FK_LocalApps_SteamStoreApps_SteamStoreAppId",
				table: "LocalApps");

			migrationBuilder.AlterColumn<int>(
				name: "SteamStoreAppId",
				table: "LocalApps",
				type: "INTEGER",
				nullable: true,
				oldClrType: typeof(int),
				oldType: "INTEGER");

			migrationBuilder.AddForeignKey(
				name: "FK_LocalApps_SteamStoreApps_SteamStoreAppId",
				table: "LocalApps",
				column: "SteamStoreAppId",
				principalTable: "SteamStoreApps",
				principalColumn: "Id");

			migrationBuilder.Sql("PRAGMA foreign_keys = ON;", suppressTransaction: true);
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql("PRAGMA foreign_keys = OFF;", suppressTransaction: true);

			migrationBuilder.DropForeignKey(
				name: "FK_LocalApps_SteamStoreApps_SteamStoreAppId",
				table: "LocalApps");

			migrationBuilder.AlterColumn<int>(
				name: "SteamStoreAppId",
				table: "LocalApps",
				type: "INTEGER",
				nullable: false,
				defaultValue: 0,
				oldClrType: typeof(int),
				oldType: "INTEGER",
				oldNullable: true);

			migrationBuilder.AddForeignKey(
				name: "FK_LocalApps_SteamStoreApps_SteamStoreAppId",
				table: "LocalApps",
				column: "SteamStoreAppId",
				principalTable: "SteamStoreApps",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.Sql("PRAGMA foreign_keys = ON;", suppressTransaction: true);
		}
	}
}
