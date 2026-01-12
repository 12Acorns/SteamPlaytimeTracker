using SteamPlaytimeTracker.Steam.Data.Playtime;
using SteamPlaytimeTracker.Utility;
using SteamPlaytimeTracker.IO;
using OutParsing;
using System.IO;
using SteamPlaytimeTracker.DbObject;
using System.Globalization;
using SteamPlaytimeTracker.Utility.Cache;
using SteamPlaytimeTracker.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace SteamPlaytimeTracker.Services.Playtime;

internal sealed class PlaytimeService : IPlaytimeService
{
	private readonly ICacheManager _cacheManager;

	public PlaytimeService(ICacheManager cacheManager)
	{
		_cacheManager = cacheManager;
	}

	public async ValueTask<Dictionary<uint, List<PlaytimeSlice>>> GetPlayimeSegments(CancellationToken cancellationToken = default)
	{
		return await _cacheManager.GetAsync("PlaytimeCache", 15, async token =>
		{
			try
			{
				var res = GetPlaytimeSegmentsPrimary(cancellationToken);
				if(res is null)
				{
					LoggingService.Logger.Warning("No playtime segments could be retrieved from the primary source.");
					return [];
				}
				LoggingService.Logger.Information("Playtime segments successfully retrieved from the primary source.");
				return await res.GroupBy(x => x.AppId).ToDictionaryAsync(x => x.Key, y => y.ToList(), cancellationToken: token).ConfigureAwait(false);
			}
			catch(Exception ex)
			{
				LoggingService.Logger.Error(ex, "An error occurred while retrieving playtime segments from the primary source.");
				return [];
			}
		}, token: cancellationToken);
	}
	private static IAsyncEnumerable<PlaytimeSlice> GetPlaytimeSegmentsPrimary(CancellationToken cancellationToken = default)
	{
		return IOUtility.HandleTmpFileLifetimeAsyncEnumerable(ApplicationPath.GetPath(GlobalData.MainTimeSliceCheckLookupName), 
			filePath => ProcessFile(filePath, cancellationToken), cancellationToken: cancellationToken);
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
							pendingStack = new Stack<string>();
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
			yield break;
		}
	}

	private static async Task<IEnumerable<IGrouping<uint, PlaytimeSlice>>?> GetSegmentsFromPrimary_OLD(CancellationToken cancellationToken = default) =>
		await Task.Run(() => IOUtility.HandleTmpFileLifetime(
		ApplicationPath.GetPath(GlobalData.MainTimeSliceCheckLookupName), filePath =>
		{
			var dates = new List<(string Date, uint AppId, bool IsEnd)>();

			var segments = new List<PlaytimeSliceDTO>(capacity: 120);
			foreach(var line in File.ReadAllLines(filePath))
			{
				if(OutParser.TryParse(line, "[{startDate}] AppID {appIdS} adding PID {pidIdS} as a tracked process {appPath}",
					out string startDate, out uint appIdS, out int pidIdS, out string appPath))
				{
					dates.Add((startDate, appIdS, false));
					continue;
				}
				// Remove any dangling start dates that don't have an end date, do not know if the whitespace indicates a new steam session
				// so assume it does
				if(string.IsNullOrWhiteSpace(line))
				{
					dates.RemoveLastWhile(date => !date.IsEnd);
					continue;
				}
				if(OutParser.TryParse(line, "[{endDate}] AppID {appId} no longer tracking PID {pidId}, exit code {exitCode}",
					out string endDate, out uint appId, out int pidId, out int exitCode))
				{
					dates.Add((endDate, appId, true));
				}
			}

			if(dates.Count % 2 != 0)
			{
				dates.RemoveLastWhile(date => !date.IsEnd);
			}
			var groupedDates = dates.GroupBy(x => x.AppId);
			List<IEnumerable<(string Date, uint AppId, bool IsEnd)>> groupedSegments = [];
			foreach(var group in groupedDates)
			{
				List<(string Date, uint AppId, bool IsEnd)> localSegments = [];
				// Thankfully order is preserved when doing groupings
				var items = group.ToArray();
				int index = 0;
				while(index < items.Length)
				{
					if(items[index].IsEnd)
					{
						localSegments.Add(items[index]);
						index++;
						continue;
					}
					var startIdx = index;
					var first = items[index];
					while(index + 1 < items.Length && items[index + 1].AppId == first.AppId && !items[index + 1].IsEnd)
					{
						index++;
					}
					localSegments.Add(first);

					index++;
				}
				var distinctSegments = localSegments.DistinctBy(static x => x.Date).ToList();
				distinctSegments.RemoveLastWhile(distinct => !distinct.IsEnd);
				int idx = distinctSegments.Count - 1;
				while(idx >= 0)
				{
					while(idx > 0 && distinctSegments[idx].IsEnd && distinctSegments[idx - 1].IsEnd)
					{
						distinctSegments.RemoveAt(idx);
						idx--;
					}
					idx--;
				}
				groupedSegments.Add(distinctSegments);
			}
			// I believe select many maintains order
			return groupedSegments.Select(static y => y.Chunk(2).Select(static x =>
			{
				if(x.Length != 2)
				{
					throw new InvalidOperationException("Playtime segments must be in pairs of start and end dates.");
				}

				var startDateOffset = DateTimeOffset.ParseExact(
						x[0].Date,
						"yyyy-MM-dd HH:mm:ss",
						CultureInfo.InvariantCulture,
						DateTimeStyles.AssumeLocal);
				var endDateOffset = DateTimeOffset.ParseExact(
					x[1].Date,
					"yyyy-MM-dd HH:mm:ss",
					CultureInfo.InvariantCulture,
					DateTimeStyles.AssumeLocal);
				var dateDelta = endDateOffset - startDateOffset;
				return new PlaytimeSlice { SessionStart = startDateOffset, SessionLength = dateDelta, AppId = x[0].AppId };
			})).SelectMany(static x => x).GroupBy(static x => x.AppId);
		})).ConfigureAwait(false);

	private readonly record struct GameKey(uint AppId, int Pid);
}