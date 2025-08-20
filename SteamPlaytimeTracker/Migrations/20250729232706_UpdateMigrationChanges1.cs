using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamPlaytimeTracker.Migrations
{
	/// <inheritdoc />
	public partial class UpdateMigrationChanges1 : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_SteamAppEntries_SteamAppDTO_SteamAppDTOId",
				table: "SteamAppEntries");

			migrationBuilder.DropPrimaryKey(
				name: "PK_SteamAppDTO",
				table: "SteamAppDTO");

			migrationBuilder.RenameTable(
				name: "SteamAppDTO",
				newName: "AllSteamApps");

			migrationBuilder.AddPrimaryKey(
				name: "PK_AllSteamApps",
				table: "AllSteamApps",
				column: "SteamAppDTOId");

			migrationBuilder.AddForeignKey(
				name: "FK_SteamAppEntries_AllSteamApps_SteamAppDTOId",
				table: "SteamAppEntries",
				column: "SteamAppDTOId",
				principalTable: "AllSteamApps",
				principalColumn: "SteamAppDTOId",
				onDelete: ReferentialAction.Cascade);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_SteamAppEntries_AllSteamApps_SteamAppDTOId",
				table: "SteamAppEntries");

			migrationBuilder.DropPrimaryKey(
				name: "PK_AllSteamApps",
				table: "AllSteamApps");

			migrationBuilder.RenameTable(
				name: "AllSteamApps",
				newName: "SteamAppDTO");

			migrationBuilder.AddPrimaryKey(
				name: "PK_SteamAppDTO",
				table: "SteamAppDTO",
				column: "SteamAppDTOId");

			migrationBuilder.AddForeignKey(
				name: "FK_SteamAppEntries_SteamAppDTO_SteamAppDTOId",
				table: "SteamAppEntries",
				column: "SteamAppDTOId",
				principalTable: "SteamAppDTO",
				principalColumn: "SteamAppDTOId",
				onDelete: ReferentialAction.Cascade);
		}
	}
}
