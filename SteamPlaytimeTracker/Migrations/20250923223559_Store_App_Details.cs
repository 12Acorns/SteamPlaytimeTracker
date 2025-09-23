using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamPlaytimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class Store_App_Details : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocalApps_SteamApps_SteamAppId",
                table: "LocalApps");

            migrationBuilder.RenameColumn(
                name: "SteamAppId",
                table: "LocalApps",
                newName: "SteamStoreAppId");

            migrationBuilder.RenameIndex(
                name: "IX_LocalApps_SteamAppId",
                table: "LocalApps",
                newName: "IX_LocalApps_SteamStoreAppId");

            migrationBuilder.CreateTable(
                name: "SteamAppStoreDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    AppType = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    AppId = table.Column<uint>(type: "INTEGER", nullable: false),
                    Age = table.Column<int>(type: "INTEGER", nullable: false),
                    IsFree = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamAppStoreDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SteamStoreAppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false),
                    StoreDataId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamStoreAppData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SteamStoreAppData_SteamAppStoreDetails_StoreDataId",
                        column: x => x.StoreDataId,
                        principalTable: "SteamAppStoreDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SteamStoreApps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    AppDataId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SteamStoreApps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SteamStoreApps_SteamStoreAppData_AppDataId",
                        column: x => x.AppDataId,
                        principalTable: "SteamStoreAppData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SteamStoreAppData_StoreDataId",
                table: "SteamStoreAppData",
                column: "StoreDataId");

            migrationBuilder.CreateIndex(
                name: "IX_SteamStoreApps_AppDataId",
                table: "SteamStoreApps",
                column: "AppDataId");

            migrationBuilder.AddForeignKey(
                name: "FK_LocalApps_SteamStoreApps_SteamStoreAppId",
                table: "LocalApps",
                column: "SteamStoreAppId",
                principalTable: "SteamStoreApps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocalApps_SteamStoreApps_SteamStoreAppId",
                table: "LocalApps");

            migrationBuilder.DropTable(
                name: "SteamStoreApps");

            migrationBuilder.DropTable(
                name: "SteamStoreAppData");

            migrationBuilder.DropTable(
                name: "SteamAppStoreDetails");

            migrationBuilder.RenameColumn(
                name: "SteamStoreAppId",
                table: "LocalApps",
                newName: "SteamAppId");

            migrationBuilder.RenameIndex(
                name: "IX_LocalApps_SteamStoreAppId",
                table: "LocalApps",
                newName: "IX_LocalApps_SteamAppId");

            migrationBuilder.AddForeignKey(
                name: "FK_LocalApps_SteamApps_SteamAppId",
                table: "LocalApps",
                column: "SteamAppId",
                principalTable: "SteamApps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
