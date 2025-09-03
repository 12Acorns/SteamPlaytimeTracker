using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.DbObject;
using Microsoft.EntityFrameworkCore;
using SteamPlaytimeTracker.IO;
using Serilog;
using SteamPlaytimeTracker.Steam.Data.Playtime;
using System.Diagnostics;

namespace SteamPlaytimeTracker;

internal sealed class DbAccess : DbContext
{
	private readonly ILogger _logger;

	public DbAccess(DbContextOptions<DbAccess> options, ILogger logger) : base(options)
	{
		_logger = logger;
		_logger.Information("Db Path: {0}", ApplicationPath.GetPath("Db"));
	}
	public DbAccess() : this(
		RefreshConnection(),
		LoggingService.Logger) { }

	public DbSet<PlaytimeSlice> PlaytimeSlices { get; set; }
	public DbSet<SteamAppEntry> LocalApps { get; private set; }
	public DbSet<SteamApp> SteamApps { get; private set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<SteamAppEntry>()
			.HasMany(e => e.PlaytimeSlices)
			.WithOne(e => e.SteamAppEntry)
			.HasForeignKey("SteamAppEntryId")
			.IsRequired();
		modelBuilder.Entity<SteamAppEntry>()
			.HasOne(e => e.SteamApp)
			.WithOne()
			.HasForeignKey<SteamAppEntry>("SteamAppId")
			.IsRequired();
	}

	private static DbContextOptions<DbAccess> RefreshConnection()
	{
		ApplicationPath.TryAddPath(GlobalData.DbLookupName, "Steam Playtime Tracker", "appusage.db");
		return new DbContextOptionsBuilder<DbAccess>()
			.UseSqlite($"Data Source={ApplicationPath.GetPath(GlobalData.DbLookupName)}")
			.Options;
	}
}
