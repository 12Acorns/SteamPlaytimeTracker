using Microsoft.EntityFrameworkCore;
using OneOf;
using OutParsing;
using Serilog;
using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.IO;
using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Steam;
using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.Utility;
using SteamPlaytimeTracker.Utility.Cache;
using System.Collections.Concurrent;
using System.IO;
using System.Net;

namespace SteamPlaytimeTracker.Services.Steam;

internal sealed class AppService : IAppService
{
	private const int LocalAppCacheDurationMinutes = 15;

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

	public async ValueTask<List<SteamAppEntry>> AllEntries(CancellationToken token) => await _db.UserApps
			.AsNoTracking()
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
			return await _cacheManager.GetAsync(appId.ToString(), async () => 
				await _db.UserApps
						.AsNoTracking()
						.Include(x => x.StoreDetails)
							.ThenInclude(x => x.AppData)
							.ThenInclude(x => x.StoreData)
						.Include(x => x.PlaytimeSlices)
						.FirstOrDefaultAsync(x => x.StoreDetails.Id == appId, token).ConfigureAwait(false))
					.ConfigureAwait(false);
		}
		catch(Exception ex)
		{
			_logger.Error(ex, "Failed to get app {1} from Database. Error: {0}", appId);
			throw;
		}
	}

	public async ValueTask<IEnumerable<SteamStoreAppData>> GetLocalAppsAsync(CancellationToken token)
	{
		return await _cacheManager.GetAsync("LocalApps", LocalAppCacheDurationMinutes, async () =>
		{
			if(ApplicationPath.TryGetPath(GlobalData.MainTimeSliceCheckLookupName, out var primarySearchFile) && File.Exists(primarySearchFile))
			{
				return await GetLocalAppsPrimaryAsync(primarySearchFile, token).ConfigureAwait(false);
			}
			return [];
		}).ConfigureAwait(false);
	}
	private async ValueTask<IEnumerable<SteamStoreAppData>> GetLocalAppsPrimaryAsync(string searchFile, CancellationToken token) => await HandleTmpFileLifetimeAsync(searchFile, async tmpTile =>
	{
		var seen = new ConcurrentDictionary<uint, byte>();
		var resultTasks = new ConcurrentBag<Task<OneOf<SteamStoreAppData, ParseResult, HttpStatusCode>>>();
		await foreach(var line in IOUtility.ReadLinesAsync(tmpTile, cancellationToken: token).ConfigureAwait(false))
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
			return x.IsT0 && x.AsT0.Success && !string.IsNullOrWhiteSpace(x.AsT0.StoreData?.Name);
		}).Select(x => x.AsT0);
	}).ConfigureAwait(false);
	private async ValueTask<T> HandleTmpFileLifetimeAsync<T>(string path, Func<string, ValueTask<T>> asyncFunc)
	{
		string tmpFilePath = "";
		try
		{
			tmpFilePath = Path.Combine(ApplicationPath.GetPath(GlobalData.TmpFolderName), Guid.NewGuid().ToString());

			File.Copy(path, tmpFilePath);
			_logger.Information("Copied file to temporary location: {0}", tmpFilePath);
			return await asyncFunc(tmpFilePath).ConfigureAwait(false);
		}
		catch(Exception ex)
		{
			_logger.Error(ex, "Failed to copy file");
			throw;
		}
		finally
		{
			if(File.Exists(tmpFilePath))
			{
				File.Delete(tmpFilePath);
			}
			_logger.Information("Copied file to temporary location: {0}", tmpFilePath);
		}
	}
}
