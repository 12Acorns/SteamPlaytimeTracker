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

	public DbSet<SteamAppEntry> SteamAppEntries { get; private set; }
	public DbSet<SteamAppDTO> AllSteamApps { get; private set; }

	private static DbContextOptions<DbAccess> RefreshConnection()
	{
		ApplicationPath.TryAddPath(GlobalData.DbLookupName, "Steam Playtime Tracker", "appusage.db");
		return new DbContextOptionsBuilder<DbAccess>()
			.UseSqlite($"Data Source={ApplicationPath.GetPath(GlobalData.DbLookupName)}")
			.Options;
	}
}
