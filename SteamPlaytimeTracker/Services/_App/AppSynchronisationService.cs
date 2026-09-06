using Serilog;
using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Services.Messaging;
using SteamPlaytimeTracker.Steam.Data.Playtime;
using SteamPlaytimeTracker.Utility.Equality;
using SteamPlaytimeTracker.Utility.Messaging;
using System.Collections.Concurrent;

namespace SteamPlaytimeTracker.Services._App;

internal sealed class AppSynchronisationService : IAppSynchronisationService
{
	private static readonly MessageCatagory _enqueueSyncCatagory = MessageCatagoryBuilder.Parse("Queue.DB.Sync")!;
	private static readonly MessageCatagory _dbSyncCatagory = MessageCatagoryBuilder.Parse("DB.Sync")!;

	private readonly IMessageExchangeService _messageExchangeService;
	private readonly ILifetimeService _lifetimeService;
	private readonly IAppService _appService;
	private readonly DbAccess _steamDb;
	private readonly ILogger _logger;
	private readonly ConcurrentQueue<SteamAppEntry> _syncQueue = [];

	public AppSynchronisationService(DbAccess steamDb, IAppService appService, ILifetimeService lifetimeService, IMessageExchangeService messageExchangeService,
		ILogger logger)
	{
		_appService = appService;
		_lifetimeService = lifetimeService;
		_messageExchangeService = messageExchangeService;
		_steamDb = steamDb;
		_logger = logger;
	}

	public void EnqueueForDbSync(SteamAppEntry entry)
	{
		if(_syncQueue.Contains(entry))
		{
			_logger.Warning("Attempted to enqueue {AppName} for DB sync, but it is already in the queue. Ignoring.", 
				entry.SteamApp?.Name ?? "NULL");
			_messageExchangeService.AddMessage(new Message(MessageType.Error, _enqueueSyncCatagory, "Failed to enqueue app onto db for syncronization. Duplicate found."));
			return;
		}
		_syncQueue.Enqueue(entry);
	}
	public async Task CommitSyncToDbAsync(CancellationToken token)
	{
		try
		{
			var allEntriesLookup = (await _appService.AllEntries(_lifetimeService.CancellationToken).ConfigureAwait(false))
				.Where(x => x.SteamApp is not null)
				.ToDictionary(x => x.SteamApp!.AppId);
			List<PlaytimeSlice> toAdd = [];
			while(_syncQueue.TryDequeue(out var rawEntry))
			{
				_lifetimeService.CancellationToken.ThrowIfCancellationRequested();
				if(!allEntriesLookup.TryGetValue(rawEntry.SteamApp!.AppId, out var dbEntry))
				{
					_steamDb.Add(rawEntry);
					_steamDb.Add(rawEntry.StoreDetails);
					_steamDb.Add(rawEntry.StoreDetails.AppData);
					_steamDb.Add(rawEntry.StoreDetails.AppData.StoreData!);
					continue;
				}
				dbEntry.StoreDetails = rawEntry.StoreDetails;
				if(PlaytimeSliceEquality.SequencesEqual(dbEntry.PlaytimeSlices, rawEntry.PlaytimeSlices))
				{
					continue;
				}
				var uniqueSegments = rawEntry.PlaytimeSlices.Except(dbEntry.PlaytimeSlices, PlaytimeSliceEquality.Instance).ToList();
				if(uniqueSegments.Count is 0)
				{
					continue;
				}
				uniqueSegments.ForEach(x => x.SteamAppEntry = dbEntry);
				toAdd.AddRange(uniqueSegments);
				_steamDb.Update(dbEntry);
				_steamDb.Update(dbEntry.StoreDetails);
				_steamDb.Update(dbEntry.StoreDetails.AppData);
				_steamDb.Update(dbEntry.StoreDetails.AppData.StoreData!);
				_logger.Verbose("Queued playtime intervals for app: {AppName} (AppID: {AppId}) with {SegmentCount} new segments for sync",
					dbEntry.SteamApp!.Name, dbEntry.SteamApp.AppId, uniqueSegments.Count);
			}
			await _steamDb.SaveChangesAsync(_lifetimeService.CancellationToken).ConfigureAwait(false);
			_steamDb.PlaytimeSlices.AddRange(toAdd);
			await _steamDb.SaveChangesAsync(_lifetimeService.CancellationToken).ConfigureAwait(false);
		}
		catch(Exception ex) when(ex is not OperationCanceledException)
		{
			_logger.Error(ex, "Failed to sync apps to database");
			_messageExchangeService.AddMessage(new Message(MessageType.Error, _dbSyncCatagory, $"Failed to sync apps to database. Error: {ex}"));
		}
	}
}