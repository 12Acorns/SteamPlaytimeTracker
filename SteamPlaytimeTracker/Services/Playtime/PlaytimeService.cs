using OutParsing;
using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.IO;
using SteamPlaytimeTracker.Services.Messaging;
using SteamPlaytimeTracker.Steam.Data.Playtime;
using SteamPlaytimeTracker.Utility;
using SteamPlaytimeTracker.Utility.Cache;
using SteamPlaytimeTracker.Utility.Messaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SteamPlaytimeTracker.Services.Playtime;

internal sealed class PlaytimeService : IPlaytimeService
{
	private readonly ICacheManager _cacheManager;
	private readonly IMessageExchangeService _messageExchangeService;

	public PlaytimeService(ICacheManager cacheManager, IMessageExchangeService messageExchangeService)
	{
		_cacheManager = cacheManager;
		_messageExchangeService = messageExchangeService;
	}

	public async ValueTask<Dictionary<uint, List<PlaytimeSlice>>> GetPlayimeIntervalsMap(CancellationToken cancellationToken = default)
	{
		return await _cacheManager.GetAsync("PlaytimeCache", 15, async token =>
		{
			try
			{
				var primaryIntervals = GetPlaytimeSegmentsPrimary(cancellationToken);
				if(primaryIntervals is null)
				{
					LoggingService.Logger.Warning("No playtime segments could be retrieved from the primary source.");
					return [];
				}
				LoggingService.Logger.Information("Playtime segments successfully retrieved from the primary source.");
				var parsedIntervals = new Dictionary<uint, List<PlaytimeSlice>>();
				await foreach(var interval in primaryIntervals.WithCancellation(cancellationToken).ConfigureAwait(false))
				{
					ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault(parsedIntervals, interval.AppId, out var exists);
					if(!exists)
					{
						list = [];
					}
					list!.Add(interval);
				}
				foreach(var intervals in parsedIntervals)
				{
					parsedIntervals[intervals.Key] = PlaytimeUtility.CorrectIntervalOverlap(intervals.Value);
				}
				return parsedIntervals;
			}
			catch(Exception ex) when(ex is not OperationCanceledException)
			{
				LoggingService.Logger.Error(ex, "An error occurred while retrieving playtime segments from the primary source.");
				return [];
			}
		}, token: cancellationToken);
	}
	private async IAsyncEnumerable<PlaytimeSlice> GetPlaytimeSegmentsPrimary([EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var result = (await IOUtility.HandleTmpFileLifetimeAsyncEnumerable(ApplicationPath.GetPath(GlobalData.MainTimeSliceCheckLookupName), 
			filePath => ProcessFile(filePath, cancellationToken), cancellationToken: cancellationToken).ConfigureAwait(false)).DoIfError(failure =>
		{
			if(failure.FailureType is not IOUtility.IOFailure.IOFailureType.Copy)
			{
				return;
			}
			_messageExchangeService.AddMessage(new Message(MessageType.Error, "File Access",
				$"Failed to read required files to generate playtime segments, " +
				$"please ensure Steam is fully shut down before using the application. " +
				$"After closing Steam, restart the application.\nFull Error: {failure.FailureException?.Message ?? ""}"));
		}).DefaultWith(_ => AsyncEnumerable.Empty<PlaytimeSlice>());
		await foreach(var item in result.WithCancellation(cancellationToken).ConfigureAwait(false))
		{
			yield return item;
		}
		static async IAsyncEnumerable<PlaytimeSlice> ProcessFile(string filePath, [EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			var startDates = new Stack<(string Date, GameKey Key)>();
			var pendingDates = new Dictionary<GameKey, Stack<string>>();
			await foreach(var line in IOUtility.ReadLinesAsync(filePath, cancellationToken: cancellationToken).ConfigureAwait(false))
			{
				if(OutParser.TryParse(line, "[{startDate}] AppID {appId} adding PID {pidId} as a tracked process {appPath}",
					out string startDate, out uint appId, out int pidId, out string appPath))
				{
					if(startDates.TryPeek(out var currentStart) && currentStart.Key == new GameKey(appId, pidId))
					{
						continue;
					}
					if(startDates.Count > 0)
					{
						var (pendingStartDate, pendingKey) = startDates.Pop();
						ref var pendingStack = ref CollectionsMarshal.GetValueRefOrAddDefault(pendingDates, pendingKey, out var exists)!;
						if(!exists)
						{
							pendingStack = [];
						}
						pendingStack.Push(pendingStartDate);
					}
					startDates.Push((startDate, new(appId, pidId)));
				}
				else if(OutParser.TryParse(line, "[{endDate}] AppID {appIdE} no longer tracking PID {pidIdE}, exit code {exitCode}",
					out string endDate, out uint appIdE, out int pidIdE, out int exitCode))
				{
					if(startDates.Count == 0)
					{
						continue;
					}
					(startDate, GameKey key) = startDates.Pop();
					var endKey = new GameKey(appIdE, pidIdE);
					if(endKey != key)
					{
						startDates.Push((startDate, endKey));
						if(!pendingDates.TryGetValue(endKey, out var pendingStack) || pendingStack.Count <= 0)
						{
							continue;
						}
						startDate = pendingStack.Pop();
						if(pendingStack.Count == 0)
						{
							pendingDates.Remove(endKey);
						}
					}
					if(PlaytimeSlice.TryCreate(startDate, endDate, endKey.AppId, out var slice))
					{
						yield return slice;
					}
				}
			}
			IOUtility.TryDeleteFile(filePath);
			yield break;
		}
	}
	private readonly record struct GameKey(uint AppId, int Pid);
}