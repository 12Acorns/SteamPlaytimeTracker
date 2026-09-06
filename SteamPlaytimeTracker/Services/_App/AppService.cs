using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.Utility.Cache;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.DbObject;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using SteamPlaytimeTracker.Utility;
using SteamPlaytimeTracker.IO;
using ValueTaskSupplement;
using OutParsing;
using System.Net;
using System.IO;
using Serilog;
using OneOf;
using SteamPlaytimeTracker.Services.Web.Steam;
using SteamPlaytimeTracker.Services.Disk;

namespace SteamPlaytimeTracker.Services._App;

internal sealed class AppService : IAppService
{
	private const int LocalAppCacheDurationMinutes = 15;

	private readonly ILocalSteamAppService _localSteamAppService;
	private readonly ILifetimeService _lifetimeService;
	private readonly ISteamWebService _steamWebService;
	private readonly ICacheManager _cacheManager;
	private readonly ILogger _logger;
	private readonly DbAccess _db;

	public AppService(DbAccess db, ILogger logger, ICacheManager cacheManager, ILifetimeService lifetimeService,
		ISteamWebService steamWebService, ILocalSteamAppService localSteamAppService)
	{
		_db = db;
		_logger = logger;
		_cacheManager = cacheManager;
		_lifetimeService = lifetimeService;
		_steamWebService = steamWebService;
		_localSteamAppService = localSteamAppService;
	}

	public async ValueTask<List<SteamAppEntry>> AllEntries(CancellationToken token) => await _db.UserApps
			.Include(x => x.StoreDetails)
				.ThenInclude(x => x.AppData)
				.ThenInclude(x => x.StoreData)
			.Include(x => x.PlaytimeSlices)
			.AsSplitQuery()
			.ToListAsync(token).ConfigureAwait(false);
	public async ValueTask<SteamAppEntry?> GetEntryAsync(uint appId, CancellationToken token)
	{
		try
		{
			return await _cacheManager.GetIfNotNullAsync($"GEA_{appId}", async token => 
				await _db.UserApps
						.Include(x => x.StoreDetails)
							.ThenInclude(x => x.AppData)
							.ThenInclude(x => x.StoreData)
						.Include(x => x.PlaytimeSlices)
						.AsSplitQuery()
						.FirstOrDefaultAsync(x => x.StoreDetails.Id == appId, token).ConfigureAwait(false), token: token)
					.ConfigureAwait(false);
		}
		catch(Exception ex)
		{
			_logger.Error(ex, "Failed to get app {0} from Database.", appId);
			throw;
		}
	}

	public async ValueTask<IEnumerable<SteamStoreAppData>> GetLocalAppsAsync(CancellationToken token)
	{
		return await _cacheManager.GetAsync("LocalApps", LocalAppCacheDurationMinutes, async token =>
		{
			if(ApplicationPath.TryGetPath(GlobalData.MainTimeSliceCheckLookupName, out var primarySearchFile) && File.Exists(primarySearchFile))
			{
				return await GetLocalAppsPrimaryAsync(primarySearchFile, token).ConfigureAwait(false);
			}
			return [];
		}, token: token).ConfigureAwait(false);
	}

	public async ValueTask<OneOf<SteamStoreAppData?, ParseResult, HttpStatusCode>> GetStoreAppDetailsAsync(uint appId, CancellationToken token = default) => 
		(await _steamWebService.GetAppDetails(appId, token).ConfigureAwait(false)).MapT0(appData => appData.Success ? appData : null);

	private async ValueTask<IEnumerable<SteamStoreAppData>> GetLocalAppsPrimaryAsync(string searchFile, CancellationToken token) => 
		await IOUtility.HandleTmpFileLifetimeAsync(searchFile, async tmpTile =>
	{
		var seen = new ConcurrentDictionary<uint, byte>();
		var resultTasks = new ConcurrentBag<ValueTask<OneOf<SteamStoreAppData, ParseResult, HttpStatusCode>>>();
		await foreach(var line in IOUtility.ReadLinesAsync(tmpTile, cancellationToken: token).ConfigureAwait(false))
		{
			if(token.IsCancellationRequested)
			{
				_logger.Debug("Cancellation requested, stopping reading local apps.");
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
			resultTasks.Add(_steamWebService.GetAppDetails(appId, _lifetimeService.CancellationToken));
		};
		var results = await ValueTaskEx.WhenAll(resultTasks).ConfigureAwait(false);
		return results.Where(x => 
		{
			x.Switch(_ => { }, _ => { }, httpResponse =>
			{
				if(httpResponse is HttpStatusCode.TooManyRequests)
				{
					_logger.Debug("Too many requests when fetching app details.");
					_logger.Debug("Adding app to re-fetch queue");
				}
			});
			return x.IsT0 && x.AsT0.Success && !string.IsNullOrWhiteSpace(x.AsT0.StoreData?.Name);
		}).Select(x => x.AsT0);
	}, cancellationToken: token).ConfigureAwait(false) ?? [];
}