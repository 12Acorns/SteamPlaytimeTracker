using Serilog;
using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Services.Playtime;
using SteamPlaytimeTracker.Steam.Data.Playtime;
using SteamPlaytimeTracker.Utility.Equality;
using System.Collections.Concurrent;

namespace SteamPlaytimeTracker.Services._App;

internal sealed class AppSynchronisationService : IAppSynchronisationService
{
	private readonly ILifetimeService _lifetimeService;
	private readonly IAppService _appService;
	private readonly DbAccess _steamDb;
	private readonly ILogger _logger;
	private readonly ConcurrentQueue<SteamAppEntry> _syncQueue = [];

	public AppSynchronisationService(DbAccess steamDb, IAppService appService, ILifetimeService lifetimeService, 
		ILogger logger)
	{
		_appService = appService;
		_lifetimeService = lifetimeService;
		_steamDb = steamDb;
		_logger = logger;
	}

	public void EnqueueForDbSync(SteamAppEntry entry)
	{
		if(_syncQueue.Contains(entry))
		{
			_logger.Warning("Attempted to enqueue {AppName} for DB sync, but it is already in the queue. Ignoring.", 
				entry.SteamApp?.Name ?? "NULL");
			return;
		}
		_syncQueue.Enqueue(entry);
	}
	public async Task CommitSyncToDbAsync(CancellationToken token)
	{
		try
		{
			var allEntriesLookup = (await _appService.AllEntries(_lifetimeService.CancellationToken).ConfigureAwait(false))
				.ToDictionary(x => x.SteamApp!.AppId);
			var appsToSync = new List<SteamAppEntry>();
			while(_syncQueue.TryDequeue(out var rawEntry))
			{
				_lifetimeService.CancellationToken.ThrowIfCancellationRequested();
				if(rawEntry.SteamApp is null)
				{
					continue;
				}
				if(!allEntriesLookup.TryGetValue(rawEntry.SteamApp.AppId, out var dbEntry))
				{
					_steamDb.UserApps.Add(rawEntry);
					appsToSync.Add(rawEntry);
					continue;
				}
				dbEntry.StoreDetails = rawEntry.StoreDetails;
				if(SequencesEqual(dbEntry.PlaytimeSlices, rawEntry.PlaytimeSlices, PlaytimeSliceEquality.Instance))
				{
					continue;
				}
				var uniqueSegments = rawEntry.PlaytimeSlices.Except(dbEntry.PlaytimeSlices, PlaytimeSliceEquality.Instance).ToList();
				if(uniqueSegments.Count is 0)
				{
					_steamDb.UserApps.Update(dbEntry);
					appsToSync.Add(dbEntry);
					continue;
				}
				_steamDb.PlaytimeSlices.AddRange(uniqueSegments);
				dbEntry.PlaytimeSlices.AddRange(uniqueSegments);
				_steamDb.UserApps.Update(dbEntry);
				appsToSync.Add(dbEntry);
				_logger.Verbose("Updated playtime segments for app: {AppName} (AppID: {AppId}) with {SegmentCount} new segments.",
					dbEntry.SteamApp!.Name, dbEntry.SteamApp.AppId, uniqueSegments.Count);
			}
			await _steamDb.SaveChangesAsync(_lifetimeService.CancellationToken).ConfigureAwait(false);
		}
		catch(Exception ex) when(ex is not OperationCanceledException)
		{
			_logger.Error(ex, "Failed to sync apps to database");
		}
	}
	private static bool SequencesEqual(IEnumerable<PlaytimeSlice> first, IEnumerable<PlaytimeSlice> second, IEqualityComparer<PlaytimeSlice> comparer) =>
		first.OrderBy(x => x.SessionStart).Aggregate(0, HashCode.Combine) == second.OrderBy(x => x.SessionStart).Aggregate(0, HashCode.Combine);
}