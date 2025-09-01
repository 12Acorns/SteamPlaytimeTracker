using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.Utility.Cache;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.DbObject;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using SteamPlaytimeTracker.Utility;
using SteamPlaytimeTracker.IO;
using OutParsing;
using System.IO;
using Serilog;

namespace SteamPlaytimeTracker.Services.Steam;

internal sealed class AppService : IAppService
{
	private const int LocalAppCacheDurationMinutes = 15;

	private static readonly string _tmpStorePath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
		"Steam Playtime Tracker",
		"tmp.txt");

	private readonly DbAccess _db;
	private readonly ILogger _logger;
	private readonly ICacheManager _cacheManager;

	public AppService(DbAccess db, ILogger logger, ICacheManager cacheManager)
	{
		_db = db;
		_logger = logger;
		_cacheManager = cacheManager;
	}

	public async ValueTask<SteamAppEntry?> GetEntryAsync(uint appId, CancellationToken token)
	{
		try
		{
			return await _cacheManager.Get<ValueTask<SteamAppEntry?>>(appId.ToString(), async () => 
				await _db.LocalApps
						.Include(x => x.SteamApp)
						.Include(x => x.PlaytimeSlices)
						.FirstOrDefaultAsync(x => x.SteamApp.AppId == appId, token).ConfigureAwait(false))
					.ConfigureAwait(false);
		}
		catch(Exception ex)
		{
			_logger.Error(ex, "Failed to get app {1} from Database. Error: {0}", appId);
			throw;
		}
	}

	public async ValueTask<IEnumerable<SteamApp>> GetLocalAppsAsync(CancellationToken token)
	{
		return await _cacheManager.Get<ValueTask<IEnumerable<SteamApp>>>("LocalApps", LocalAppCacheDurationMinutes, async () =>
		{
			if(ApplicationPath.TryGetPath(GlobalData.MainTimeSliceCheckLookupName, out var primarySearchFile) && File.Exists(primarySearchFile))
			{
				return await GetLocalAppsPrimaryAsync(primarySearchFile, token).ConfigureAwait(false);
			}
			return [];
		}).ConfigureAwait(false);
	}
	private async ValueTask<IEnumerable<SteamApp>> GetLocalAppsPrimaryAsync(string searchFile, CancellationToken token) => await HandleTmpFileLifetimeAsync(searchFile, async () =>
	{
		var lookup = await _db.SteamApps
			.GroupBy(x => x.AppId).Select(x => x.First())
			.ToDictionaryAsync(x => x.AppId, token).ConfigureAwait(false);
		var seen = new HashSet<uint>();
		var results = new ConcurrentBag<SteamApp>();
		await foreach(var line in IOUtility.ReadLinesAsync(_tmpStorePath, token).ConfigureAwait(false))
		{
			if(token.IsCancellationRequested)
			{
				_logger.Information("Cancellation requested, stopping reading local apps.");
				break;
			}
			if(line == string.Empty)
			{
				continue;
			}
			if(!OutParser.TryParse(line, "[{date}] AppID {appId} adding PID {pidId} as a tracked process {appPath}",
				out string date, out uint appId, out int pidId, out string appPath))
			{
				continue;
			}
			if(!seen.Add(appId))
			{
				continue;
			}
			var appName = "n/a";
			if(lookup.TryGetValue(appId, out var app))
			{
				appName = app.Name;
			}
			else
			{
				continue;
			}
			if(token.IsCancellationRequested)
			{
				_logger.Information("Cancellation requested, stopping reading local apps.");
				break;
			}
			results.Add(new SteamApp { AppId = appId, Name = appName });
		}
		return results;
	}).ConfigureAwait(false);
	private async ValueTask<T> HandleTmpFileLifetimeAsync<T>(string path, Func<ValueTask<T>> asyncFunc)
	{
		try
		{
			if(File.Exists(_tmpStorePath))
			{
				File.Delete(_tmpStorePath);
			}
			File.Copy(path, _tmpStorePath);
		}
		catch(Exception ex)
		{
			if(File.Exists(_tmpStorePath))
			{
				File.Delete(_tmpStorePath);
			}
			_logger.Error(ex, "Failed to copy file: {0}");
			throw;
		}
		_logger.Information("Copied file to temporary location: {0}", _tmpStorePath);

		try
		{
			return await asyncFunc().ConfigureAwait(false);
		}
		finally
		{
			if(File.Exists(_tmpStorePath))
			{
				File.Delete(_tmpStorePath);
			}
			_logger.Information("Copied file to temporary location: {0}", _tmpStorePath);
		}
	}
}
