using SteamPlaytimeTracker.Steam.Data.Playtime;
using SteamPlaytimeTracker.DbObject;
using System.Globalization;
using OutParsing;
using System.IO;
using SteamPlaytimeTracker.Utility;

namespace SteamPlaytimeTracker.IO;

internal static class PlaytimeProvider
{
	public static Dictionary<uint, List<PlaytimeSlice>> GetPlayimeSegments()
	{
		var res = GetSegmentsFromPrimary();
		if(res == null)
		{
			LoggingService.Logger.Warning("No playtime segments could be retrieved from the primary source.");
			return [];
		}
		LoggingService.Logger.Information("Playtime segments successfully retrieved from the primary source.");
		return res.ToDictionary(static x => x.Key, static x => x.ToList());
	}

	// Lord forgive, I promose I will refactor anothertime
	// May the tech debt be forgiving 🙏
	private static IEnumerable<IGrouping<uint, PlaytimeSlice>>? GetSegmentsFromPrimary() => IOUtility.HandleTmpFileLifetime(
		ApplicationPath.GetPath(GlobalData.MainTimeSliceCheckLookupName), filePath =>
	{
		// New plan to collect playtime in a cleaner way
		// Collect all segments in a List<List<(...)>>
		// Eacg inner list is the playtime within a block before the whitespace seperation
		// Each entry in the inner list is either a start or end date
		// In each inner list, remove all end dates from the start until there is a start date
		// Go into loop, while there are still entries and is for the same app, and the date is an end date keep iterating
		// Once a new start date is found look back to the last end date
		// Create a new segment from the start date to the end date
		// Repeat until all segments have been processed

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
			if(string.IsNullOrWhiteSpace(line))
			{
				if(dates.Count == 0)
				{
					continue;
				}
				while(dates.Count > 0)
				{
					var (_, _, isEnd) = dates[^1];
					if(!isEnd)
					{
						dates.RemoveAt(dates.Count - 1);
					}
					break;
				}
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
			while(dates.Count > 0)
			{
				var (_, _, isEnd) = dates[^1];
				if(!isEnd)
				{
					dates.RemoveAt(dates.Count - 1);
				}
				break;
			}
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
			var distinct = localSegments.DistinctBy(static x => x.Date).ToList();
			while(distinct.Count > 0)
			{
				var (_, _, isEnd) = distinct[^1];
				if(!isEnd)
				{
					distinct.RemoveAt(distinct.Count - 1);
				}
				break;
			}
			int idx = distinct.Count - 1;
			while(idx >= 0)
			{
				while(idx - 1 >= 0 && distinct[idx].IsEnd && distinct[idx - 1].IsEnd)
				{
					distinct.RemoveAt(idx);
					idx--;
				}
				idx--;
			}
			groupedSegments.Add(distinct);
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
	});
}