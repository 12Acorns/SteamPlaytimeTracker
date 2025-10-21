using SteamPlaytimeTracker.Steam.Data.Playtime;
using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.DbObject;
using Microsoft.EntityFrameworkCore;
using SteamPlaytimeTracker.IO;
using Serilog;

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

	public DbSet<PlaytimeSlice> PlaytimeSlices { get; private set; }
	public DbSet<SteamAppEntry> UserApps { get; private set; }
	public DbSet<SteamStoreApp> SteamStoreApps { get; private set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<SteamAppEntry>()
			.HasMany(e => e.PlaytimeSlices)
			.WithOne(e => e.SteamAppEntry)
			.HasForeignKey("SteamAppEntryId")
			.IsRequired();
		modelBuilder.Entity<SteamAppEntry>()
			.HasOne(e => e.StoreDetails)
			.WithOne()
			.HasForeignKey<SteamAppEntry>("SteamStoreAppId")
			.IsRequired(false);
		modelBuilder.Entity<SteamStoreApp>()
			.HasOne(e => e.AppData)
			.WithOne()
			.HasForeignKey<SteamStoreApp>("SteamStoreAppDataId")
			.IsRequired();
		modelBuilder.Entity<SteamStoreAppData>()
			.HasOne(e => e.StoreData)
			.WithOne()
			.HasForeignKey<SteamStoreAppData>("SteamAppStoreDetailsId")
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
