using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamPlaytimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class SlimmedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SteamAppEntries_AllSteamApps_SteamAppDTOId",
                table: "SteamAppEntries");

            migrationBuilder.DropTable(
                name: "AllSteamApps");

            migrationBuilder.DropTable(
                name: "PlaytimeSliceDTO");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SteamAppEntries",
                table: "SteamAppEntries");

            migrationBuilder.DropIndex(
                name: "IX_SteamAppEntries_SteamAppDTOId",
                table: "SteamAppEntries");

            migrationBuilder.DropColumn(
                name: "SteamAppDTOId",
                table: "SteamAppEntries");

            migrationBuilder.RenameTable(
                name: "SteamAppEntries",
                newName: "LocalApps");

            migrationBuilder.RenameColumn(
                name: "SteamAppEntryId",
                table: "LocalApps",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LocalApps",
                table: "LocalApps",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "PlaytimeSlices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SessionStart = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    SessionLength = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    SteamAppEntryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaytimeSlices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaytimeSlices_LocalApps_SteamAppEntryId",
                        column: x => x.SteamAppEntryId,
                        principalTable: "LocalApps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SteamApps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AppId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    SteamAppEntryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamApps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SteamApps_LocalApps_SteamAppEntryId",
                        column: x => x.SteamAppEntryId,
                        principalTable: "LocalApps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaytimeSlices_SteamAppEntryId",
                table: "PlaytimeSlices",
                column: "SteamAppEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_SteamApps_SteamAppEntryId",
                table: "SteamApps",
                column: "SteamAppEntryId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaytimeSlices");

            migrationBuilder.DropTable(
                name: "SteamApps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LocalApps",
                table: "LocalApps");

            migrationBuilder.RenameTable(
                name: "LocalApps",
                newName: "SteamAppEntries");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "SteamAppEntries",
                newName: "SteamAppEntryId");

            migrationBuilder.AddColumn<int>(
                name: "SteamAppDTOId",
                table: "SteamAppEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SteamAppEntries",
                table: "SteamAppEntries",
                column: "SteamAppEntryId");

            migrationBuilder.CreateTable(
                name: "AllSteamApps",
                columns: table => new
                {
                    SteamAppDTOId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AppId = table.Column<uint>(type: "INTEGER", nullable: false),
                    AppName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllSteamApps", x => x.SteamAppDTOId);
                });

            migrationBuilder.CreateTable(
                name: "PlaytimeSliceDTO",
                columns: table => new
                {
                    PlaytimeSliceDTOId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AppId = table.Column<uint>(type: "INTEGER", nullable: false),
                    SessionTimeTicks = table.Column<long>(type: "INTEGER", nullable: false),
                    StartTimeOffsetMinutes = table.Column<short>(type: "INTEGER", nullable: false),
                    StartTimeTicks = table.Column<long>(type: "INTEGER", nullable: false),
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
                name: "IX_SteamAppEntries_SteamAppDTOId",
                table: "SteamAppEntries",
                column: "SteamAppDTOId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaytimeSliceDTO_SteamAppEntryId",
                table: "PlaytimeSliceDTO",
                column: "SteamAppEntryId");

            migrationBuilder.AddForeignKey(
                name: "FK_SteamAppEntries_AllSteamApps_SteamAppDTOId",
                table: "SteamAppEntries",
                column: "SteamAppDTOId",
                principalTable: "AllSteamApps",
                principalColumn: "SteamAppDTOId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
