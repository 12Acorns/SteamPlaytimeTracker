using Microsoft.EntityFrameworkCore;
using Serilog;
using SteamPlaytimeTracker.DbObject;
using System.IO;

namespace SteamPlaytimeTracker;

internal sealed class DbAccess : DbContext
{
	private static readonly string _dbPath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
		"Steam Playtime Tracker",
		"appusage.db");

	private readonly ILogger _logger;

	public DbAccess(ILogger logger)
	{
		_logger = logger;
		_logger.Information("Db Path: {0}", _dbPath);
	}

	public DbSet<SteamAppEntry> SteamAppEntries { get; private set; }

	protected override void OnConfiguring(DbContextOptionsBuilder options)
		=> options.UseSqlite($"Data Source={_dbPath}");
}
