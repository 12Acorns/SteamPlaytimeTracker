using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamPlaytimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class I_Hate_DB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SteamAppStoreDetails_AgeUnion_AgeUnionId",
                table: "SteamAppStoreDetails");

            migrationBuilder.DropTable(
                name: "AgeUnion");

            migrationBuilder.DropIndex(
                name: "IX_SteamAppStoreDetails_AgeUnionId",
                table: "SteamAppStoreDetails");

            migrationBuilder.RenameColumn(
                name: "AgeUnionId",
                table: "SteamAppStoreDetails",
                newName: "Age");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Age",
                table: "SteamAppStoreDetails",
                newName: "AgeUnionId");

            migrationBuilder.CreateTable(
                name: "AgeUnion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Age = table.Column<int>(type: "INTEGER", nullable: false),
                    AgeLiteral = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgeUnion", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SteamAppStoreDetails_AgeUnionId",
                table: "SteamAppStoreDetails",
                column: "AgeUnionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SteamAppStoreDetails_AgeUnion_AgeUnionId",
                table: "SteamAppStoreDetails",
                column: "AgeUnionId",
                principalTable: "AgeUnion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
