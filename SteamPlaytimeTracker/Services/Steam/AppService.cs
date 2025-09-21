using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.Utility.Cache;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.DbObject;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using SteamPlaytimeTracker.Utility;
using SteamPlaytimeTracker.Steam;
using SteamPlaytimeTracker.IO;
using OutParsing;
using System.Net;
using System.IO;
using Serilog;
using OneOf;

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
	private readonly IAsyncLifetimeService _lifetimeService;

	public AppService(DbAccess db, ILogger logger, ICacheManager cacheManager, IAsyncLifetimeService lifetimeService)
	{
		_db = db;
		_logger = logger;
		_cacheManager = cacheManager;
		_lifetimeService = lifetimeService;
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
		//var lookup = await _db.SteamApps
		//	.DistinctBy(x => x.AppId)
		//	.ToDictionaryAsync(x => x.AppId, token).ConfigureAwait(false);
		var seen = new ConcurrentDictionary<uint, byte>();
		var resultTasks = new ConcurrentBag<Task<OneOf<AppDetailsContainer, ParseResult, HttpStatusCode>>>();
		await foreach(var line in IOUtility.ReadLinesAsync(_tmpStorePath, token).ConfigureAwait(false))
		{
			if(token.IsCancellationRequested)
			{
				_logger.Information("Cancellation requested, stopping reading local apps.");
				break;
			}
			if(string.IsNullOrWhiteSpace(line))
			{
				continue;
			}
			if(!OutParser.TryParse(line, "[{date}] AppID {appId} adding PID {pidId} as a tracked process {appPath}",
				out string date, out uint appId, out int pidId, out string appPath))
			{
				continue;
			}
			// Already processed this app
			if(!seen.TryAdd(appId, 0))
			{
				continue;
			}
			// In future save app details to own dbset
			resultTasks.Add(SteamRequest.GetAppDetails(appId, _lifetimeService.CancellationToken).AsTask());
		};
		var results = await Task.WhenAll(resultTasks).ConfigureAwait(false);
		return results.Where(x =>
		{
			x.Switch(_ => { }, _ => { }, httpResponse =>
			{
				if(httpResponse is HttpStatusCode.TooManyRequests)
				{
					_logger.Warning("Too many requests when fetching app details.");
					_logger.Information("Adding app to re-fetch queue");
				}
			});
			return x.IsT0 && !string.IsNullOrWhiteSpace(x.AsT0.Details.Name);
		}).Select(x => new SteamApp() { AppId = x.AsT0.Details.Id, Name = x.AsT0.Details.Name });
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
