using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamPlaytimeTracker.Migrations
{
	/// <inheritdoc />
	public partial class InitialCreate : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "SteamAppDTO",
				columns: table => new
				{
					SteamAppDTOId = table.Column<int>(type: "INTEGER", nullable: false)
						.Annotation("Sqlite:Autoincrement", true),
					AppId = table.Column<int>(type: "INTEGER", nullable: false),
					AppName = table.Column<string>(type: "TEXT", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SteamAppDTO", x => x.SteamAppDTOId);
				});

			migrationBuilder.CreateTable(
				name: "SteamAppEntries",
				columns: table => new
				{
					SteamAppEntryId = table.Column<int>(type: "INTEGER", nullable: false)
						.Annotation("Sqlite:Autoincrement", true),
					SteamAppDTOId = table.Column<int>(type: "INTEGER", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SteamAppEntries", x => x.SteamAppEntryId);
					table.ForeignKey(
						name: "FK_SteamAppEntries_SteamAppDTO_SteamAppDTOId",
						column: x => x.SteamAppDTOId,
						principalTable: "SteamAppDTO",
						principalColumn: "SteamAppDTOId",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "PlaytimeSliceDTO",
				columns: table => new
				{
					PlaytimeSliceDTOId = table.Column<int>(type: "INTEGER", nullable: false)
						.Annotation("Sqlite:Autoincrement", true),
					StartTimeTicks = table.Column<long>(type: "INTEGER", nullable: false),
					StartTimeOffsetMinutes = table.Column<short>(type: "INTEGER", nullable: false),
					SessionTimeTicks = table.Column<long>(type: "INTEGER", nullable: false),
					AppId = table.Column<uint>(type: "INTEGER", nullable: false),
					SteamAppEntryId = table.Column<int>(type: "INTEGER", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PlaytimeSliceDTO", x => x.PlaytimeSliceDTOId);
					table.ForeignKey(
						name: "FK_PlaytimeSliceDTO_SteamAppEntries_SteamAppEntryId",
						column: x => x.SteamAppEntryId,
						principalTable: "SteamAppEntries",
						principalColumn: "SteamAppEntryId");
				});

			migrationBuilder.CreateIndex(
				name: "IX_PlaytimeSliceDTO_SteamAppEntryId",
				table: "PlaytimeSliceDTO",
				column: "SteamAppEntryId");

			migrationBuilder.CreateIndex(
				name: "IX_SteamAppEntries_SteamAppDTOId",
				table: "SteamAppEntries",
				column: "SteamAppDTOId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "PlaytimeSliceDTO");

			migrationBuilder.DropTable(
				name: "SteamAppEntries");

			migrationBuilder.DropTable(
				name: "SteamAppDTO");
		}
	}
}
