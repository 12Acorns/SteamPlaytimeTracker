using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamPlaytimeTracker.Migrations
{
    /// <inheritdoc />
    public partial class Nullable_Store_App : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql("PRAGMA foreign_keys = OFF;", suppressTransaction: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Sql("PRAGMA foreign_keys = ON;", suppressTransaction: true);
		}
	}
}
